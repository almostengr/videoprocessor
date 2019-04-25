package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;

public class PreventDupeProcess {
	
	File processFile = null;

	void createProcessFile() throws IOException {
		this.processFile = File.createTempFile("kdenlivetoyoutube", ".tmp");
		this.processFile.deleteOnExit();
		
		SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
		Date date = new Date();
		
		App.logMessage("Creating process file" + this.processFile.getAbsolutePath());
		FileWriter fileWriter = new FileWriter(this.processFile);
		fileWriter.write("Started at " + dateFormat.format(date));
		fileWriter.close();
	} // end function
	
	boolean checkForDuplicateProcess() throws IOException {
		boolean returnValue = false;
		
		this.createProcessFile();
		
		App.logMessage("Checking for duplicate processes");
		
		FileOperations fileOperations = new FileOperations();
		File[] fileListing = fileOperations.getFilesInFolder(processFile.getPath());
	
		int counter = 0;
		for (int i = 0; i < fileListing.length; i++) {
			// check to see if multiple kdenlive processes have been started
			if (fileListing[i].getAbsolutePath().contains("kdenlivetoyoutube") && fileListing[i].getAbsolutePath().endsWith(".tmp")) {
				counter++;
			} // end if
			
			if (counter > 1) {
				App.logMessage("Process is already running. Exiting");
				returnValue = true;
			} // end if
		} // end for
		
		return returnValue;
	} // end function
}
