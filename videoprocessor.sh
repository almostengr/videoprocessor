#!/bin/bash

PATH="/usr/bin/:/bin:/usr/sbin:/sbin:${PATH}"

BASE_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/videos"
DEBUG=1

INCOMING_DIRECTORY="${BASE_DIRECTORY}/incoming"
PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processed"
ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive"
ACTIVE_FILE="${BASE_DIRECTORY}/.active.txt"
MIX_AUDIO_TRACK_FILE="${MUSIC_DIRECTORY}mix07.mp3"

PADDING=70 # Padding for the graphics from the end of the screen
UPPERLEFT="x=${PADDING}:y=${PADDING}"
UPPERCENTER="x=(w-tw)/2:y=${PADDING}"
UPPERRIGHT="x=w-tw-${PADDING}:y=${PADDING}"
CENTERED="x=(w-tw)/2:y=(h-th)/2"
LOWERLEFT="x=${PADDING}:y=h-th-${PADDING}"
LOWER_LEFT1="x=${PADDING}:y=h-th-${PADDING}-50"
LOWERCENTER="x=(w-tw)/2:y=h-th-${PADDING}"
LOWERRIGHT="x=w-tw-${PADDING}:y=h-th-${PADDING}"

videoDirectory=""
TIMESTAMP=$(date +'%Y%m%d.%H%M%S')
LOG_DIRECTORY="/home/almostengr/Documents/videoprocessor"
LOG_FILE="${LOG_DIRECTORY}/${TIMESTAMP}.log"
dayOfWeek=$(date +%u)

FINAL_OUTPUT_VERTICAL="outputVerticalFinal.mp4"
FINAL_OUTPUT_HORIZONTAL="outputFinal.mp4"

selectMixTrack()
{
    MUSIC_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ytvideostructure/07music/"

    case $dayOfWeek in
        0)
	    MIX_AUDIO_TRACK_FILE="${MUSIC_DIRECTORY}mix01.mp3"
        ;;

        1)
	    MIX_AUDIO_TRACK_FILE="${MUSIC_DIRECTORY}mix02.mp3"
        ;;

        2)
	    MIX_AUDIO_TRACK_FILE="${MUSIC_DIRECTORY}mix03.mp3"
        ;;

        3)
	    MIX_AUDIO_TRACK_FILE="${MUSIC_DIRECTORY}mix04.mp3"
        ;;

        4)
	    MIX_AUDIO_TRACK_FILE="${MUSIC_DIRECTORY}mix05.mp3"
        ;;

        5)
	    MIX_AUDIO_TRACK_FILE="${MUSIC_DIRECTORY}mix06.mp3"
        ;;

        *)
	    MIX_AUDIO_TRACK_FILE="${MUSIC_DIRECTORY}mix07.mp3"
        ;;
    esac
}

errorMessage()
{
    echo "ERROR $(date) $1" | tee -a "${LOG_FILE}"
    if [ "${videoDirectory}" != "" ]; then
        echo "$message" > "errorOccurred.txt"
    fi

    removeActiveFile
    exit 4
}

infoMessage()
{
    echo "INFO $(date) $1" | tee -a "${LOG_FILE}"
}

debugMessage()
{
    if [ $DEBUG -eq 1 ]; then
        echo "DEBUG $(date) $1" | tee -a "${LOG_FILE}"
    fi
}

changeToIncomingDirectory()
{
    mkdir -p "${INCOMING_DIRECTORY}"
    cd "${INCOMING_DIRECTORY}" || exit
    pwd
}

removeActiveFile()
{
    if [ -e "$ACTIVE_FILE" ]; then
        rm "$ACTIVE_FILE"
    fi
}

