package com.thealmostengineer.kdenlivetoyoutube;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.lang.ProcessBuilder.Redirect;
import java.util.ArrayList;
import java.util.concurrent.TimeUnit;
import java.util.logging.Logger;

/**
 * Perform actions related to Kdenlive
 * 
 * @author almostengr, Kenny Robinson
 *
 */
public class RenderFullLength  {
	
	/**
	 * Calls melt after to render the video
	 * 
	 * @param meltPath				The path to melt
	 * @param kdenliveFileName		The name of the kdenlive project file
	 * @param videoOutputFileName	The name of the video that will be rendered
	 * @throws IOException 
	 * @throws InterruptedException 
	 * @throws Exception
	 */
	public static void renderVideo(String meltPath, String kdenliveFileName, String videoOutputFileName) throws IOException, InterruptedException  {
		if (meltPath.isEmpty()) {
			throw new RuntimeException("Melt path is empty");
		} // end if
		if (kdenliveFileName.isEmpty()) {
			throw new RuntimeException("Kdenlive project file name is empty");
		} // end if
		if (videoOutputFileName.isEmpty()) {
			throw new RuntimeException("Video output file name is empty");
		} // end if
		
		String[] meltPathSplit = meltPath.split(" ");
		ArrayList<String> pbArguments = new ArrayList<String>();
//		pbArguments.add(meltPath);
		for (int i = 0; i < meltPathSplit.length; i++) {
			pbArguments.add(meltPathSplit[i]);
		} // end for
		
		Logging.info("Reading kdenlive file for resolution information");
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
		
		Logging.info("Resolution: " + resolution);
		pbArguments.add(kdenliveFileName); 			// source file
		pbArguments.add("-consumer");
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
		
		Logging.info("Rendering video " + videoOutputFileName);
		Process processRenderVideo = pbRenderVideo.start();
		processRenderVideo.waitFor(Timeouts.getLongTimeoutHours(), TimeUnit.HOURS);	// wait for video to render
		Logging.info("Done rendering video " + videoOutputFileName);
	} // end function
}
