package com.thealmostengineer.kdenlivetoyoutube.filesystem;

import java.io.File;

import com.thealmostengineer.kdenlivetoyoutube.Logging;

/**
 * Manipulate the files and folders used by the process 
 * 
 * @author almostengr, Kenny Robinson
 *
 */
public class FileOperations {
		
	/**
	 * Deletes the folder and it's subfolders and files
	 * 
	 * @param directoryStr		The path to the folder to be deleted
	 */
	public static void deleteFolder(String directoryStr) {
		
		File directoryFile = new File(directoryStr);
		Logging.info("Deleting folder " + directoryFile.getAbsolutePath());
		
	    File[] files = directoryFile.listFiles();
	    if(files!=null) { //some JVMs return null for empty dirs
	        for(File f: files) {
	            if(f.isDirectory()) {
	                deleteFolder(f.getAbsolutePath());
	            } else {
	                f.delete();
	            } // end if
	        } // end for
	    } // end if
	    directoryFile.delete();
	} // end function
	
	/**
	 * Create a folder 
	 * 
	 * @param directoryStr	THe path to the folder to be created
	 * @return
	 */
	public static File createFolder(String directoryStr) {
		File directoryFile = new File(directoryStr);
		
		if (directoryFile.exists() == false) {
			directoryFile.mkdirs();
			Logging.info("Created directory " + directoryStr);
		} // end if
		
		return directoryFile;
	} // end function
	
	/**
	 * Get the list of files in the provided directory
	 * 
	 * @param directory		The directory to get the files for
	 * @return
	 */
	public static File[] getFilesInFolder(String directory) {
		Logging.info("Getting files in " + directory);
		
		File file = new File(directory);
		
		// create the directory if it does nto exist
		if (file.exists() == false) {
			createFolder(directory);
		} // end if
		
		File[] fileList = file.listFiles();
		
		return fileList;
	}  // end function
	
	public static int getCountOfFilesInFolder(String directory) {
		Logging.info("Getting count of files in " + directory);
		
		File file = new File(directory);
		
		if (file.exists() == false) {
			createFolder(directory);
		} // end if

		File[] fileList = file.listFiles();
		
		Logging.info("Found " + fileList.length + " items in directory " + directory);
		return fileList.length;
	} // end function
}
