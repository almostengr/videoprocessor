#!/bin/bash

PATH="/usr/bin/:/bin:/usr/sbin:/sbin:${PATH}"

BASE_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/videos"
DEBUG=0

INCOMING_DIRECTORY="${BASE_DIRECTORY}/incoming"
PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processed"
ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive"
ERROR_DIRECTORY="${BASE_DIRECTORY}/error"
ACTIVE_FILE="${BASE_DIRECTORY}/.active.txt"

MUSIC_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ytvideostructure/07music/"
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

ctaDuration=7
subscribeBoxColor="black"
# subscribeBoxText=""

followBoxColor="black"
# followPageText=""

bgBoxColor="black"
brandDelaySeconds=297
fontSize="h/34"

getFirstVideoDirectory() {
    videoDirectory=$(ls -trd1 */ --time=birth | grep -i -v errorOccurred |  cut -f1 -d'/' | head -1)
    if [ "$videoDirectory" == "" ]; then
        infoMessage "No videos to process"
        removeActiveFile
        exit 6
    fi

    videoDirectory="${videoDirectory%/}"
    infoMessage "Processing ${videoDirectory}"

    return ${videoDirectory}
}

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
        debugMessage "Removing active file"
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

# check for single process running
exitWhenActiveFilePresent() {
    if [ -e "$ACTIVE_FILE" ]; then
        errorMessage "Active file was found. If no files are being processed, then manually remove it."
        exit 5
    fi

    touch "$ACTIVE_FILE"
}

createMissingDirectories() {
    mkdir -p "${PROCESSED_DIRECTORY}"
    mkdir -p "${ARCHIVE_DIRECTORY}"
    mkdir -p "${LOG_DIRECTORY}"
}

lowercaseAllFileNames() {
    /usr/bin/rename 'y/A-Z/a-z/' *
    result=$?

    if [ "${result}" -gt 0 ]; then
        errorMessage "Rename binary not installed. Run sudo apt-get install rename"
        exit 7
    fi
}

exitWhenExcludedFilesPresent() {
    # stop processing if excluded files are present

    fileCount=$(find . -type f \( -name "*.kdenlive" -o -name "details.txt" \) | wc -l)

    if [ $fileCount -gt 0 ]; then
        mv "$1" "$1.errorOccurred"
        errorMessage "Invalid files present. Please remove the files from the directory"
    fi
}

removePreviousRenderFiles() {
    rm ffmpeg.input outputFinal.mp4 outputNoGraphics.mp4 $FINAL_OUTPUT_VERTICAL foreground.mp4 background.mp4 *ts *mp3
}

if [ $DEBUG -eq 1 ]; then
    set -x
fi


touch "${LOG_FILE}"

# remove wild card files from being shown
shopt -s nullglob

# clean up old log files
find "${LOG_DIRECTORY}" -mtime +30 -exec rm {} \;