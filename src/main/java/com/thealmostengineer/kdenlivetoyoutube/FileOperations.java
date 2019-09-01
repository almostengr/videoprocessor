package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.lang.ProcessBuilder.Redirect;
import java.util.Properties;
import java.util.concurrent.TimeUnit;

/**
 * Manipulate the files used by the process 
 * 
 * @author almostengineer
 *
 */
public class FileOperations {
	
	/**
	 * Load the properties file
	 * 
	 * @param propertiesFileName	The name of the properties file
	 * @return						Returns loaded Properties object
	 * @throws Exception
	 */
	Properties loadProperties(String propertiesFileName) throws Exception {
		App.logger.info("Loading properties");
		
		InputStream inputStream = new FileInputStream(propertiesFileName); // check for the file
		Properties properties = new Properties(); 
		properties.load(inputStream); // load the properties
		inputStream.close(); // close the input file
		
		return properties;
	} // end function
	
	/**
	 * Save the property to the properties file
	 * 
	 * @param property		The key of the property to save
	 * @param value			The value of the property to save
	 * @param fileName		The name of the file to save the property to
	 * @throws FileNotFoundException
	 * @throws IOException
	 */
	void saveProperty(String property, String value, String fileName) throws FileNotFoundException, IOException {
		Properties properties = new Properties();
		File file = new File(fileName);
		FileOutputStream fileOutputStream = new FileOutputStream(file);
		
		App.logger.info("Saving the property to " + file.getAbsolutePath());
		properties.setProperty(property, value);
		properties.store(fileOutputStream, "");
		fileOutputStream.close(); // close the file
	} // end function
	
	/**
	 * Deletes the folder and it's subfolders and files
	 * 
	 * @param directoryStr		The path to the folder to be deleted
	 */
	void deleteFolder(String directoryStr) {
		
		File directoryFile = new File(directoryStr);
		App.logger.info("Deleting folder " + directoryFile.getAbsolutePath());
		
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
	File createFolder(String directoryStr) {
		File directoryFile = new File(directoryStr);
		
		if (directoryFile.exists() == false) {
			directoryFile.mkdirs();
			App.logger.info("Created directory " + directoryStr);
		} // end if
		
		return directoryFile;
	} // end function
	
	/**
	 * Get the list of files in the provided directory
	 * 
	 * @param directory		The directory to get the files for
	 * @return
	 */
	File[] getFilesInFolder(String directory) {
		App.logger.info("Getting files in " + directory);
		
		File file = new File(directory);
		
		// create the directory if it does nto exist
		if (file.exists() == false) {
			this.createFolder(directory);
		} // end if
		
		File[] fileList = file.listFiles();
		
		return fileList;
	}  // end function
	
	int getCountOfFilesInFolder(String directory) {
		App.logger.info("Getting count of files in " + directory);
		
		File file = new File(directory);
		
		// create the directory if it does nto exist
		if (file.exists() == false) {
			this.createFolder(directory);
		} // end if

		File[] fileList = file.listFiles();
		
		App.logger.info("Found " + fileList.length + " items in directory " + directory);
		return fileList.length;
	} // end function
	
	/**
	 * Archive the project 
	 * 
	 * @param filePathToGz		The path to the compressed archive file
	 * @param archiveDirectory	The directory to move the compressed archive file to
	 * @throws Exception
	 */
	void archiveProject(String filePathToGz, String archiveDirectory) throws Exception {
		App.logger.info("Archiving project files");
		
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
		
		createFolder(archiveDirectory);
		
		File gzFile = new File(filePathToTar);
		File gzArchiveFile = new File(archiveDirectory + gzFile.getName());
		gzFile.renameTo(gzArchiveFile); // equivalent of move file
	} // end function
	
	/**
	 * Uncompress and untar the archive file
	 * 
	 * @param filePathToGz		The path to the project archive
	 * @param outputDirectory	The directory that the files should be untarred to
	 * @throws Exception
	 */
	void unpackageCompressTar(String filePathToGz, String outputDirectory) throws Exception {
		App.logger.info("Uncompressing file " + filePathToGz);
		
		Timeouts timeouts = new Timeouts();
		
		// run gunzip on the file if it is compressed
		if (filePathToGz.endsWith(".gz")) {
			App.logger.info("Gz file: " + filePathToGz);
			
			ProcessBuilder pbGunzip = new ProcessBuilder("/bin/gunzip", filePathToGz);
			pbGunzip.inheritIO();
			pbGunzip.redirectError(Redirect.INHERIT);
			pbGunzip.redirectOutput(Redirect.INHERIT);
			
			App.logger.info("Starting gunzip for " + filePathToGz);
			
			Process processGunzip = pbGunzip.start();
			processGunzip.waitFor(timeouts.getShortTimeoutSeconds(), TimeUnit.SECONDS);
		} // end if
		
		createFolder(outputDirectory);
		
		String filePathToTar; 
		try {
			filePathToTar = filePathToGz.substring(0, filePathToGz.lastIndexOf(".gz"));
		} catch (Exception e) {
			filePathToTar = filePathToGz;
		} // end try
		
		App.logger.info("Tar file: " + filePathToTar);
		
		ProcessBuilder pbUntar = new ProcessBuilder("/bin/tar", "-xf", filePathToTar, "-C", outputDirectory);
		pbUntar.inheritIO();
		pbUntar.redirectError(Redirect.INHERIT);
		pbUntar.redirectOutput(Redirect.INHERIT);
		
		App.logger.info("Untarring to " + outputDirectory);
		
		Process processUntar = pbUntar.start();
		processUntar.waitFor(timeouts.getShortTimeoutSeconds(), TimeUnit.SECONDS);
		
 		App.logger.info("Done uncompressing file " + filePathToGz);
	} // end function
}
