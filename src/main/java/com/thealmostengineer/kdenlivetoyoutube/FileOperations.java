package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.lang.ProcessBuilder.Redirect;
import java.util.Properties;
import java.util.concurrent.TimeUnit;

import com.google.common.io.Files;

/**
 * Manipulate the files used by the process 
 * 
 * @author almostengineer
 *
 */
public class FileOperations {
	
	Properties loadProperties(String propertiesFileName) throws Exception {
		App.logMessage("Loading properties");
		
		InputStream inputStream = new FileInputStream(propertiesFileName); // check for the file
		Properties properties = new Properties(); 
		properties.load(inputStream); // load the properties
		inputStream.close(); // close the input file
		
		return properties;
	} // end function
	
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
	
	File createFolder(String directoryStr) {
		File directoryFile = new File(directoryStr);
		
		if (directoryFile.exists() == false) {
			directoryFile.mkdirs();
			App.logMessage("Created directory " + directoryStr);
		}
		
		return directoryFile;
	} // end function
	
	File[] getFilesInFolder(String directory) {
		App.logMessage("Getting files in " + directory);
		
		File file = new File(directory);
		File[] fileList = file.listFiles();
		
		App.logMessage("Done getting files in " + directory);
		return fileList;
	}  // end function
	
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
		Files.move(gzFile, gzArchiveFile);
		
		App.logMessage("Done archiving project");
	}
	
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
