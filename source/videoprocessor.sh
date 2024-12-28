#!/bin/bash

PATH=/usr/bin/:/bin:/usr/sbin:/sbin

BASE_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/videos"
DEBUG=1

INCOMING_DIRECTORY="${BASE_DIRECTORY}/incoming"
PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processed"
ARCHIVE_DIRECTORY=""
ACTIVE_FILE="${BASE_DIRECTORY}/.active.txt"
MIX_AUDIO_TRACK_FILE="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ytvideostructure/07music/mix02.mp3"

PADDING=70 # Padding for the graphics from the end of the screen
UPPERLEFT="x=${PADDING}:y=${PADDING}"
UPPERCENTER="x=(w-tw)/2:y=${PADDING}"
UPPERRIGHT="x=w-tw-${PADDING}:y=${PADDING}"
CENTERED="x=(w-tw)/2:y=(h-th)/2"
LOWERLEFT="x=${PADDING}:y=h-th-${PADDING}"
LOWERCENTER="x=(w-tw)/2:y=h-th-${PADDING}"
LOWERRIGHT="x=w-tw-${PADDING}:y=h-th-${PADDING}"

videoDirectory=""
TIMESTAMP=$(date +'%Y%m%d.%H%M%S')
LOG_FILE="/home/almostengr/Documents/videoprocessor.${TIMESTAMP}.log"
dayOfWeek=$(date +%u)
archiveFileExist=false
manualFileExist=false


