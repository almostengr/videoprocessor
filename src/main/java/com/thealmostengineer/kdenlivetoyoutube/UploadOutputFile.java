package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.util.logging.Logger;

/**
 * 
 * @author almostengr, Kenny Robinson
 *
 */
public class UploadOutputFile {
	
	public static void uploadOutputFile(File videoOutputFile) {
		if (videoOutputFile.getAbsolutePath().toLowerCase().endsWith(".mp4")) {
//		if (true) {
			// login to api on first go
			Auth.setClientSecretsPath(App.appProperty.getProperty("client_secrets_file"));
			
			// upload video via YouTube API
			UploadVideo.uploadVideo(videoOutputFile.getAbsolutePath());
			
			FileOperations.deleteFolder(videoOutputFile.getAbsolutePath()); // delete file after uploading
		}
		else {
			Logging.info("Skipped file " + videoOutputFile.getAbsolutePath());
		} // end if
	}
}