createFfmpegInputFile()
{
    debugMessage "Video format type: $1"
    touch ffmpeg.input

    for tsFile in "$(pwd)"/*$1
    do
        echo "file '${tsFile}'" >> ffmpeg.input
    done
}

###############################################################################
###############################################################################
## main
###############################################################################
###############################################################################

if [ $DEBUG -eq 1 ]; then
    set -x
fi

# remove wild card files from being shown
shopt -s nullglob

# check for single process running
if [ -e "$ACTIVE_FILE" ]; then
    errorMessage "Active file was found. If no files are being processed, then manually remove it."
    exit
fi

touch "$ACTIVE_FILE"

# create missing directories
mkdir -p "${PROCESSED_DIRECTORY}"
mkdir -p "${ARCHIVE_DIRECTORY}"
mkdir -p "${LOG_DIRECTORY}"

touch "${LOG_FILE}"

# clean up old log files
find "${LOG_DIRECTORY}" -mtime +30 -exec rm {} \;

changeToIncomingDirectory

# get first directory
videoDirectory=$(ls -trd1 */ --time=birth | grep -i -v errorOccurred |  cut -f1 -d'/' | head -1)
if [ "$videoDirectory" == "" ]; then
    infoMessage "No videos to process"
    removeActiveFile
    exit
fi

videoDirectory="${videoDirectory%/}"
infoMessage "Processing ${videoDirectory}"

# set the video type

videoType=$(echo "$videoDirectory" | awk -F'.' '{print $NF}')
debugMessage "Video type: ${videoType}"
ctaDuration=7

subscribeBoxColor="black"
subscribeBoxText=""

followBoxColor="black"
followPageText=""

bgBoxColor="black"

## set the text for the graphics

case $videoType in
    carriagehills)
        subscribeBoxColor="green"
        subscribeBoxText="JOIN OUR NEXT DOOR GROUP!"
        channelBrandText="CARRIAGE HILLS NEIGHBORHOOD ASSOCATION"
        ;;

    handyman | handymanvertical)
        subscribeBoxText="SUBSCRIBE AND FOLLOW FOR MORE HOME IMPROVEMENT IDEAS!"
        followPageText="FOLLOW US FOR MORE DIY HOME REPAIRS"

        if [ $dayOfWeek -lt 4 ]; then
            channelBrandText="RHTSERVICES.NET"
        else
            channelBrandText="@RHTSERVICESLLC"
        fi
        ;;

    techtalk | techtalkvertical)
        subscribeBoxText="SUBSCRIBE AND FOLLOW TO SEE MORE SOFTWARE AND TECH PROJECTS"
        followPageText="FOLLOW US FOR TECH CAREER ADVICE"

        if [ $dayOfWeek -lt 4 ]; then
            channelBrandText="@ALMOSTENGR"
        else
            channelBrandText="RHTSERVICES.NET"
        fi
        ;;

    lightshow)
        channelBrandText="$(date +%Y) CHRISTMAS LIGHT SHOW"
        bgBoxColor="maroon"
        ;;

    dashcam | fireworks | carrepair | dashcamvertical | dashcam2)
        ctaDuration=12
        subscribeBoxColor="green"
        subscribeBoxText="HELP THE CHANNEL GROW bY SUBSCRIBING NOW!"
        bgBoxColor="green"

        if [ $dayOfWeek -lt 4 ]; then
            channelBrandText="#KennyRamDashCam"
        else
            channelBrandText="Kenny Ram Dash Cam"
        fi
        ;;

    toastmasters)
        subscribeBoxColor="royalblue"
        followBoxColor="royalblue"
        bgBoxColor="royalblue"
        followPageText="FOLLOW US AT FACEBOOK.COM/TOWERTOASTMASTERS"
        subscribeBoxText="LEARN MORE ABOUT US AT TOWERTOASTMASTERS.COM"

        channelBrandText="TOWERTOASTMASTERS.ORG"
        ;;

    *)
        mv "$videoDirectory" "${videoDirectory}.errorOccurred"
        errorMessage "Invalid video type."
        ;;
esac

cd "${videoDirectory}" || exit

# stop processing if excluded files are present

fileCount=$(find . -type f \( -name "*.kdenlive" -o -name "details.txt" \) | wc -l)

if [ $fileCount -gt 0 ]; then
    mv "$1" "$1.errorOccurred"
    errorMessage "Invalid files present. Please remove the files from the directory"
fi

# remove previous render files

rm ffmpeg.input outputFinal.mp4 outputNoGraphics.mp4 $FINAL_OUTPUT_VERTICAL foreground.mp4 background.mp4 *ts *mp3

# lower case all file names

/usr/bin/rename 'y/A-Z/a-z/' *
result=$?

if [ "${result}" -gt 0 ]; then
    errorMessage "Rename binary not installed. Run sudo apt-get install rename"
    exit
