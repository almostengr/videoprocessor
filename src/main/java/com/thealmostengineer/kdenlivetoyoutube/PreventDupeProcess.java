package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.logging.Logger;

/**
 * @author almostengr, Kenny Robinson
 *
 */
public class PreventDupeProcess {
	static Logger logger = App.logger;
	private static File processFile = null;

	public static void createProcessFile() throws IOException {
		processFile = File.createTempFile("kdenlivetoyoutube", ".tmp");
		processFile.deleteOnExit();
		
		SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
		Date date = new Date();
		
		logger.info("Creating process file" + processFile.getAbsolutePath());
		FileWriter fileWriter = new FileWriter(processFile);
		fileWriter.write("Started at " + dateFormat.format(date));
		fileWriter.close();
	} // end function
	
	public static boolean checkForDuplicateProcess() throws IOException {
		boolean returnValue = false;
		
		createProcessFile();
		
		logger.info("Checking for duplicate processes");
		
		File[] fileListing = FileOperations.getFilesInFolder(processFile.getParent());
	
		int counter = 0;
		for (int i = 0; i < fileListing.length; i++) {
			
			// check to see if multiple kdenlive processes have been started
			if (fileListing[i].getAbsolutePath().contains("kdenlivetoyoutube") && 
					fileListing[i].getAbsolutePath().endsWith(".tmp")) {
				counter++;
			} // end if
			
			if (counter > 1) {
				throw new RuntimeException("Process is already running.");
			} // end if
		} // end for
		
		return returnValue;
	} // end function
}
