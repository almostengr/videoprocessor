#!/bin/bash

# Get the directory where the current script resides
SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &> /dev/null && pwd)

# Source the file using the absolute path
source "$SCRIPT_DIR/dashcam_common.sh"

DEBUG=1
BASE_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/videos/dashcam3"

INCOMING_DIRECTORY="${BASE_DIRECTORY}/incoming"
PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processed"
ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive"
ACTIVE_FILE="${BASE_DIRECTORY}/.active.txt"

mergeFrontAndRearFiles() {
    ## merge front and rear files
    for frontVideo in *f*mp4
    do
        backVideo=$(echo $frontVideo | sed 's/f/b/g')

        # /usr/bin/ffmpeg -i 2026-07-11_12_46_26_f.mp4 -i 2026-07-11_12_46_26_b.mp4 -filter_complex "[1:v]scale=480:270[back];[0:v][back]overlay=W-w-20:H-h-20:format=auto" -c:v libx264 -preset veryfast -crf 23 -c:a aac -b:a 128k -shortest output2.mp4
        /usr/bin/ffmpeg -i "${frontVideo}" -i "${backVideo}" -filter_complex "[1:v]scale=480:270[back];[0:v][back]overlay=W-w-20:H-h-20:format=auto" -c:v libx264 -preset veryfast -crf 23 -an -shortest "${frontVideo}.ts"
    done
}

exitWhenActiveFilePresent

createMissingDirectories

changeToIncomingDirectory

# for videoDirectory in $(ls -1 "${INCOMING_DIRECTORY}")
for videoDirectory in */
do
    check_disk_space
    
    # fullVideoDirectory="${INCOMING_DIRECTORY}/${videoDirectory}"
    fullVideoDirectory="${videoDirectory}"

    cd "${fullVideoDirectory}" || exit

    exitWhenExcludedFilesPresent

    removePreviousRenderFiles

    lowercaseAllFileNames

    mergeFrontAndRearFiles

    createFfmpegInputFile ts

    selectMixTrack

    selectCallToAction

    debugMessage "Creating output with graphics file"

    # render the video file without graphics included
    /usr/bin/ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i /usr/bin/ffmpeg.input -i "${MIX_AUDIO_TRACK_FILE}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "outputNoGraphics.mp4"

    commandReturnCode=$?
    if [ $commandReturnCode -gt 0 ]; then
        infoMessage "Rendering with CPU"
        /usr/bin/ffmpeg -y -hide_banner -f concat -safe 0 -i /usr/bin/ffmpeg.input -i "${MIX_AUDIO_TRACK_FILE}" -shortest -map 0:v:0 -map 1:a:0 "outputNoGraphics.mp4"

        commandReturnCode=$?
        if [ $commandReturnCode -gt 0 ]; then
            errorMessage "Unable to render with CPU"
            mv "${fullVideoDirectory}" "${ERROR_DIRECTORY}"
        fi
    fi

    # add graphics to video
    videoGraphicsFilter="drawtext=textfile:'${channelBrandText}':fontcolor=${textColor}@0.6:fontsize=${fontSize}:${UPPERRIGHT}:box=1:boxcolor=${bgBoxColor}@0.4:boxborderw=10"

    if [ "${subscribeBoxText}" != "" ]; then
        videoGraphicsFilter="${videoGraphicsFilter},drawtext=text='${subscribeBoxText}':fontcolor=${textColor}:box=1:boxcolor=${subscribeBoxColor}@1:boxborderw=20:fontsize=${fontSize}:${LOWERLEFT}:enable='if(lt(t,10),0,if(lt(mod(t-10,${brandDelaySeconds}),${ctaDuration}),1,0))'"
    fi

    if [ "${followPageText}" != "" ]; then
        videoGraphicsFilter="${videoGraphicsFilter},drawtext=text='${followPageText}':fontcolor=${textColor}:box=1:boxcolor=${followBoxColor}@1:boxborderw=20:fontsize=${fontSize}:${LOWER_LEFT1}:enable='if(lt(t,10),0,if(lt(mod(t-10,${brandDelaySeconds}),${ctaDuration}),1,0))'"
    fi

    debugMessage "Creating output with graphics file"

    /usr/bin/ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i outputNoGraphics.mp4 -filter_hw_device foo -vf "${videoGraphicsFilter}, format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -c:a copy outputFinal.mp4

    commandReturnCode=$?
    if [ $commandReturnCode -gt 0 ]; then
        infoMessage "Rendering with CPU"
        /usr/bin/ffmpeg -y -hide_banner -i outputNoGraphics.mp4 -vf "${videoGraphicsFilter}" -shortest -c:a copy outputFinal.mp4;

        commandReturnCode=$?
        if [ $commandReturnCode -gt 0 ]; then
            errorMessage "Unable to render with CPU"
            mv "${fullVideoDirectory}" "${ERROR_DIRECTORY}"
        fi
    fi

    # move output file
    mv outputFinal.mp4 "${ARCHIVE_DIRECTORY}/${videoDirectory}.mp4"

    # archive the file video file and move it
    tarballArchiveFile="${videoDirectory}.tar.xz"
    tar -cJf "$tarballArchiveFile" outputNoGraphics.mp4

    returnCode=$?
    if [ ${returnCode} -gt 0 ]; then
        errorMessage "Unable to archive video file."
        mv "${fullVideoDirectory}" "${ERROR_DIRECTORY}"
    fi
    mv "${tarballArchiveFile}" "${ARCHIVE_DIRECTORY}/${tarballArchiveFile}"

    # move video directory to Processed directory
    infoMessage "Moving video directory to Processed folder"
    changeToIncomingDirectory
    mv "${videoDirectory}" "${PROCESSED_DIRECTORY}"
done

removeActiveFile
