package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.util.Properties;

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
	
    public static void main( String[] args )
    {
    	int exitCode = 1;
    	Properties appProperty;
    	
    	FileOperations fileOperations = new FileOperations();
    	KdenliveOperations kdenliveOperations = new KdenliveOperations();
    	
    	try {
    		if (args[0].isEmpty()) {
    			throw new Exception("Properties file not provided.");
    		}
    		
    		appProperty = fileOperations.loadProperties(args[0]);
			
			File[] pendingFiles = fileOperations.getFilesInFolder(appProperty.getProperty("pendingDirectory"));
			for (int i = 0; i < pendingFiles.length; i++) {
				try {
					logMessage("Processing " + pendingFiles[i].getAbsolutePath());
					
					// clean up the render directory
					fileOperations.deleteFolder(appProperty.getProperty("renderDirectory"));
					
					fileOperations.unpackageCompressTar(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("renderDirectory"));
					
					String kdenliveFileName = null;
					String videoOutputFileName = null;
					File renderDir = new File(appProperty.getProperty("renderDirectory"));
					File[] renderDirFile = renderDir.listFiles();
					
					for (int j = 0; j < renderDirFile.length; j++) {
						if (renderDirFile[j].getAbsolutePath().endsWith("kdenlive")) {
							kdenliveFileName = renderDirFile[j].getAbsolutePath();
							videoOutputFileName = appProperty.getProperty("outputDirectory") + kdenliveFileName.substring(kdenliveFileName.lastIndexOf("/")) + ".mp4";
							logMessage("Kdenlive: " + kdenliveFileName);
							logMessage("Video Output: " + videoOutputFileName);
							break;
						} // end if
					} // end for

					fileOperations.createFolder(appProperty.getProperty("outputDirectory"));
					kdenliveOperations.renderVideo(appProperty.getProperty("meltPath"), kdenliveFileName, videoOutputFileName);// run the kdenlive melt command
					
					// archive the tar ball
					fileOperations.archiveProject(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("archiveDirectory"));
				} catch (Exception e) {
					fileOperations.deleteFolder(appProperty.getProperty("renderDirectory"));
					e.getMessage();
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
		}
    
    	System.exit(exitCode);
    } // end function
}
 