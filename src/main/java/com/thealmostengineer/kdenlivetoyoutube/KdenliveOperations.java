package com.thealmostengineer.kdenlivetoyoutube;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.lang.ProcessBuilder.Redirect;
import java.util.ArrayList;
import java.util.concurrent.TimeUnit;

/**
 * Perform actions related to Kdenlive
 * 
 * @author almostengineer
 *
 */
public class KdenliveOperations  {
	
	void renderVideo(String meltPath, String kdenliveFileName, String videoOutputFileName) throws Exception {
		if (meltPath.isEmpty()) {
			throw new Exception("Melt path is empty");
		}
		if (kdenliveFileName.isEmpty()) {
			throw new Exception("Kdenlive project file name is empty");
		}
		if (videoOutputFileName.isEmpty()) {
			throw new Exception("Video output file name is empty");
		}
		
		ArrayList<String> pbArguments = new ArrayList<String>();
		pbArguments.add(meltPath);
		
		App.logMessage("Reading kdenlive file for resolution information");
		File kdenliveFile = new File(kdenliveFileName);
		BufferedReader bufferedReader = new BufferedReader(new FileReader(kdenliveFile));
		String line = "";
		String resolution = "";
		while((line = bufferedReader.readLine()) != null) {
			if (line.contains("kdenlive:docproperties.profile")) {
				resolution = line.substring(line.indexOf(">")+1, line.indexOf("</prop"));
			} // end if
		} // end while
		bufferedReader.close();
		
//		pbArguments.add("atsc_720p_30"); 		// frame and resolution
		App.logMessage("Resolution: " + resolution);
//		pbArguments.add(resolution);			// frame and resolution
//		pbArguments.add("avformat");
//		pbArguments.add("-");
		pbArguments.add(kdenliveFileName); 			// source file
		pbArguments.add("-consumer");
//		pbArguments.add(videoOutputFileName);		// target file
		pbArguments.add("avformat:" + videoOutputFileName);		// target file
		pbArguments.add("properties=x264-medium");
		pbArguments.add("f=mp4");
		pbArguments.add("vcodec=libx264");
		pbArguments.add("acodec=aac");
		pbArguments.add("g=120");
		pbArguments.add("crf=23");
		pbArguments.add("ab=160k");
		pbArguments.add("preset=faster");
		pbArguments.add("threads=4");
		pbArguments.add("real_time=-1");
		pbArguments.add("-silent");
		
		ProcessBuilder pbRenderVideo = new ProcessBuilder(pbArguments);
		pbRenderVideo.inheritIO();
		pbRenderVideo.redirectError(Redirect.INHERIT);
		pbRenderVideo.redirectOutput(Redirect.INHERIT);
		
		App.logMessage("Rendering video " + videoOutputFileName);
		Process processRenderVideo = pbRenderVideo.start();
		processRenderVideo.waitFor(10, TimeUnit.HOURS);	// wait for video to render
		App.logMessage("Done rendering video " + videoOutputFileName);
	}
}