fi

if [ "$(find . -maxdepth 1 -name '*.jpg' -print -quit)" ]; then
    for imageFile in *.jpg; do
        imageFileName="${imageFile%.*}"
        # ffmpeg -y -loop 1 -i "$imageFile" -c:v libx264 -t 5 -pix_fmt yuv420p "${imageFileName}.mp4"
        ffmpeg -y -framerate 1/3 -i "${imageFile}" -c:v libx264 -r 30 -pix_fmt yuv420p "${imageFileName}.mp4"
    done
fi


# create video segments
case $videoType in
    dashcam | fireworks)
        createFfmpegInputFile mov
        ;;

    dashcam2)
        createFfmpegInputFile mp4
        ;;

    *)
        for videoFile in "$(pwd)"/*.{mp4,mkv}
        do
            audioCount=$(/usr/bin/ffprobe -hide_banner "${videoFile}" 2>&1 | grep -i audio | wc -l)
            audioFile="${videoFile}.mp3"

            if [ $audioCount -eq 0 ]; then
                # videos that do not have audio track, have silent track added
                ffmpeg -y  -i "${videoFile}" -f lavfi -i anullsrc -vcodec copy -acodec aac -shortest temp.mp4

                mv temp.mp4 "${videoFile}"

                # convert video file to audio
                ffmpeg -y -hide_banner -i "${videoFile}" -vn "${audioFile}"
            else
                # convert video file to audio file
                ffmpeg -y -hide_banner -i "${videoFile}" -vn "${audioFile}"

                ## analyze audio volume
                maxVolume=$(/usr/bin/ffmpeg -y -hide_banner -i "${audioFile}" -af "volumedetect" -vn -sn -dn -f null /dev/null 2>&1 | grep max_volume | awk -F ' ' '{print $5}')

                if [ "${maxVolume}" != "0.0" ]; then
                    maxVolume=$(echo $maxVolume | tr -d '-')

                    tempAudioFile="temp.mp3"
                    /usr/bin/ffmpeg -y -hide_banner -i "${audioFile}" -af "volume=${maxVolume}" "$tempAudioFile"

                    /bin/mv "$tempFile" "$audioFile"
                fi
            fi

            ## Create TS formatted file

            videoClipFile=$videoFile
            audioClipFile="$videoClipFile.mp3"
            tsFile="${videoClipFile}.ts"

            /usr/bin/ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i "${videoClipFile}" -i "${audioClipFile}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "${tsFile}";

            conversionReturnCode=$?
            if [ $conversionReturnCode -gt 0 ]; then
                infoMessage "Using CPU conversion for ${tsFile}"
                /usr/bin/ffmpeg -y -hide_banner -i "${videoClipFile}" -i "${audioClipFile}" -shortest -map 0:v:0 -map 1:a:0 "${tsFile}";

                commandReturnCode=$?
                if [ $commandReturnCode -gt 0 ]; then
                    errorMessage "Unable to render with CPU"
                fi
            fi
        done

        createFfmpegInputFile ts
        ;;
esac


# render the video file without graphics included
case $videoType in
    handymanvertical | techtalkvertical | dashcamvertical)
        ffmpeg -y -hide_banner -f concat -safe 0 -i ffmpeg.input -vf "scale=1920:1080,boxblur=50" -an background.mp4

        ffmpeg -y -hide_banner -f concat -safe 0 -i ffmpeg.input -c:v copy -c:a copy foreground.mp4

        ffmpeg -y -hide_banner -i foreground.mp4 -i background.mp4 -filter_complex "[0:v]setpts=PTS-STARTPTS[fg];[1:v]setpts=PTS-STARTPTS[bg];[bg][fg]overlay=(W-w)/2:(H-h)/2" -c:a copy "outputNoGraphics.mp4"
        ;;

    dashcam | dashcam2)
        selectMixTrack

        ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i ffmpeg.input -i "${MIX_AUDIO_TRACK_FILE}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "outputNoGraphics.mp4"

        commandReturnCode=$?
        if [ $commandReturnCode -gt 0 ]; then
            infoMessage "Rendering with CPU"
            ffmpeg -y -hide_banner -f concat -safe 0 -i ffmpeg.input -i "${MIX_AUDIO_TRACK_FILE}" -shortest -map 0:v:0 -map 1:a:0 "outputNoGraphics.mp4"

            commandReturnCode=$?
            if [ $commandReturnCode -gt 0 ]; then
                errorMessage "Unable to render with CPU"
            fi
        fi
        ;;

    *)
        ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i ffmpeg.input -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi "outputNoGraphics.mp4";

        commandReturnCode=$?
        if [ $commandReturnCode -gt 0 ]; then
            infoMessage "Rendering with CPU"
            ffmpeg -y -hide_banner -f concat -safe 0 -i ffmpeg.input "outputNoGraphics.mp4";

            commandReturnCode=$?
            if [ $commandReturnCode -gt 0 ]; then
                errorMessage "Unable to render with CPU"
            fi
        fi

        # case $videoType in
        #     handyman | techtalk)

        #     ## Double check command for output
        #     # https://www.reddit.com/r/ffmpeg/comments/tgkd2o/making_portrait_video_from_a_landscape_video/
        #     ffmpeg -i outputNoGraphics.mp4 -vf "crop=w=607:h=1080:x=896:y=0" -c:v libx265 -crf 26 ${FINAL_OUTPUT_VERTICAL}
        #     ;;
        #     esac
        ;;
esac

# add graphics to video

brandDelaySeconds=297
fontSize="h/34"
videoGraphicsFilter="drawtext=textfile:'${channelBrandText}':fontcolor=white@0.6:fontsize=${fontSize}:${UPPERRIGHT}:box=1:boxcolor=${bgBoxColor}@0.4:boxborderw=10"

if [ "${subscribeBoxText}" != "" ]; then
    videoGraphicsFilter="${videoGraphicsFilter},drawtext=text='${subscribeBoxText}':fontcolor=white:box=1:boxcolor=${subscribeBoxColor}@1:boxborderw=20:fontsize=${fontSize}:${LOWERLEFT}:enable='if(lt(t,10),0,if(lt(mod(t-10,${brandDelaySeconds}),${ctaDuration}),1,0))'"
fi

if [ "${followPageText}" != "" ]; then
    videoGraphicsFilter="${videoGraphicsFilter},drawtext=text='${followPageText}':fontcolor=white:box=1:boxcolor=${followBoxColor}@1:boxborderw=20:fontsize=${fontSize}:${LOWER_LEFT1}:enable='if(lt(t,10),0,if(lt(mod(t-10,${brandDelaySeconds}),${ctaDuration}),1,0))'"
fi

debugMessage "Creating output without graphics file"

ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i outputNoGraphics.mp4 -filter_hw_device foo -vf "${videoGraphicsFilter}, format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -c:a copy outputFinal.mp4

commandReturnCode=$?
if [ $commandReturnCode -gt 0 ]; then
    infoMessage "Rendering with CPU"
    ffmpeg -y -hide_banner -i outputNoGraphics.mp4 -vf "${videoGraphicsFilter}" -shortest -c:a copy outputFinal.mp4;

    commandReturnCode=$?
    if [ $commandReturnCode -gt 0 ]; then
        errorMessage "Unable to render with CPU"
    fi
fi

# move output file

mv outputFinal.mp4 "${ARCHIVE_DIRECTORY}/${videoDirectory}.mp4"

# if vertical file was created, the move it to the archive directory

if [ -f "${FINAL_OUTPUT_VERTICAL}" ]; then
    mv ${FINAL_OUTPUT_VERTICAL} "${ARCHIVE_DIRECTORY}/${videoDirectory}.vertical.mp4"
fi

# archive the file video file and move it

tarballArchiveFile="${videoDirectory}.tar.xz"

infoMessage "Archiving video file ${tarballArchiveFile}"
tar -cJf "$tarballArchiveFile" outputNoGraphics.mp4

returnCode=$?
if [ ${returnCode} -gt 0 ]; then
    errorMessage "Unable to archive video file."
fi
mv "${tarballArchiveFile}" "${ARCHIVE_DIRECTORY}/${tarballArchiveFile}"

# move video directory to Processed directory

infoMessage "Moving video directory to Processed folder"
changeToIncomingDirectory
mv "${videoDirectory}" "${PROCESSED_DIRECTORY}"

removeActiveFile