selectMixTrack()
{
    MUSIC_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ytvideostructure/07music/"

    hourOfDay=$(date +%H)

    # case $dayOfWeek in
    case $hourOfDay in
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
        echo "$message" > "${videoDirectory}/errorOccurred.txt"
    fi
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

createMissingDirectories()
{
    mkdir -p "${PROCESSED_DIRECTORY}"
    mkdir -p "${ARCHIVE_DIRECTORY}"
}

checkForSingleProcess()
{
    if [ -e "$ACTIVE_FILE" ]; then
        errorMessage "Active file was found. If no files are being processed, then manually remove it."
        exit
    fi

    touch "$ACTIVE_FILE"
}

removeActiveFile()
{
    if [ -e "$ACTIVE_FILE" ]; then
        rm "$ACTIVE_FILE"
    fi
}

getFirstDirectory()
{
    # videoDirectory=$(find * -type d | grep -i -v errorOccurred | head -1)
    videoDirectory=$(ls -trd1 */ | grep -i -v errorOccurred |  cut -f1 -d'/' | head -1)
    if [ "$videoDirectory" == "" ]; then
        infoMessage "No videos to process"
        removeActiveFile
        exit
    else
        videoDirectory="${videoDirectory%/}"
        infoMessage "Processing ${videoDirectory}"
    fi
}

stopProcessingIfExcludedFilesExist()
{
    fileCount=$(find . -type f \( -name "*.kdenlive" -o -name "details.txt" \) | wc -l)

    if [ $fileCount -gt 0 ]; then
        errorMessage "Invalid files present. Please remove the files from the directory"
        mv "$1" "$1.errorOccurred"
        removeActiveFile
        exit
    fi
}

lowercaseAllFileNames()
{
    /usr/bin/rename 'y/A-Z/a-z/' *
    result=$?

    if [ "${result}" -gt 0 ]; then
        errorMessage "Rename binary not installed. Run sudo apt-get install rename"
        exit
    fi
}

convertVideoFileToMp3Audio()
{
    ffmpeg -y -hide_banner -i "$videoFile" -vn "$audioFile"
}

analyzeAudioVolume()
{
    analysisResult=$(/usr/bin/ffmpeg -y -hide_banner -i $1 -af "volumedetect" -vn -sn -dn -f null /dev/null 2>&1 | grep max_volume)
    read -ra newArray <<< "${analysisResult}"
    maxVolume=${newArray[1]}

    if [ "${maxVolume}" == "" ]; then
        maxVolume="0"
    fi

    return $maxVolume
}

adjustAudioVolume()
{
    tempFile="temp.mp3"
    volumeLevel=$(echo "$2" | tr -d ' ')

    # dont render the audio file if the volume level has not changed
    if [ "${volumeLevel}" == "0" ]; then
        return
    fi

    /usr/bin/ffmpeg -y -hide_banner -i "$1" -af "volume=${volumeLevel}" "$tempFile"
    /bin/mv "$tempFile" "$1"
}

normalizeAudio()
{
    debugMessage "Normalizing audio for $1"

    videoFile=$1
    audioCount=$(/usr/bin/ffprobe -hide_banner "${videoFile}" 2>&1 | grep -i audio | wc -l)
    audioFile="${videoFile}.mp3"

    if [ $audioCount -eq 0 ]; then
        # videos that do not have audio track, have silent track added
        ffmpeg -y  -i "${videoFile}" -f lavfi -i anullsrc -vcodec copy -acodec aac -shortest temp.mp4

        mv temp.mp4 "${videoFile}"

        convertVideoFileToMp3Audio "${videoFile}" "${audioFile}"
    else
        convertVideoFileToMp3Audio "${videoFile}" "${audioFile}"

        volumeGain=$(analyzeAudioVolume)

        adjustAudioVolume "$audioFile" "$volumeGain"
    fi
}

createTsFile()
{
    videoClipFile=$1
    audioClipFile="$videoClipFile.mp3"
    tsFile="${videoClipFile}.ts"

    /usr/bin/ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i "${videoClipFile}" -i "${audioClipFile}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "${tsFile}";

    conversionReturnCode=$?

    if [ $conversionReturnCode -gt 0 ]; then
        errorMessage "Using CPU conversion for ${tsFile}"
        /usr/bin/ffmpeg -y -hide_banner -i "${videoClipFile}" -i "${audioClipFile}" -shortest -map 0:v:0 -map 1:a:0 "${tsFile}";
    fi
}

compressOutputVideoFile()
{
    videoTitle=$(basename "$(pwd)")
    archiveDirectory="${BASE_DIRECTORY}/archive"
    tarFileName="${videoTitle}.tar.xz"

    tar -cJf "${tarFileName}" "${OUTPUT_FILENAME}"

    mv "${tarFileName}" "${archiveDirectory}"
}

deleteProcessedRawVideoFiles()
{
    find "${PROCESSED_DIRECTORY}" -type d -mtime +14 -exec echo {} \; -exec rm -r {} \;
}

moveVideoAsProcessed()
{
    cd "${INCOMING_DIRECTORY}"
    mv "${videoDirectory}" "${PROCESSED_DIRECTORY}"
}

moveVideoFolderToTrash()
{
    infoMessage "Moving video directory ${videoDirectory} to trash"
    /usr/bin/gio trash "${videoDirectory}"
}

removeVideoDirectory()
{    
    infoMessage "Removing video directory ${videoDirectory}"
    cd "${INCOMING_DIRECTORY}"
    rm -r "${videoDirectory}"
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

addChannelBrandToVideo()
{
    brandDelaySeconds=297
    fontSize="h/34"
    videoGraphicsFilter="drawtext=textfile:'${channelBrandText}':fontcolor=white@0.6:fontsize=${fontSize}:${UPPERRIGHT}:box=1:boxcolor=${bgBoxColor}@0.4:boxborderw=10"

    if [ "${subscribeBoxText}" != "" ]; then
        videoGraphicsFilter="${videoGraphicsFilter},drawtext=text='${subscribeBoxText}':fontcolor=white:box=1:boxcolor=${subscribeBoxColor}@1:boxborderw=20:fontsize=${fontSize}:${LOWERLEFT}:enable='if(lt(t,10),0,if(lt(mod(t-10,${brandDelaySeconds}),${ctaDuration}),1,0))'"
    fi

    # if [ "${followPageText}" != "" ]; then
        # videoGraphicsFilter="${videoGraphicsFilter},drawtext=text='${followPageText}':fontcolor=white:box=1:boxcolor=${followBoxColor}@1:boxborderw=20:fontsize=${fontSize}:${LOWERLEFT}:enable='if(lt(t,10),0,if(lt(mod(t-10,${brandDelaySeconds}+${ctaDuration}),${ctaDuration}),1,0))'"
    # fi

    ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i outputNoGraphics.mp4 -filter_hw_device foo -vf "${videoGraphicsFilter}, format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -c:a copy outputFinal.mp4

    commandReturnCode=$?
    if [ $commandReturnCode -gt 0 ]; then
        errorMessage "Rendering with CPU"
        ffmpeg -y -hide_banner -i outputNoGraphics.mp4 -vf "${videoGraphicsFilter}" -shortest -c:a copy outputFinal.mp4;
    fi
}

renderVideoWithoutGraphics()
{
    if [ "${archiveFileExist}" == true ]; then
        return;
    fi

    case $videoType in
        handymanvertical | techtalkvertical | dashcamvertical)
            ffmpeg -y -hide_banner -f concat -safe 0 -i ffmpeg.input -vf "scale=1920:1080,boxblur=50" -an background.mp4

            ffmpeg -y -hide_banner -f concat -safe 0 -i ffmpeg.input -c:v copy -c:a copy foreground.mp4

            ffmpeg -y -hide_banner -i foreground.mp4 -i background.mp4 -filter_complex "[0:v]setpts=PTS-STARTPTS[fg];[1:v]setpts=PTS-STARTPTS[bg];[bg][fg]overlay=(W-w)/2:(H-h)/2" -c:a copy "outputNoGraphics.mp4"
            ;;

        dashcam)
            ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i ffmpeg.input -i "${MIX_AUDIO_TRACK_FILE}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "outputNoGraphics.mp4"

            commandReturnCode=$?
            if [ $commandReturnCode -gt 0 ]; then
                errorMessage "Rendering with CPU"
                ffmpeg -y -hide_banner -f concat -safe 0 -i ffmpeg.input -i "${MIX_AUDIO_TRACK_FILE}" -shortest -map 0:v:0 -map 1:a:0 "outputNoGraphics.mp4"
            fi
            ;;

        *)
            ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i ffmpeg.input -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi "outputNoGraphics.mp4";

            commandReturnCode=$?
            if [ $commandReturnCode -gt 0 ]; then
                errorMessage "Rendering with CPU"
                ffmpeg -y -hide_banner -f concat -safe 0 -i ffmpeg.input "outputNoGraphics.mp4";
            fi
            ;;
    esac
}

