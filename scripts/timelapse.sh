#!/bin/bash

echo "Current directory $(pwd)"

for filename in $(ls -1 | grep -v -i timelapse )
do
	/bin/date

	echo "Starting on ${filename}"

	/usr/bin/ffmpeg -i ${filename} -vf "setpts=0.75*PTS" -an ${filename}.Timelapse.mp4

	echo "Done with ${filename}"
done
