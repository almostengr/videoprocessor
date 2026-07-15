#!/bin/bash 

################################################################
# Organize the dash cam 3 files by date into their own directory.
################################################################

INCOMING_DIRECTORY="/mnt/d74511ce-4722-471d-8d27-05013fd521b3/videos/dashcam3/"

DIRS=(
    "/media/almostengr/AZDOME/video_front"
    "/media/almostengr/AZDOME/video_front_lock"
    "/media/almostengr/AZDOME/video_back"
    "/media/almostengr/AZDOME/video_back_lock"
)

for srcDir in "${DIRS[@]}"; do
    cd "$srcDir" || exit 1

    for file in *mp4
    do 
        # fileDate=$(/usr/bin/ffprobe "${file}" 2>&1 | /usr/bin/grep "Input" | /usr/bin/awk -F '_' '{print $2}' | /usr/bin/head -c 8)
        fileDate=$(/usr/bin/ffprobe "${file}" 2>&1 | grep Input | awk -F ' ' '{print $5}' | awk -F '_' '{print $1}' | sed "s/'//g")

        echo "Copying $file (from $srcDir) -> ${INCOMING_DIRECTORY}/${fileDate}"

        newDirectory="${INCOMING_DIRECTORY}/${fileDate}"

        /usr/bin/mkdir -p "$newDirectory"

        /usr/bin/cp -p "${file}" "${newDirectory}"
    done
done