setVideoType()
{
    videoType=$(echo "$videoDirectory" | awk -F'.' '{print $NF}')
    debugMessage "Video type: ${videoType}"
    ctaDuration=5

    subscribeBoxColor="red"
    subscribeBoxText=""

    followBoxColor="blue"
    followPageText=""

    bgBoxColor="black"

    case $videoType in
        carriagehills)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivecarriagehills"
            
            subscribeBoxColor="green"
            subscribeBoxText="JOIN OUR NEXT DOOR GROUP!"
            channelBrandText="CARRIAGE HILLS NEIGHBORHOOD ASSOCATION"
            ;;

        handyman | handymanvertical)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivehandyman"
            
            subscribeBoxText="SUBSCRIBE AND FOLLOW FOR MORE HOME IMPROVEMENT IDEAS!"
            followPageText="FOLLOW US FOR MORE DIY HOME REPAIRS"

            if [ $dayOfWeek -lt 4 ]; then
                channelBrandText="RHTSERVICES.NET"
            else
                channelBrandText="@RHTSERVICESLLC"
            fi
            ;;

        techtalk | lightshow | techtalkvertical)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivetechnology"

            subscribeBoxText="SUBSCRIBE AND FOLLOW TO SEE MORE SOFTWARE AND TECH PROJECTS"
            followPageText="FOLLOW US FOR TECH CAREER ADVICE"

            if [ "${videoType}" == "lightshow" ]; then
                channelBrandText="$(date +%Y) CHRISTMAS LIGHT SHOW"
                bgBoxColor="maroon"
            elif [ $dayOfWeek -lt 4 ]; then
                channelBrandText="@RHTSERVICESTECH"
            else
                channelBrandText="RHTSERVICES.NET"
            fi
            ;;

        dashcam | fireworks | carrepair | dashcamvertical)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivedashcam"
            
            ctaDuration=10
            subscribeBoxColor="green"
            subscribeBoxText="SUBSCRIBE TO SEE WEEKLY DASH CAM VIDEOS!"
            bgBoxColor="green"

            if [ $dayOfWeek -lt 4 ]; then
                channelBrandText="#KennyRamDashCam"
            else
                channelBrandText="KENNY RAM DASH CAM"
            fi
            ;;

        toastmasters)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivetoastmasters"
            subscribeBoxColor="royalblue"
            subscribeBoxText="FOLLOW US AT FACEBOOK.COM/TOWERTOASTMASTERS"
            bgBoxColor="royalblue"

            channelBrandText="TOWERTOASTMASTERS.ORG"
            ;;

        *)
            errorMessage "Invalid video type."
            mv "$videoDirectory" "${videoDirectory}.errorOccurred"
            removeActiveFile
            exit
            ;;
    esac
}

