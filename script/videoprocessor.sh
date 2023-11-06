#!/bin/bash

PATH=/usr/bin/:/bin:/usr/sbin:/sbin

BASE_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/videos"
DEBUG=1

INCOMING_DIRECTORY="${BASE_DIRECTORY}/incoming"
PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processed"
ARCHIVE_DIRECTORY=""
UPLOAD_DIRECTORY=""
ACTIVE_FILE="${BASE_DIRECTORY}/.active.txt"
MIX_AUDIO_TRACK_FILE="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ytvideostructure/07music/mix05.mp3"

PADDING=70
UPPERLEFT="x=${PADDING}:y=${PADDING}"
UPPERCENTER="x=(w-tw)/2:y=${PADDING}"
UPPERRIGHT="x=w-tw-${PADDING}:y=${PADDING}"
CENTERED="x=(w-tw)/2:y=(h-th)/2"
LOWERLEFT="x=${PADDING}:y=h-th-${PADDING}"
LOWERCENTER="x=(w-tw)/2:y=h-th-${PADDING}"
LOWERRIGHT="x=w-tw-${PADDING}:y=h-th-${PADDING}"

CHANNEL_BRAND="x=w-tw-${PADDING}:y=${PADDING}"

errorMessage()
{
    echo "ERROR $(date) $1"
}

infoMessage()
{
    echo "INFO $(date) $1"
}

debugMessage()
{
    if [ $DEBUG -eq 1 ]; then
        echo "DEBUG $(date) $1"
    fi
}

changeToIncomingDirectory() 
{
    mkdir -p "${INCOMING_DIRECTORY}"
    cd "${INCOMING_DIRECTORY}"
    pwd
}

createMissingDirectories()
{
    mkdir -p "${PROCESSED_DIRECTORY}"
    mkdir -p "${ARCHIVE_DIRECTORY}"
    mkdir -p "${UPLOAD_DIRECTORY}"
}

checkForSingleProcess()
{
    if [ -e "$ACTIVE_FILE" ]; then
        errorMessage "Active file was found. If no files are being processed, then manually remove it."
        exit
    fi
}

removeActiveFile()
{
    rm "$ACTIVE_FILE"
}

getFirstDirectory()
{
    videoDirectory=$(/bin/ls -1td */ | grep -i -v errorOccurred | /usr/bin/head -1)
    videoDirectory=${videoDirectory%/}
    if [ "$videoDirectory" == "" ]; then 
        infoMessage "No videos to process"
        sleep 3600
    else 
        infoMessage "Processing ${videoDirectory}"
    fi
}

stopProcessingIfExcludedFilesExist()
{
    fileCount=$(find . -type f \( -name "*.kdenlive" -o -name "*ffmpeg*" -o -name "details.txt" \) | wc -l)

    if [ $fileCount -gt 0 ]; then
        errorMessage "Invalid files present. Please remove the files from the directory"
        mv $1 "$1.errorOccurred"
        exit
    fi
}

lowercaseAllFileNames()
{
    /usr/bin/rename 'y/A-Z/a-z/' *
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
    return maxVolume
}

adjustAudioVolume()
{
    tempFile="temp.mp3"
    volumeLevel=$(echo $2 | tr -d ' ')
    /usr/bin/ffmpeg -hide_banner -i $1 -af "volume=${volumeLevel}" $tempFile
    /bin/mv $tempFile $1
}

normalizeAudio()
{
    debugMessage "Normalizing audio for $1"

    videoFile=$1
    audioCount=$(/usr/bin/ffprobe -hide_banner ${videoFile} 2>&1 | grep -i audio | wc -l)
    audioFile="${videoFile}.mp3"

    if [ $audioCount -eq 0 ]; then
        cp $MIX_AUDIO_TRACK_FILE $audioFile
    else 
        convertVideoFileToMp3Audio $videoFile $audioFile

        volumeGain=$(analyzeAudioVolume)

        adjustAudioVolume $audioFile $volumeGain
    fi
}

createTsFile()
{
    videoClipFile=$1
    audioClipFile="$videoClipFile.mp3"
    tsFile="${videoClipFile}.ts"

    /usr/bin/ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i "${videoClipFile}" -i "${audioClipFile}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "${tsFile}";
}

