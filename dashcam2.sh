#!/bin/bash

# Get the directory where the current script resides
SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &> /dev/null && pwd)

# Source the file using the absolute path
source "$SCRIPT_DIR/common.sh"

DEBUG=1
# BASE_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/testvideo"
BASE_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/video/dashcam2"

ctaDuration=12
subscribeBoxColor="green"
bgBoxColor="green"
channelBrandText="KENNY RAM DASH CAM"

INCOMING_DIRECTORY="${BASE_DIRECTORY}/incoming"
PROCESSED_DIRECTORY="${BASE_DIRECTORY}/processed"
ARCHIVE_DIRECTORY="${BASE_DIRECTORY}/archive"
ACTIVE_FILE="${BASE_DIRECTORY}/.active.txt"

flipRearCameraFiles() {
	rearCameraFiles=$(ls -1 *NR* | wc -l)

	if [ $rearCameraFiles -gt 0 ]; then
		for rearCameraFile in *NR*mp4
		do
			ffmpeg -i "${rearCameraFile}" -vf "vflip" "${rearCameraFlie}.flipped.mp4"

			# find the front file

			# render with rear file overlaid with front file

			# remove rear and front files

			# rename rendered file
            mv "${rearCameraFile}.flipped.mp4" "${rearCameraFile}"
		done
	fi
}

selectCallToAction() {
    case $dayOfWeek in
        0)
        subscribeBoxText="ONE WAY TO SUPPORT THE CHANNEL - PLEASE SUBSCRIBE"
        ;;

        1)
        subscribeBoxText="NEXT EXIT: SUBSCRIBE"
        ;;

        2)
        subscribeBoxText="MERGE INTO THE COMMUNITY - SUBSCRIBE NOW!"
        ;;

        4)
        subscribeBoxText="STOP AND SUBSCRIBE"
        ;;

        *)
        subscribeBoxText="HELP THE CHANNEL GROW BY SUBSCRIBING NOW!"
        ;;
    esac
}

exitWhenActiveFilePresent

createMissingDirectories

changeToIncomingDirectory

# for videoDirectory in $(ls -1 "${INCOMING_DIRECTORY}")
for videoDirectory in */
do
    # fullVideoDirectory="${INCOMING_DIRECTORY}/${videoDirectory}"
    fullVideoDirectory="${videoDirectory}"

    cd "${fullVideoDirectory}" || exit

    exitWhenExcludedFilesPresent

    removePreviousRenderFiles

    lowercaseAllFileNames

    createFfmpegInputFile mp4

    selectMixTrack

    selectCallToAction

    debugMessage "Creating output with graphics file"

    # render the video file without graphics included
    ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i ffmpeg.input -i "${MIX_AUDIO_TRACK_FILE}" -filter_hw_device foo -vf "format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 "outputNoGraphics.mp4"

    commandReturnCode=$?
    if [ $commandReturnCode -gt 0 ]; then
        infoMessage "Rendering with CPU"
        ffmpeg -y -hide_banner -f concat -safe 0 -i ffmpeg.input -i "${MIX_AUDIO_TRACK_FILE}" -shortest -map 0:v:0 -map 1:a:0 "outputNoGraphics.mp4"

        commandReturnCode=$?
        if [ $commandReturnCode -gt 0 ]; then
            errorMessage "Unable to render with CPU"
            mv "${fullVideoDirectory}" "${ERROR_DIRECTORY}"
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

    debugMessage "Creating output with graphics file"

    ffmpeg -y -hide_banner -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i outputNoGraphics.mp4 -filter_hw_device foo -vf "${videoGraphicsFilter}, format=vaapi|nv12,hwupload" -vcodec h264_vaapi -shortest -c:a copy outputFinal.mp4

    commandReturnCode=$?
    if [ $commandReturnCode -gt 0 ]; then
        infoMessage "Rendering with CPU"
        ffmpeg -y -hide_banner -i outputNoGraphics.mp4 -vf "${videoGraphicsFilter}" -shortest -c:a copy outputFinal.mp4;

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
