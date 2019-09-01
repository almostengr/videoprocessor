package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Properties;
import java.util.logging.Logger;

/**
 * Application to render videos with Kdenlive and upload them to YouTube
 * 
 * @author almostengineer, Kenny Robinson
 * 
 */
public class App 
{
	
	static Logger logger = Logger.getAnonymousLogger();
	
    public static void main( String[] args )
    {
    	int exitCode = 1; // default the exit code to failure
    	
    	try {
    		if (args[0].isEmpty()) {
    			throw new Exception("Properties file not provided.");
    		} // end if
        	 
        	FileOperations fileOperations = new FileOperations();
        	KdenliveOperations kdenliveOperations = new KdenliveOperations();
        	Properties appProperty = fileOperations.loadProperties(args[0]);
        	PreventDupeProcess preventDupeProcess = new PreventDupeProcess();
        	
        	preventDupeProcess.checkForDuplicateProcess();
    		
        	// render video archives in the pending directory
        	
			File[] pendingFiles = fileOperations.getFilesInFolder(appProperty.getProperty("pendingDirectory"));
			fileOperations.getCountOfFilesInFolder(appProperty.getProperty("pendingDirectory"));
			
			for (int i = 0; i < pendingFiles.length; i++) {
				try {
					if (pendingFiles[i].getAbsolutePath().toLowerCase().endsWith(".gz") || 
							pendingFiles[i].getAbsolutePath().toLowerCase().endsWith(".tar")) {
					
						logger.info("Processing " + pendingFiles[i].getAbsolutePath());
						
						fileOperations.deleteFolder(appProperty.getProperty("renderDirectory")); // clean render directory
						fileOperations.unpackageCompressTar(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("renderDirectory"));
						
						String kdenliveFileName = null;
						String videoOutputFileName = null;
						File[] renderDirFile = fileOperations.getFilesInFolder(appProperty.getProperty("renderDirectory")); // renderDir.listFiles();
						
						for (int i2 = 0; i2 < renderDirFile.length; i2++) {
							if (renderDirFile[i2].getAbsolutePath().endsWith("kdenlive")) {
								kdenliveFileName = renderDirFile[i2].getAbsolutePath();
								videoOutputFileName = appProperty.getProperty("outputDirectory") + kdenliveFileName.substring(kdenliveFileName.lastIndexOf("/")) + ".mp4";
								logger.info("Kdenlive: " + kdenliveFileName);
								logger.info("Video Output: " + videoOutputFileName);
								break;
							} // end if
						} // end for
	
						fileOperations.createFolder(appProperty.getProperty("outputDirectory"));
						kdenliveOperations.renderVideo(appProperty.getProperty("meltPath"), kdenliveFileName, videoOutputFileName); // run the kdenlive melt command
						
						// dont convert file to audio
//						try {
//							String generateAudio = appProperty.getProperty("generateaudio");
//							
//							if (generateAudio.toLowerCase().equals("yes")) {
//								VideoToAudio videoToAudio = new VideoToAudio();
//								videoToAudio.convertVideoToAudio(videoOutputFileName, appProperty.getProperty("ffmpegPath"));
//							} // end if
//						} catch (NullPointerException e) {
//							logger.info("Generate audio value not defined in properties file");
//						} // end try
						
						// archive the tar ball
						fileOperations.archiveProject(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("archiveDirectory"));
						
					} // end if
				} catch (Exception e) {
					logger.info(e.getMessage());
					e.printStackTrace();
				} // end try
				
				logger.info("Done processing " + pendingFiles[i].getAbsolutePath());
			} // end for

			// do uploading of files in output directory
			
			File[] videoOutputFiles = fileOperations.getFilesInFolder(appProperty.getProperty("outputDirectory"));
			fileOperations.getCountOfFilesInFolder(appProperty.getProperty("outputDirectory"));
			
			// limit the number of files uploaded to prevent hitting the quota
			int videoFileCounter = videoOutputFiles.length;
			if (videoFileCounter > 3) {
				videoFileCounter = 2;
			} // end if
			
			for (int i = 0; i < videoFileCounter; i++) {
				if (videoOutputFiles[i].getAbsolutePath().toLowerCase().endsWith(".mp4")) {
					// login to api on first go
					Auth.setClientSecretsPath(appProperty.getProperty("client_secrets_file"));
					
					// upload video via YouTube API
					UploadVideo.uploadVideo(videoOutputFiles[i].getAbsolutePath());
					
					fileOperations.deleteFolder(videoOutputFiles[i].getAbsolutePath()); // delete file after uploading
				}
				else {
					logger.info("Skipped file " + videoOutputFiles[i].getAbsolutePath());
				} // end if
			} // end for
			 
			exitCode = 0;
		} catch (Exception e) {
			logger.info("Error Detail: " + e.getMessage());
			e.printStackTrace();
		} // end catch
    
    	logger.info("Exiting, " + exitCode);
    	System.exit(exitCode);
    } // end function
}
 