compressOutputVideoFile()
{
    videoTitle=$(basename $(pwd))

    archiveDirectory="${BASE_DIRECTORY}/archive"
    tarFileName="${videoTitle}.tar.xz"

    tar -cJf ${tarFileName} ${OUTPUT_FILENAME}

    mv "${tarFileName}" "${archiveDirectory}"
}

deleteProcessedRawVideoFiles()
{
    find "${PROCESSED_DIRECTORY}" -type d -mtime +14 -exec echo {} \; -exec rm -r {} \;
}

moveVideoAsProcessed()
{
    cd ${INCOMING_DIRECTORY}
    touch $1
    mv $1 ${PROCESSED_DIRECTORY}
}

createFfmpegInputFile()
{
    debugMessage "Video format type: $1"
    touch ffmpeg.input

    for tsFile in *$1
    do
        echo "file ${tsFile}" >> ffmpeg.input
    done
}

addChannelBrandToVideo() 
{
    videoGraphicsFilter="drawtext=textfile:'${channelBrandText}':fontcolor=white@0.5:fontsize=h/28:${UPPERRIGHT}"
    ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i outputNoGraphics.mp4 -filter_hw_device foo -vf "${videoGraphicsFilter}, format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -c:a copy outputFinal.mp4
}

renderVideoWithoutGraphics()
{
    ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i ffmpeg.input -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi "outputNoGraphics.mp4";
}

renderVideoWithAudio()
{
    ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i ffmpeg.input -i "${audioFilePath}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "outputNoGraphics.mp4";
}

setVideoType()
{
    videoType=$(echo "$videoDirectory" | awk -F'.' '{print $NF}')

    debugMessage "Video type: ${videoType}"
    dayOfWeek=$(date +%u)

    case $videoType in
        handyman)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivehandyman"
            UPLOAD_DIRECTORY="${BASE_DIRECTORY}/uploadhandyman"

            if [ dayOfWeek -lt 4 ]; then
                channelBrandText="Robinson Handy and Technology Services"
            else 
                channelBrandText="rhtservices.net"
            fi
            ;;
        techtalk)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivetechnology"
            UPLOAD_DIRECTORY="${BASE_DIRECTORY}/uploadtechnology"

            if [ dayOfWeek -lt 4 ]; then
                channelBrandText="Tech Talk with RHT Services"
            else 
                channelBrandText="rhtservices.net"
            fi
            ;;
        lightshow)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivetechnology"
            UPLOAD_DIRECTORY="${BASE_DIRECTORY}/uploadtechnology"
            channelBrandText="2023 Christmas Light Show"
            ;;
        dashcam | fireworks | roads)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivedashcam"
            UPLOAD_DIRECTORY="${BASE_DIRECTORY}/uploaddashcam"
            channelBrandText="Kenny Ram Dash Cam"
            ;;
        toastmasters)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivetoastmasters"
            UPLOAD_DIRECTORY="${BASE_DIRECTORY}/uploadtoastmasters"
            channelBrandText="towertoastmasters.org"
            ;;
        *)
            errorMessage "Invalid video type."
            exit
            ;;
    esac
}

renderVideoSegments()
{
    case $videoType in
        dashcam | fireworks)
            errorMessage "Dash cam and fireworks videos not implemented"
            exit
            ;;
            
        *)
            for videoFile in *mp4 *mkv
            do
                normalizeAudio "${videoFile}"

                createTsFile "${videoFile}"
            done
            ;;
    esac
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

while true
do
    changeToIncomingDirectory

    videoDirectory=$(getFirstDirectory)

    getFirstDirectory

    setVideoType

    createMissingDirectories

    cd ${videoDirectory}

    stopProcessingIfExcludedFilesExist ${videoDirectory}

    lowercaseAllFileNames
        
    renderVideoSegments

    createFfmpegInputFile ts

    renderVideoWithoutGraphics

    addChannelBrandToVideo

    mv outputFinal.mp4 "${UPLOAD_DIRECTORY}/${videoDirectory}.mp4"

    tarballArchiveFile="${videoDirectory}.tar.xz"

    tar -cJf "$tarballArchiveFile" outputNoGraphics.mp4

    mv "${tarballArchiveFile}" "${ARCHIVE_DIRECTORY}/${tarballArchiveFile}"

    changeToIncomingDirectory

    mv "${videoDirectory}" "${PROCESSED_DIRECTORY}"
done