renderVideoSegments()
{
    if [ "${archiveFileExist}" == true ]; then
        return;
    fi

    case $videoType in
        dashcam | fireworks)
            createFfmpegInputFile mov
            ;;

        handymanvertical | techtalkvertical | dashcamvertical)
            for videoFile in "$(pwd)"/*.{mp4}
            do
                normalizeAudio "${videoFile}"
            done

            createFfmpegInputFile mp4
            ;;

        *)
            for videoFile in "$(pwd)"/*.{mp4,mkv}
            do
                normalizeAudio "${videoFile}"
                createTsFile "${videoFile}"
            done

            createFfmpegInputFile ts
            ;;
    esac
}

renderVideoFromImages()
{
    if [ "${archiveFileExist}" == true || "${manualFileExist}" == true ]; then
        return;
    fi

    if [ "$(find . -maxdepth 1 -name '*.jpg' -print -quit)" ]; then
        for imageFile in *.jpg; do
            imageFileName="${imageFile%.*}"
            # ffmpeg -y -loop 1 -i "$imageFile" -c:v libx264 -t 5 -pix_fmt yuv420p "${imageFileName}.mp4"
            ffmpeg -y -framerate 1/3 -i "${imageFile}" -c:v libx264 -r 30 -pix_fmt yuv420p "${imageFileName}.mp4"
        done
    fi
}

removePreviousRenderFiles()
{
    rm ffmpeg.input outputFinal.mp4 outputNoGraphics.mp4 foreground.mp4 background.mp4 *ts *mp3
}

archiveVideoFile()
{
    tarballArchiveFile="${videoDirectory}.tar.xz"

    infoMessage "Archiving video file ${tarballArchiveFile}"
    tar -cJf "$tarballArchiveFile" outputNoGraphics.mp4

    returnCode=$?
    if [ ${returnCode} -gt 0 ]; then
        errorMessage "Unable to archive video file."
        exit
    fi
    mv "${tarballArchiveFile}" "${ARCHIVE_DIRECTORY}/${tarballArchiveFile}"
}

doesArchiveFileExist()
{
    if [ -e "${videoDirectory}/archive.option" ]; then
        archiveFileExist=true
    fi
}

doesManualEditFileExist()
{
    if [ -e "${videoDirectory}/manual.option" ]; then
        manualFileExist=true
    fi
}

###############################################################################
###############################################################################
## main
###############################################################################
###############################################################################

if [ $DEBUG -eq 1 ]; then
    set -x
fi

checkForSingleProcess

touch "${LOG_FILE}"

changeToIncomingDirectory

getFirstDirectory

setVideoType

createMissingDirectories

cd "${videoDirectory}" || exit

stopProcessingIfExcludedFilesExist "${videoDirectory}"

removePreviousRenderFiles

lowercaseAllFileNames

doesArchiveFileExist

doesManualEditFileExist

renderVideoFromImages

renderVideoSegments

renderVideoWithoutGraphics

addChannelBrandToVideo

mv outputFinal.mp4 "${ARCHIVE_DIRECTORY}/${videoDirectory}.mp4"

archiveVideoFile

changeToIncomingDirectory

# moveVideoAsProcessed

# removeVideoDirectory  # todo implement after being tested

moveVideoFolderToTrash

removeActiveFile
