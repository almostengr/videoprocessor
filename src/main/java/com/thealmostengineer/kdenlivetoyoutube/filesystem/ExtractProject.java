package com.thealmostengineer.kdenlivetoyoutube.filesystem;

import java.lang.ProcessBuilder.Redirect;
import java.util.concurrent.TimeUnit;

import com.thealmostengineer.kdenlivetoyoutube.Logging;
import com.thealmostengineer.kdenlivetoyoutube.Timeouts;

/**
 * Handles extracting / unarchiving the project in order to render the video
 * 
 * @author almostengr, Kenny Robinson
 *
 */
public class ExtractProject {
	
	/**
	 * Uncompress and untar the archive file
	 * 
	 * @param filePathToGz		The path to the project archive
	 * @param outputDirectory	The directory that the files should be untarred to
	 * @throws Exception
	 */
	public static void unpackageCompressTar(String filePathToGz, String outputDirectory) throws Exception {
		Logging.info("Uncompressing file " + filePathToGz);
		
		// run gunzip on the file if it is compressed
		if (filePathToGz.endsWith(".gz")) {
			Logging.info("Gz file: " + filePathToGz);
			
			ProcessBuilder pbGunzip = new ProcessBuilder("/bin/gunzip", filePathToGz);
			pbGunzip.inheritIO();
			pbGunzip.redirectError(Redirect.INHERIT);
			pbGunzip.redirectOutput(Redirect.INHERIT);
			
			Logging.info("Starting gunzip for " + filePathToGz);
			
			Process processGunzip = pbGunzip.start();
			processGunzip.waitFor(Timeouts.getShortTimeoutSeconds(), TimeUnit.SECONDS);
		} // end if
		
		FileOperations.createFolder(outputDirectory);
		
		String filePathToTar; 
		try {
			filePathToTar = filePathToGz.substring(0, filePathToGz.lastIndexOf(".gz"));
		} catch (Exception e) {
			filePathToTar = filePathToGz;
		} // end try
		
		Logging.info("Tar file: " + filePathToTar);
		
		ProcessBuilder pbUntar = new ProcessBuilder("/bin/tar", "-xf", filePathToTar, "-C", outputDirectory);
		pbUntar.inheritIO();
		pbUntar.redirectError(Redirect.INHERIT);
		pbUntar.redirectOutput(Redirect.INHERIT);
		
		Logging.info("Untarring to " + outputDirectory);
		
		Process processUntar = pbUntar.start();
		processUntar.waitFor(Timeouts.getShortTimeoutSeconds(), TimeUnit.SECONDS);
		
 		Logging.info("Done uncompressing file " + filePathToGz);
	} // end function
}
