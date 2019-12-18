package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.lang.ProcessBuilder.Redirect;
import java.util.concurrent.TimeUnit;
import java.util.logging.Logger;

/**
 * Handles archiving the project after the video has been rendered. 
 * 
 * @author almostengr, Kenny Robinson
 *
 */
public class ArchiveProject {

	static Logger logger = App.logger;
	
	/**
	 * Archive the project 
	 * 
	 * @param filePathToGz		The path to the compressed archive file
	 * @param archiveDirectory	The directory to move the compressed archive file to
	 * @throws Exception
	 */
	public static void archiveProject(String filePathToGz, String archiveDirectory) throws Exception {
		logger.info("Archiving project files");
		
		String filePathToTar; 
		try {
			filePathToTar = filePathToGz.substring(0, filePathToGz.lastIndexOf(".gz"));
		} catch (Exception e) {
			filePathToTar = filePathToGz;
		} // end try
		
		ProcessBuilder pbCompressGz = new ProcessBuilder("/bin/gzip", filePathToTar);
		pbCompressGz.inheritIO();
		pbCompressGz.redirectError(Redirect.INHERIT);
		pbCompressGz.redirectOutput(Redirect.INHERIT);
		
		Timeouts timeouts = new Timeouts();
		Process prcCompressGz = pbCompressGz.start();
		prcCompressGz.waitFor(timeouts.getShortTimeoutSeconds(), TimeUnit.SECONDS);
		
		filePathToTar += ".gz"; // update file path
		
		FileOperations.createFolder(archiveDirectory);
		
		File gzFile = new File(filePathToTar);
		File gzArchiveFile = new File(archiveDirectory + gzFile.getName());
		gzFile.renameTo(gzArchiveFile); // equivalent of move file
	} // end function
}
