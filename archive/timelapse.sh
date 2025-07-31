#!/bin/bash

###############################################
## DESCRIPTION: Generate a timelapse video from the provided input file.
## AUTHOR: Kenny Robinson @almostengr
## DATE: 2020-04-30
## USAGE: timelapse.sh <filename> 
## <filename> is the name of the video file that a timelapse will be 
## generated for.
###############################################

function log_message {
    echo "$(/bin/date) ${1}"
}

log_message "Script starting"

FILENAME="${1}"

# input video file must be provided
if [[ "${FILENAME}" != "" ]]; then
	log_message "Input video file: ${FILENAME}"

	log_message "Modifying mp4 filename"

	MODFILENAME=$(echo ${FILENAME} | sed 's/.mp4//g')
	OUTPUTFILE="${MODFILENAME}.timelapse.mp4"

	if [[ "${FILENAME}" == *"mov" ]]; then
		log_message "Modifying .mov filename"

		MODFILENAME=$(echo ${FILENAME} | sed 's/.mov//g')
		OUTPUTFILE="${MODFILENAME}.timelapse.mov"

	elif [[ "${FILENAME}" == *"mkv" ]]; then
		log_message "Modifying .mkv filename"

		MODFILENAME=$(echo ${FILENAME} | sed 's/.mkv//g')
		OUTPUTFILE="${MODFILENAME}.timelapse.mkv"
	fi

	if [ -z "${TIMELAPSESPEED}" ]; then
		log_message "Timelapse speed not defined"
		TIMELAPSESPEED=0.5
	fi

	log_message "Generating timelapse video"

	# /usr/bin/ffmpeg -i ${FILENAME} -vf "setpts=0.75*PTS" -an ${OUTPUTFILE}
	/usr/bin/ffmpeg -i ${FILENAME} -vf "setpts=${TIMELAPSESPEED}*PTS" -an ${OUTPUTFILE}

	log_message "Done generating timelapse video"

else 
	log_message "The video file to timelapse was not provided."
fi
