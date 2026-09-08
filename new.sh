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

AUDIO=""
CHANNEL_BRAND_TEXT=""
BRAND_TEXT_BG_COLOR="black"
IS_VERTICAL=0

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

loadChannelDashCam() { 
    AUDIO=$(selectMixTrack())
    CHANNEL_BRAND_TEXT="KENNY RAM DASH CAM"
    BRAND_TEXT_BG_COLOR="green"
    ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive_dashcam"
}

loadChannelDashCamCar() { 
    AUDIO=$(selectMixTrack())
    CHANNEL_BRAND_TEXT="KENNY RAM DASH CAM"
    BRAND_TEXT_BG_COLOR="green"
    ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive_dashcam"
}

loadChannelDashCamTruck() { 
    AUDIO=$(selectMixTrack())
    CHANNEL_BRAND_TEXT="KENNY RAM DASH CAM"
    BRAND_TEXT_BG_COLOR="green"
    ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive_dashcam"
}

loadChannelAlmostengr() { 
    CHANNEL_BRAND_TEXT="@ALMOSTENGR"
    ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive_personal"
}

loadChannelRhtServices() { 
    CHANNEL_BRAND_TEXT="RHTSERVICES.NET"
    BRAND_TEXT_BG_COLOR="Darkorange"
    ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive_handyman"
}

loadToastmasters() { 
    CHANNEL_BRAND_TEXT="TOWERTOASTMASTERS.ORG"
    BRAND_TEXT_BG_COLOR="royalblue"
    ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive_toastmasters"
}

loadChannelCarriageHills(){ 
    CHANNEL_BRAND_TEXT="CARRIAGE HILLS NEIGHBORHOOD ASSOCIATION"
    BRAND_TEXT_BG_COLOR="green"
    ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive_chna"
}

loadChannelLightShow() { 
    CHANNEL_BRAND_TEXT="$(date +%Y) CHRISTMAS LIGHT SHOW"
    BRAND_TEXT_BG_COLOR="maroon"
    ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive_personal"
}   


###############################################################################
###############################################################################
## main
###############################################################################
###############################################################################


# remove wild card files from being shown
shopt -s nullglob

# check for single process running
if [ -e "$ACTIVE_FILE" ]; then
    errorMessage "Active file was found. If no files are being processed, then manually remove it."
    exit 5
fi

touch "$ACTIVE_FILE"


# clean up old log files
find "${LOG_DIRECTORY}" -mtime +30 -exec rm {} \;

changeToIncomingDirectory

# get first directory
videoDirectory=$(ls -trd1 */ --time=birth | grep -i -v errorOccurred |  cut -f1 -d'/' | head -1)
if [ "$videoDirectory" == "" ]; then
    infoMessage "No videos to process"
    removeActiveFile
    exit 6
fi


videoDirectory="${videoDirectory%/}"
infoMessage "Processing ${videoDirectory}"

cd "${videoDirectory}" || exit

if [ ! -f config.sh ]; then 
    exit 3
fi

source config.sh

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
    exit 7
fi

if [ "$(find . -maxdepth 1 -name '*.jpg' -print -quit)" ]; then
    for imageFile in *.jpg; do
        imageFileName="${imageFile%.*}"
        # ffmpeg -y -loop 1 -i "$imageFile" -c:v libx264 -t 5 -pix_fmt yuv420p "${imageFileName}.mp4"
        ffmpeg -y -framerate 1/3 -i "${imageFile}" -c:v libx264 -r 30 -pix_fmt yuv420p "${imageFileName}.mp4"
    done
fi

###################################################
#############33 OTHER PROCESSING GOES HERE
#####################################################




# add graphics to video

brandDelaySeconds=297
fontSize="h/34"
videoGraphicsFilter="drawtext=textfile:'${CHANNEL_BRAND_TEXT}':fontcolor=white@0.6:fontsize=${fontSize}:${UPPERRIGHT}:box=1:boxcolor=${BRAND_TEXT_BG_COLOR}@0.4:boxborderw=10"

# if [ "${subscribeBoxText}" != "" ]; then
#     videoGraphicsFilter="${videoGraphicsFilter},drawtext=text='${subscribeBoxText}':fontcolor=white:box=1:boxcolor=${subscribeBoxColor}@1:boxborderw=20:fontsize=${fontSize}:${LOWERLEFT}:enable='if(lt(t,10),0,if(lt(mod(t-10,${brandDelaySeconds}),${ctaDuration}),1,0))'"
# fi

# if [ "${followPageText}" != "" ]; then
#     videoGraphicsFilter="${videoGraphicsFilter},drawtext=text='${followPageText}':fontcolor=white:box=1:boxcolor=${followBoxColor}@1:boxborderw=20:fontsize=${fontSize}:${LOWER_LEFT1}:enable='if(lt(t,10),0,if(lt(mod(t-10,${brandDelaySeconds}),${ctaDuration}),1,0))'"
# fi

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
