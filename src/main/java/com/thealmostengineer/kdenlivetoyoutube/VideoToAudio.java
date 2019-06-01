package com.thealmostengineer.kdenlivetoyoutube;

import java.lang.ProcessBuilder.Redirect;
import java.util.ArrayList;
import java.util.concurrent.TimeUnit;

public class VideoToAudio {
	String audioOutputFileName;
	
	/**
	 * Setting of the audioOutputFilename
	 * 
	 * @param videoFileName		The name of the video file that the audio file will be created from.
	 */
	protected void setAudioOutputFileName(String videoFileName) {
		this.audioOutputFileName = videoFileName; // take value passed in
		this.audioOutputFileName = this.audioOutputFileName.substring(0, this.audioOutputFileName.lastIndexOf(".mp4")); // remove mp4 extension
		this.audioOutputFileName = this.audioOutputFileName + ".mp3"; // append the mp3 extension
		App.logMessage("Audio filename: " + this.audioOutputFileName);
	} // end function
	
	/**
	 * Returns the audio file name
	 * @return 	Audio file name as a string
	 */
	String getAudioOutputFileName() {
		return this.audioOutputFileName;
	} // end function
	
	/**
	 * Calls ffmpeg to convert the video file provided to an audio file.
	 * 
	 * @param videoFileName		The name of the video file to convert to an audio file
	 * @param ffmpegPath		The path to ffmpeg
	 * @throws Exception
	 */
	void convertVideoToAudio(String videoFileName, String ffmpegPath) throws Exception {
		setAudioOutputFileName(videoFileName);
		App.logMessage("Converting video to audio " + this.audioOutputFileName);
		
		// command line arguments
		ArrayList<String> pbArguments = new ArrayList<String>();

		// split the arguments provided from the properties file for the ffmpeg path
		String[] ffmpegPathSplit = ffmpegPath.split(" ");
		for (int i = 0; i < ffmpegPathSplit.length; i++) {
			pbArguments.add(ffmpegPathSplit[i]);
		} // end for
		
		pbArguments.add("-i");	// input file argument
		pbArguments.add(videoFileName);	// input video file name
		pbArguments.add("-vn");
		pbArguments.add("-acodec"); // output file format codec
		pbArguments.add("mp3"); // output file format
		pbArguments.add(this.audioOutputFileName); // output video file
		
		ProcessBuilder processBuilder = new ProcessBuilder(pbArguments);
		processBuilder.inheritIO();
		processBuilder.redirectError(Redirect.INHERIT);		// write the error to the console
		processBuilder.redirectOutput(Redirect.INHERIT);	// write the output to the console
		
		Process process = processBuilder.start();
		process.waitFor(10, TimeUnit.HOURS); // wait for process to complete
		App.logMessage("Done converting video to audio " + this.audioOutputFileName);
	} // end function
}
