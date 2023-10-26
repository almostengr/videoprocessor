#!/bin/bash
PATH=/usr/bin/:/bin:/usr/sbin:/sbin

BASE_DIRECTORY="/home/almostengr"
INCOMING_DIRECTORY="${BASE_DIRECTORY}/incoming"
PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processed"
OUTPUT_FILENAME="output.mp4"

PADDING=70
UPPERLEFT="x=${PADDING}:y=${PADDING}"
UPPERCENTER="x=(w-tw)/2:y=${PADDING}"
UPPERRIGHT="x=w-tw-${PADDING}:y=${PADDING}"
CENTERED="x=(w-tw)/2:y=(h-th)/2"
LOWERLEFT="x=${PADDING}:y=h-th-${PADDING}"
LOWERCENTER="x=(w-tw)/2:y=h-th-${PADDING}"
LOWERRIGHT="x=w-tw-${PADDING}:y=h-th-${PADDING}"

CHANNEL_BRAND="x=w-tw-${PADDING}:y=${PADDING}"

checkForSingleProcess()
{
    processCount=$(/usr/bin/ps -ef | grep videoprocessor | /usr/bin/wc -l)

    if [ $processCount -gt 1 ] then;
        echo "Found instance already running."
        exit 1;
    fi
}

getFirstDirectory()
{
    directory=$(/bin/ls -1td */ | grep -i -v errorOccurred | /usr/bin/head -1)
    if [ $directory == "" ]; then 
        sleep 3600
    else 
        echo "Processing ${directory}"
    fi

    return directory
}

stopProcessingIfExcludedFilesExist()
{
    fileCount=$(ls -1 *kdenlive details.txt *ffmpeg* | wc -l)
    if [ $fileCount -gt 0 ]; then
        echo "Multiple invalid files exist. Please remove the files from the directory"
        mv $1 "$1.errorOccurred"
        exit 2
    fi
}

lowercaseAllFileNames()
{
    /usr/bin/rename 'y/A-Z/a-z/' *
}

convertVideoFileToMp3Audio()
{
    /usr/bin/ffmpeg -i $1 -vn $2
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
    /bin/mv $1 $tempFile
}

normalizeAudio()
{
    videoFile=$1
    audioFile="${videoFile}.mp3"
    audioCount=$(/usr/bin/ffprobe -hide_banner ${videoFile} | grep -i audio | wc -l)

    if [ audioCount -eq 0 ]; then

        # todo get mix track
    else 
        convertVideoFileToMp3Audio $videoFile $audioFile

        volumeGain=$(analyzeAudioVolume)

        adjustAudioVolume $audioFile $volumeGain
    fi

    tsFile="${videoFile}.ts"
    /usr/bin/ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i \"${videoFile}\" -i \"${audioFile}\" -filter_hw_device foo -vf \"format=vaapi|nv12,hwupload\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"${tsFile}\";
}

archiveFinalVideo()
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

moveToProcessed()
{
    cd ${INCOMING_DIRECTORY}
    touch $1
    mv $1 ${PROCESSED_DIRECTORY}
}

###############################################################################
###############################################################################
## main
###############################################################################
###############################################################################

checkForSingleProcess()

while (true)
{
    deleteProcessedRawVideoFiles()

    cd ${INCOMING_DIRECTORY}

    videoDirectory=$(getFirstDirectory)

    if [ ${videoDirectory} == "" ]; then
        continue
    fi

    cd ${videoDirectory}

    stopProcessingIfExcludedFilesExist ${videoDirectory}

    lowercaseAllFileNames

    for videoFile in *mp4 *mov *mkv
    do
        normalizeAudio "${videoFile}"
    done

    moveToProcessed ${videoDirectory}
}
