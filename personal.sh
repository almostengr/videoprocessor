#!/bin/bash

# Get the directory where the current script resides
SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &> /dev/null && pwd)

# Source the file using the absolute path
source "$SCRIPT_DIR/common.sh"

DEBUG=1
# BASE_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/testvideo"
BASE_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/videos/techtalk"

bgBoxColor="black"
channelBrandText="@ALMOSTENGR"
subscribeBoxText="SUBSCRIBE AND FOLLOW TO SEE MORE SOFTWARE AND TECH PROJECTS"

INCOMING_DIRECTORY="${BASE_DIRECTORY}/incoming"
PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processed"
ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive"
ACTIVE_FILE="${BASE_DIRECTORY}/.active.txt"

exitWhenActiveFilePresent

createMissingDirectories

changeToIncomingDirectory

for videoDirectory in */
do
    fullVideoDirectory="${videoDirectory}"

    cd "${fullVideoDirectory}" || exit

    exitWhenExcludedFilesPresent

    removePreviousRenderFiles

    lowercaseAllFileNames

    for videoFile in "$(pwd)"/*.mp4
    do
        # audioCount=$(/usr/bin/ffprobe -hide_banner "${videoFile}" 2>&1 | grep -i audio | wc -l)
        audioFile="${videoFile}.mp3"

        # convert video file to audio file
        ffmpeg -y -hide_banner -i "${videoFile}" -vn "${audioFile}"

        ## analyze audio volume
        maxVolume=$(/usr/bin/ffmpeg -y -hide_banner -i "${audioFile}" -af "volumedetect" -vn -sn -dn -f null /dev/null 2>&1 | grep max_volume | awk -F ' ' '{print $5}')

        if [ "${maxVolume}" != "0.0" ]; then
            maxVolume=$(echo $maxVolume | tr -d '-')

            tempAudioFile="temp.mp3"
            /usr/bin/ffmpeg -y -hide_banner -i "${audioFile}" -af "volume=${maxVolume}" "$tempAudioFile"

            /bin/mv "$tempAudioFile" "$audioFile"
        fi

        ## Create TS formatted file
        tsFile="${videoFile}.ts"

        /usr/bin/ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -safe 0 -i "${videoFile}" -i "${audioFile}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "${tsFile}";

        conversionReturnCode=$?
        if [ $conversionReturnCode -gt 0 ]; then
            infoMessage "Using CPU conversion for ${tsFile}"
            /usr/bin/ffmpeg -y -hide_banner -i "${videoFile}" -i "${audioFile}" -shortest -map 0:v:0 -map 1:a:0 "${tsFile}";

            commandReturnCode=$?
            if [ $commandReturnCode -gt 0 ]; then
                errorMessage "Unable to render with CPU"
            fi
        fi
    done

    createFfmpegInputFile ts

    # render the video file without graphics included
    if [ -e vertical ]; then
        renderVerticalVideo
        
        commandReturnCode=$?
        if [ $commandReturnCode -gt 0 ]; then
            errorMessage "Unable to render with CPU"
        fi
    else
        ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i ffmpeg.input -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi "outputNoGraphics.mp4";
    
        commandReturnCode=$?
        if [ $commandReturnCode -gt 0 ]; then
            infoMessage "Rendering with CPU"
            ffmpeg -y -hide_banner -f concat -i ffmpeg.input "outputNoGraphics.mp4";

            commandReturnCode=$?
            if [ $commandReturnCode -gt 0 ]; then
                errorMessage "Unable to render with CPU"
            fi
        fi
    fi

    # add graphics to video
    videoGraphicsFilter="drawtext=textfile:'${channelBrandText}':fontcolor=white@0.6:fontsize=${fontSize}:${UPPERRIGHT}:box=1:boxcolor=${bgBoxColor}@0.4:boxborderw=10"

    if [ "${subscribeBoxText}" != "" ]; then
        videoGraphicsFilter="${videoGraphicsFilter},drawtext=text='${subscribeBoxText}':fontcolor=white:box=1:boxcolor=${subscribeBoxColor}@1:boxborderw=20:fontsize=${fontSize}:${LOWERLEFT}:enable='if(lt(t,10),0,if(lt(mod(t-10,${brandDelaySeconds}),${ctaDuration}),1,0))'"
    fi

    if [ "${followPageText}" != "" ]; then
        videoGraphicsFilter="${videoGraphicsFilter},drawtext=text='${followPageText}':fontcolor=white:box=1:boxcolor=${followBoxColor}@1:boxborderw=20:fontsize=${fontSize}:${LOWER_LEFT1}:enable='if(lt(t,10),0,if(lt(mod(t-10,${brandDelaySeconds}),${ctaDuration}),1,0))'"
    fi

    debugMessage "Creating output without graphics file"

    ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -safe 0 -i outputNoGraphics.mp4 -filter_hw_device foo -vf "${videoGraphicsFilter}, format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -c:a copy outputFinal.mp4

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

    # archive the file video file and move it
    tarballArchiveFile="${videoDirectory}.tar.xz"

    # infoMessage "Archiving video file ${tarballArchiveFile}"
    tar -cJf "$tarballArchiveFile" outputNoGraphics.mp4

    returnCode=$(archiveVideoDirectory)

    if [ ${returnCode} -gt 0 ]; then
        errorMessage "Unable to archive video file."
        mv "${fullVideoDirectory}" "${ERROR_DIRECTORY}"
    fi
    mv "${tarballArchiveFile}" "${ARCHIVE_DIRECTORY}/${tarballArchiveFile}"

    infoMessage "Moving video directory to Processed folder"
    changeToIncomingDirectory
    mv "${videoDirectory}" "${PROCESSED_DIRECTORY}"
done 

removeActiveFile
