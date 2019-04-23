package com.thealmostengineer.kdenlivetoyoutube;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.util.Properties;
import java.util.concurrent.TimeUnit;

/**
 * Application to render videos with Kdenlive and upload them to YouTube
 * 
 * @author almostengineer, Kenny Robinson
 * 
 */
public class App 
{
	static public void logMessage(String message) {
		System.out.println(message);	
	}
	
	static void checkDuplicateProcess() throws Exception {
		logMessage("Checking to see if existing process is already running");
		
		File processList = File.createTempFile("processlist", ".tmp");
		processList.deleteOnExit();
		
		ProcessBuilder processBuilder = new ProcessBuilder("/bin/ps", "-ef");
		processBuilder.inheritIO();
		processBuilder.redirectErrorStream(true);
		processBuilder.redirectOutput(processList);
		
		Process process = processBuilder.start();
		process.waitFor(15, TimeUnit.SECONDS);
		
		BufferedReader bufferedReader = new BufferedReader(new FileReader(processList));
		String line = "";
		String output = "";
		while ((line = bufferedReader.readLine()) != null) {
			output = line;
		} // end while
		bufferedReader.close();
		
		if (output.toLowerCase().contains(" melt")) {
			throw new Exception("Process already running");
		} // end if
	} // end function
	
    public static void main( String[] args )
    {
    	int exitCode = 1;
    	
    	try {
    		if (args[0].isEmpty()) {
    			throw new Exception("Properties file not provided.");
    		} // end if
        	 
        	FileOperations fileOperations = new FileOperations();
        	KdenliveOperations kdenliveOperations = new KdenliveOperations();
        	Properties appProperty = fileOperations.loadProperties(args[0]);
    		
			File[] pendingFiles = fileOperations.getFilesInFolder(appProperty.getProperty("pendingDirectory"));
			
			for (int i = 0; i < pendingFiles.length; i++) {
				try {
					if (pendingFiles[i].getAbsolutePath().endsWith(".gz") == false || pendingFiles[i].getAbsolutePath().endsWith(".tar") == false) {
						throw new Exception("Skipping file. Invalid extension");
					} // end if
					
					logMessage("Processing " + pendingFiles[i].getAbsolutePath());
					
					fileOperations.deleteFolder(appProperty.getProperty("renderDirectory")); // clean render directory
					
					fileOperations.unpackageCompressTar(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("renderDirectory"));
					
					String kdenliveFileName = null;
					String videoOutputFileName = null;
					File[] renderDirFile = fileOperations.getFilesInFolder(appProperty.getProperty("renderDirectory")); // renderDir.listFiles();
					
					for (int i2 = 0; i2 < renderDirFile.length; i2++) {
						if (renderDirFile[i2].getAbsolutePath().endsWith("kdenlive")) {
							kdenliveFileName = renderDirFile[i2].getAbsolutePath();
							videoOutputFileName = appProperty.getProperty("outputDirectory") + kdenliveFileName.substring(kdenliveFileName.lastIndexOf("/")) + ".mp4";
							logMessage("Kdenlive: " + kdenliveFileName);
							logMessage("Video Output: " + videoOutputFileName);
							break;
						} // end if
					} // end for

					fileOperations.createFolder(appProperty.getProperty("outputDirectory"));
					kdenliveOperations.renderVideo(appProperty.getProperty("meltPath"), kdenliveFileName, videoOutputFileName); // run the kdenlive melt command
					
					// archive the tar ball
					fileOperations.archiveProject(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("archiveDirectory"));
				} catch (Exception e) {
					fileOperations.deleteFolder(appProperty.getProperty("renderDirectory"));
					logMessage(e.getMessage());
					e.printStackTrace();
				} // end try
				
				logMessage("Done processing " + pendingFiles[i].getAbsolutePath());
			} // end for
			
			// TODO login to api on first go
			// TODO upload video via YouTube API
			
			exitCode = 0;
		} catch (Exception e) {
			logMessage(e.getMessage());
			e.printStackTrace();
		} // end catch
    
    	System.exit(exitCode);
    } // end function
}
 