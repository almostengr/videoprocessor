#!/bin/bash
PATH=/usr/bin/:/bin:/usr/sbin:/sbin

BASE_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/videos"
DEBUG=1

INCOMING_DIRECTORY="${BASE_DIRECTORY}/incoming"
PROCESSED_DIRECTORY=""
ARCHIVE_DIRECTORY=""
UPLOAD_DIRECTORY=""
OUTPUT_FILENAME="output.mp4"
ACTIVE_FILE="${BASE_DIRECTORY}/.active.txt"

PADDING=70
UPPERLEFT="x=${PADDING}:y=${PADDING}"
UPPERCENTER="x=(w-tw)/2:y=${PADDING}"
UPPERRIGHT="x=w-tw-${PADDING}:y=${PADDING}"
CENTERED="x=(w-tw)/2:y=(h-th)/2"
LOWERLEFT="x=${PADDING}:y=h-th-${PADDING}"
LOWERCENTER="x=(w-tw)/2:y=h-th-${PADDING}"
LOWERRIGHT="x=w-tw-${PADDING}:y=h-th-${PADDING}"

CHANNEL_BRAND="x=w-tw-${PADDING}:y=${PADDING}"

logMessage()
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
        echo "Active file was found. If no files are being processed, then manually remove it."
        exit
    fi

    # touch "$ACTIVE_FILE"
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
        echo "No videos to process"
        sleep 3600
    else 
        echo "Processing ${videoDirectory}"
    fi

    # return $directory
}

stopProcessingIfExcludedFilesExist()
{
    # fileCount=$(ls -1 *kdenlive details.txt *ffmpeg* | wc -l)
    # =$(find . -type f \( -name "*.kdenlive" -o -name "*ffmpeg*" -o -name "details.txt" \) | grep -E -c '\.(txt|csv)$')
    fileCount=$(find . -type f \( -name "*.kdenlive" -o -name "*ffmpeg*" -o -name "details.txt" \) | wc -l)

    if [ $fileCount -gt 0 ]; then
        echo "Invalid files present. Please remove the files from the directory"
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
    videoFile=$1
    audioFile="${videoFile}.mp3"
    audioCount=$(/usr/bin/ffprobe -hide_banner ${videoFile} | grep -i audio | wc -l)

    if [ $audioCount -eq 0 ]; then

        # todo get mix track
        date
    else 
        convertVideoFileToMp3Audio $videoFile $audioFile

        volumeGain=$(analyzeAudioVolume)

        adjustAudioVolume $audioFile $volumeGain
    fi

    tsFile="${videoFile}.ts"
    # /usr/bin/ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i "${videoFile}" -i "${audioFile}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "${tsFile}";
    # ffmpeg -y -hide_banner -i "$videoFile" -vn "$audioFile"
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
    echo $1
    ls -1 *$1 > ffmpeg.input
}

renderVideo()
{
    ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i ffmpeg.input -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi "output.mp4";
}

renderVideoWithAudio()
{
    ffmpeg -i -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i ffmpeg.input -i "${audioFilePath}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "output.mp4";
}

getRandomHandymanBrandingText()
{
    text[0]="Robinson Handy and Technology Services"
    text[1]="rhtservices.net"
    text[2]="@rhtservicesllc"
    text[3]="#rhtservicesllc"

    rand=$[$RANDOM % ${#text[@]}]
    # echo ${text[$rand]}
    return ${text[$rand]}
}

setVideoType()
{
    videoType=$(echo "$videoDirectory" | awk -F'.' '{print $NF}')

    echo "Video type: ${videoType}"

    case $videoType in
        handyman)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivehandyman"
            UPLOAD_DIRECTORY="${BASE_DIRECTORY}/uploadhandyman"
            PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processedhandyman"
            channelBrandText="rhtservices.net/home-improvement"
            ;;
        lightshow | techtalk)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivetechnology"
            UPLOAD_DIRECTORY="${BASE_DIRECTORY}/uploadtechnology"
            PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processedtechnology"
            channelBrandText="rhtservices.net/technology"
            ;;
        dashcam | fireworks | armchair)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivedashcam"
            UPLOAD_DIRECTORY="${BASE_DIRECTORY}/uploaddashcam"
            PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processeddashcam"
            channelBrandText="Kenny Ram Dash Cam"
            ;;
        toastmasters)
            ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archivetoastmasters"
            UPLOAD_DIRECTORY="${BASE_DIRECTORY}/uploadtoastmasters"
            PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processedtoastmasters"
            channelBrandText="towertoastmasters.org"
            ;;
        *)
            echo "Invalid video type."
            exit
            ;;
    esac
}

renderVideoByType()
{
    logMessage "Rendering video"

    case $videoType in
        dashcam | fireworks)
            createFfmpegInputFile mov

            renderVideoWithAudio
            ;;
            
        *)
            for videoFile in *mp4 *mov *mkv
            do
                normalizeAudio "${videoFile}"
            done

            # createFfmpegInputFile ts

            # renderVideo
            ;;
    esac
}

###############################################################################
###############################################################################
## main
###############################################################################
###############################################################################

set -x

logMessage "test"

debugMessage "debugging"

# checkForSingleProcess

# # while (true)
# # {


#     # deleteProcessedRawVideoFiles

# # cd ${INCOMING_DIRECTORY}
# changeToIncomingDirectory

# videoDirectory=$(getFirstDirectory)

# getFirstDirectory

#     # if [[ ${videoDirectory} == "" ]]; then
#     #     continue
#     # fi

# setVideoType

# createMissingDirectories

# cd ${videoDirectory}

# stopProcessingIfExcludedFilesExist ${videoDirectory}

# lowercaseAllFileNames
    
# renderVideoByType

#     # render video with graphics

#     # compressOutputVideoFile # archive final video; move archive

#     # moveVideoAsProcessed ${videoDirectory}
# # }
