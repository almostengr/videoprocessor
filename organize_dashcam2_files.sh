#!/bin/bash

################################################################
# Organize the dash cam 2 files by date into their own directory.
################################################################

INCOMING_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/videos/incoming"

# todo - use NR files in the future, but for now remove them

echo "Removing NR files"
rm *NR*mp4

for file in *NF*mp4
do
    fileDate=$(/usr/bin/ffprobe "${file}" 2>&1 | /usr/bin/grep "Input" | /usr/bin/awk -F '_' '{print $2}' | /usr/bin/head -c 8)

    echo "Moving ${file}"

    newDirectory="${INCOMING_DIRECTORY}/${fileDate}.dashcam2"

    /usr/bin/mkdir -p "$newDirectory"

    /usr/bin/mv "${file}" "${newDirectory}"
done
