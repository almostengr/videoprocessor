#!/bin/bash

##############################################
# Author: Kenny Robinson, @almostengr
# Video Tutorial: https://youtu.be/FEatUj8B0XA
# Description: Commands referenced from 
# https://scribbleghost.net/2018/10/26/merge-video-files-together-with-ffmpeg/
###############################################

/bin/date

/bin/echo "Current directory $(pwd)"

echo "Adding files to list"

echo "Adding video intro"

for filename in $(ls -1 *mp4 *MP4 *mov *MOV *mkv *MKV | grep -v output)
do
    /bin/echo "file '${filename}'" >> list.txt

    if [[ "${filename}" == *"mov" ]]; then
        OUTPUTFILE=output.mov
    elif [[ "${filename}" == *"mkv" ]]; then
	OUTPUTFILE=output.mkv
    else
        OUTPUTFILE=output.mp4
    fi
done

echo "Rendering video"

/usr/bin/ffmpeg -f concat -i list.txt -c copy ${OUTPUTFILE}

echo "Done rendering video"

echo "Performing cleanup"

/bin/date
