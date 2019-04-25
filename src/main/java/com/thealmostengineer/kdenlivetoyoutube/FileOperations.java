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
		App.logMessage("Loading properties");
		
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
		
		App.logMessage("Saving the property to " + file.getAbsolutePath());
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
		App.logMessage("Deleting folder " + directoryFile.getAbsolutePath());
		
	    File[] files = directoryFile.listFiles();
	    if(files!=null) { //some JVMs return null for empty dirs
	        for(File f: files) {
	            if(f.isDirectory()) {
	                deleteFolder(f.getAbsolutePath());
	            } else {
	                f.delete();
	            }
	        }
	    }
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
			App.logMessage("Created directory " + directoryStr);
		}
		
		return directoryFile;
	} // end function
	@Override
	public String toString() {
		// TODO Auto-generated method stub
		return super.toString();
	}
	
	/**
	 * Get the list of files in the provided directory
	 * 
	 * @param directory		The directory to get the files for
	 * @return
	 */
	File[] getFilesInFolder(String directory) {
		App.logMessage("Getting files in " + directory);
		
		File file = new File(directory);
		File[] fileList = file.listFiles();
		
		App.logMessage("Done getting files in " + directory);
		return fileList;
	}  // end function
	
	/**
	 * Archive the project 
	 * 
	 * @param filePathToGz		The path to the compressed archive file
	 * @param archiveDirectory	The directory to move the compressed archive file to
	 * @throws Exception
	 */
	void archiveProject(String filePathToGz, String archiveDirectory) throws Exception {
		App.logMessage("Archiving project...");
		
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
		
		Process prcCompressGz = pbCompressGz.start();
		prcCompressGz.waitFor(300, TimeUnit.SECONDS);
		
		filePathToTar += ".gz"; // update file path
		
		createFolder(archiveDirectory);
		
		File gzFile = new File(filePathToTar);
		File gzArchiveFile = new File(archiveDirectory + gzFile.getName());
		gzFile.renameTo(gzArchiveFile); // equivalent of move file
		
		App.logMessage("Done archiving project");
	}
	
	/**
	 * Uncompress and untar the archive file
	 * 
	 * @param filePathToGz		The path to the project archive
	 * @param outputDirectory	The directory that the files should be untarred to
	 * @throws Exception
	 */
	void unpackageCompressTar(String filePathToGz, String outputDirectory) throws Exception {
		App.logMessage("Uncompressing file " + filePathToGz);
		
		// run gunzip on the file if it is compressed
		if (filePathToGz.endsWith(".gz")) {
			App.logMessage("Gz file: " + filePathToGz);
			
			ProcessBuilder pbGunzip = new ProcessBuilder("/bin/gunzip", filePathToGz);
			pbGunzip.inheritIO();
			pbGunzip.redirectError(Redirect.INHERIT);
			pbGunzip.redirectOutput(Redirect.INHERIT);
			
			App.logMessage("Starting gunzip for " + filePathToGz);
			
			Process processGunzip = pbGunzip.start();
			processGunzip.waitFor(60, TimeUnit.SECONDS);
		}
		
		createFolder(outputDirectory);
		
		String filePathToTar; 
		try {
			filePathToTar = filePathToGz.substring(0, filePathToGz.lastIndexOf(".gz"));
		} catch (Exception e) {
			filePathToTar = filePathToGz;
		}
		
		App.logMessage("Tar file: " + filePathToTar);
		
		ProcessBuilder pbUntar = new ProcessBuilder("/bin/tar", "-xf", filePathToTar, "-C", outputDirectory);
		pbUntar.inheritIO();
		pbUntar.redirectError(Redirect.INHERIT);
		pbUntar.redirectOutput(Redirect.INHERIT);
		
		App.logMessage("Untarring to " + outputDirectory);
		
		Process processUntar = pbUntar.start();
		processUntar.waitFor(60, TimeUnit.SECONDS);
		
 		App.logMessage("Done uncompressing file " + filePathToGz);
	}
}
