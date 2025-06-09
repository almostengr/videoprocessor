# Video Processor

## Purpose

Ask any content creator and they will tell you that creating videos is a time consuming process.
For me, the most mundane and worse part of the process is waiting on the videos to render and waiting on
the uploads to complete.

Some video editors, have a version of their software that can be ran on a server to render the video files
created. By offloading the video rendering to another machine, you can edit more videos in less time.

## Solution

I created this script to automate the video creation process. Videos that I post on my various content
platforms have a predefined structure for each video type. Thus they are easy to automate. 

I am a member of organizations, where I am partically or fully responsible for the marketing and 
branding of that organization. Some of the marketing is done with the use of video. This script also 
accomidates those additional video types.

## My YouTube Channels

* [Dash Cam Channel](https://www.youtube.com/channel/UCB7rvymUaUbbig3skv2zvCQ?sub_confirmation=1)
* [Home Improvement Channel](https://www.youtube.com/channel/UC4HCouBLtXD1j1U_17aBqig?sub_confirmation=1)
* [Tech / Personal Channel](http://www.youtube.com/channel/UC4xp-TEEIAL-4XtMVvfRaQw?sub_confirmation=1)

## Video Schedule By Channel

* Kenny Ram Dash Cam - on Monday, Wednesdays, and Fridays
* RHT Services Home Improvement - on Saturdays
* RHT Services Tech Talk - on Tuesdays, sometimes Thursdays

## Crontab Command

Command below to render videos on a schedule.

```bash
5 0-5 * * * /home/almostengr/videoprocessor/source/videoprocessor.sh
```

Command will run 5 minutes after the hour between the hours of midnight (00:05) and 05:05. This time was selected
as it is the time that I would be using the computer. Thus the work I would be doing would not be slowed
down by the rendering process and visa versa.

## Additional FFMPEG Commands

Occasionally, there are some additional FFMPEG commands that I need to run to create the videos
that I desire to have. Since the commands are rarely used, I chose to not add the functionality
that uses these commands into the script, but instead note them here for future reference.

### Timelapse Video Without Audio

```bash
ffmpeg -i FILE0710.MOV -filter:v "setpts=0.5*PTS" -an FILE0710.timelapse.MOV
```

### Create Video without Audio

```bash
ffmpeg -i out165.mov -an -c:v copy uncut165.mp4
```

### Scale Video Resolution

```bash
ffmpeg -i out165.mov -an -vf scale=1920:1080 scaled165.mov
```

### Concat Video Timelapse Files

```bash
for file in *MP4
do
echo "file $file" >> input.txt
done

ffmpeg -f concat -i input.txt -c:v copy output.mp4
```

### Create Video With Image Files

```bash
ffmpeg -framerate 1/3 -pattern_type glob -i '*.jpg' -c:v libx264 -r 30 -pix_fmt yuv420p output.mp4
```

### Add Music To Video

```bash
fmpeg -i 20230401_110000.mp4 -i ../../../ytvideostructure/07music/mix03.mp3 -shortest -c:v copy -c:a copy -map 0:v:0 -map 1:a:0 output.mp4
```

### Convert Variable Rate Video To Constant Rate Video

```bash
ffmpeg -i input.mp4 -vf "setpts=1.0*PTS" -r 30 -c:v libx264 -crf 18 -c:a aac -b:a 192k output.mp4
```

### Normalize Audio

```bash
ffmpeg -y  -i input.mp4 -f lavfi -i anullsrc -vcodec copy -acodec aac -shortest output.mp3
```

### Rotate Video Clip

```bash
ffmpeg -i in.mov -vf "transpose=1" out.mov
```

For the transpose parameter you can pass:

* 0 = 90° counterclockwise and vertical flip (default)
* 1 = 90° clockwise
* 2 = 90° counterclockwise
* 3 = 90° clockwise and vertical flip

## References

* https://filme.imyfone.com/video-editing-tips/how-to-merge-or-combine-videos-using-ffmpeg/
* https://stackoverflow.com/questions/44280903/ffmpeg-vaapi-and-drawtext
* https://trac.ffmpeg.org/wiki/Hardware/VAAPI
* https://stackoverflow.com/questions/7333232/how-to-concatenate-two-mp4-files-using-ffmpeg
