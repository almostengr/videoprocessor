# Video Processor

## Purpose

Ask any content creator and they will tell you that creating videos is a time consuming process.
For me, the most mundane and worse part of the process is waiting on the videos to render and uploading them
to YouTube.

Some video editors, have a version of their software that can be ran on the server to render the video files
created. By offloading the video rendering to another machine, you can edit more videos in less time.

Purpose of this application is to automate the video creation process. Videos that I post on both of my YouTube
channels have a predefined structure for each video. Thus they are easy to automate. When additional
content or on screen text needs to be displayed, the automation for that particular channel is modified
accordingly.

Thumbnails are generated using a custom webpage and Selenium Webdriver. Selenium Webdriver is used 
to connect to a specified URL which includes the video title in the URL. That title is use to 
display text on the page. From there, Webdriver then takes a screenshot of the page with the title, 
so that it can be used for the video thumbnail.

## Additional Resources

* Video on [Getting API Key to Use with YouTube API](https://www.youtube.com/watch?v=JbWnRhHfTDA)
* Video on [Walk-through of Upload Video](https://www.youtube.com/watch?v=pb_t5_ShQOM)

## My YouTube Channels

* [Dash Cam Channel](https://www.youtube.com/channel/UCB7rvymUaUbbig3skv2zvCQ?sub_confirmation=1)
* [Home Improvement Channel](https://www.youtube.com/channel/UC4HCouBLtXD1j1U_17aBqig?sub_confirmation=1)
* [Technology Channel](http://www.youtube.com/channel/UC4xp-TEEIAL-4XtMVvfRaQw?sub_confirmation=1)

## Video Schedule By Channel

* Kenny Ram Dash Cam - Videos released weekly
* Robinson Handy and Technology Services - Videos released on 2nd and 4th Saturdays
* Tech Talk with RHT Services - released on 1st and 3rd Tuesdays
* Latter two channels may release videos on 5th Saturday or Tuesday respectively
* [rhtservices.net](https://rhtservices.net) website will be updated monthly with the transcriptions of the videos released

## References

* https://www.youtube.com/watch?v=mp3mDT_x6ho
* https://filme.imyfone.com/video-editing-tips/how-to-merge-or-combine-videos-using-ffmpeg/
* https://stackoverflow.com/questions/44280903/ffmpeg-vaapi-and-drawtext
* https://trac.ffmpeg.org/wiki/Hardware/VAAPI
* https://stackoverflow.com/questions/7333232/how-to-concatenate-two-mp4-files-using-ffmpeg
