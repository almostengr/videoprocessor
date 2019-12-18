package com.thealmostengineer.kdenlivetoyoutube.ffmpeg;

import java.io.IOException;
import java.lang.ProcessBuilder.Redirect;
import java.util.ArrayList;
import java.util.concurrent.TimeUnit;

import com.thealmostengineer.kdenlivetoyoutube.Logging;
import com.thealmostengineer.kdenlivetoyoutube.Timeouts;

public class RenderTimelapse {
	
	public static void renderTimelapse(String fullLengthFileName) throws IOException, InterruptedException {
		ArrayList<String> ffmpegArguments = new ArrayList<String>();
		
		ffmpegArguments.add("/usr/bin/ffmpeg");
		ffmpegArguments.add("-i");
		ffmpegArguments.add(fullLengthFileName);
		ffmpegArguments.add("-vf");
		ffmpegArguments.add("\"setpts=0.25*PTS\""); // adjust value for speed
		ffmpegArguments.add("-an"); // no audio option
		
		String timelapseFileName = fullLengthFileName.replace(".mp4", " Timelapse.mp4");
		ffmpegArguments.add(timelapseFileName);
		
		ProcessBuilder renderTimelapsePb = new ProcessBuilder(ffmpegArguments);

		renderTimelapsePb.inheritIO();
		renderTimelapsePb.redirectError(Redirect.INHERIT);
		renderTimelapsePb.redirectOutput(Redirect.INHERIT);
		
		Logging.info("Rendering video " + timelapseFileName);
		Process processRenderVideo = renderTimelapsePb.start();
		processRenderVideo.waitFor(Timeouts.getLongTimeoutHours(), TimeUnit.HOURS);	// wait for video to render
		Logging.info("Done rendering video " + timelapseFileName);
	}
}
