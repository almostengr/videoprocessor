package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.lang.ProcessBuilder.Redirect;
import java.util.Properties;
import java.util.concurrent.TimeUnit;

import org.openqa.selenium.WebDriver;

import com.google.common.io.Files;

/**
 * Application to render videos with Kdenlive and upload them to YouTube
 * @author almostengineer, Kenny Robinson
 * 
 */
public class App 
{
	
	static int defaultTimeout = 120;
	
	static void logMessage(String message) {
		System.out.println(message);	
	}
	
	static Properties loadProperties(String propertiesFileName) throws Exception {
		logMessage("Loading properties");
		
		InputStream inputStream = new FileInputStream(propertiesFileName); // check for the file
		Properties properties = new Properties(); 
		properties.load(inputStream); // load the properties
		
		logMessage("Done loading properties");
		
		inputStream.close(); // close the input file
		return properties;
	}
	
	static Process startKdenlive(String kdenlivePath) throws Exception {
		logMessage("Starting kdenlive");
		ProcessBuilder processBuilder = new ProcessBuilder(kdenlivePath);
		processBuilder.inheritIO();
//		processBuilder.redirectError(Redirect.INHERIT);
//		processBuilder.redirectOutput(Redirect.INHERIT);
		
		Process process = processBuilder.start();
		
		return process;
	}

	static File[] getFilesInDirectory(String pendingDirectory) {
		logMessage("Getting files in " + pendingDirectory);
		File file = new File(pendingDirectory);
		File[] fileList = file.listFiles();
		logMessage("Done getting files in " + pendingDirectory);
		return fileList;
	}

	static void deleteFolder(File folder) {
		logMessage("Deleting folder " + folder.getAbsolutePath());
	    File[] files = folder.listFiles();
	    if(files!=null) { //some JVMs return null for empty dirs
	        for(File f: files) {
	            if(f.isDirectory()) {
	                deleteFolder(f);
	            } else {
	                f.delete();
	            }
	        }
	    }
	    folder.delete();
	    logMessage("Done deleting folder " + folder.getAbsolutePath());
	}

	static WebDriver setTimeouts(WebDriver wDriver, int timeInSeconds) {
		wDriver.manage().timeouts().implicitlyWait(timeInSeconds, TimeUnit.SECONDS);
		wDriver.manage().timeouts().pageLoadTimeout(timeInSeconds, TimeUnit.SECONDS);
		return wDriver;
	}
	
	static void unpackageCompressTar(String filePathToGz, String outputDirectory) throws Exception {
		logMessage("Uncompressing file " + filePathToGz);
		// run gunzip on the file if it is compressed
		if (filePathToGz.endsWith(".gz")) {
			logMessage("Gz file: " + filePathToGz);
			ProcessBuilder pbGunzip = new ProcessBuilder("/bin/gunzip", filePathToGz);
			pbGunzip.inheritIO();
			pbGunzip.redirectError(Redirect.INHERIT);
			pbGunzip.redirectOutput(Redirect.INHERIT);
			
			logMessage("Starting gunzip for " + filePathToGz);
			Process processGunzip = pbGunzip.start();
			processGunzip.waitFor(60, TimeUnit.SECONDS);
		}
		
		File outputDirectoryFile = new File(outputDirectory);
		if (outputDirectoryFile.exists() == false) {
			outputDirectoryFile.mkdirs();
			logMessage("Created render directory");
		}
		
		String filePathToTar = filePathToGz.substring(0, filePathToGz.lastIndexOf(".gz"));
		logMessage("Tar file: " + filePathToTar);
		ProcessBuilder pbUntar = new ProcessBuilder("/bin/tar", "-xf", filePathToTar, "-C", outputDirectory);
		pbUntar.inheritIO();
		pbUntar.redirectError(Redirect.INHERIT);
		pbUntar.redirectOutput(Redirect.INHERIT);
		
		logMessage("Untarring to " + outputDirectory);
		Process processUntar = pbUntar.start();
		processUntar.waitFor(60, TimeUnit.SECONDS);
		
 		logMessage("Done uncompressing file " + filePathToGz);
	}
	
	static void archiveProject(String filePathToGz, String archiveDirectory) throws Exception {
		logMessage("Archiving project...");

		String filePathToTar = filePathToGz.substring(0, filePathToGz.lastIndexOf(".gz"));
		
		ProcessBuilder pbCompressGz = new ProcessBuilder("/bin/gzip", filePathToTar);
		pbCompressGz.inheritIO();
		pbCompressGz.redirectError(Redirect.INHERIT);
		pbCompressGz.redirectOutput(Redirect.INHERIT);
		
		Process prcCompressGz = pbCompressGz.start();
		prcCompressGz.waitFor(300, TimeUnit.SECONDS);
		
		filePathToTar += ".gz"; // update file path
		
		File archiveFilePath = new File(archiveDirectory);
		if (archiveFilePath.exists() == false) {
			archiveFilePath.mkdirs();
			logMessage("Created archive directory");
		}
		
		File gzFile = new File(filePathToTar);
		File gzArchiveFile = new File(archiveDirectory + gzFile.getName());
		Files.move(gzFile, gzArchiveFile);
		logMessage("Done archiving project");
	}
	
	static void cleanupRenderDirectory(String renderDirectory) {
		logMessage("Cleaning up render directory");
		File videoOutputDirectory = new File(renderDirectory);
		deleteFolder(videoOutputDirectory);
		logMessage("Done cleaning up render directory");
	} // end function
	
	static void setProcessTimeout(String timeoutValue) {
		Integer setTimeout = Integer.parseInt(timeoutValue);
		if (setTimeout < 5) {
			setTimeout = 60;
		} // end if
	} // end function
	
    public static void main( String[] args )
    {
    	int exitCode = 1;
    	Process kdenliveProcess = null;
    	
    	try {
			Properties appProperty = loadProperties("testing.properties");
//			kdenliveProcess = startKdenlive(appProperty.getProperty("kdenlivePath"));
//			TimeUnit.SECONDS.sleep(15);
			
			File[] pendingFiles = getFilesInDirectory(appProperty.getProperty("pendingDirectory"));
			
			for (int i = 0; i < pendingFiles.length; i++) {
				
				logMessage("Processing " + pendingFiles[i].getAbsolutePath());
				
				unpackageCompressTar(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("renderDirectory"));
				
				// TODO run the kdenlive melt command
				
				// clean up the render directory
				cleanupRenderDirectory(appProperty.getProperty("renderDirectory"));
				
				// archive the tar ball
				archiveProject(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("archiveDirectory")); 
			} // end for
			
			// TODO if first loop, login to the youtube and to go upload page
			
			// TODO upload video via YouTube API 				
			
			exitCode = 0;
		} catch (Exception e) {
			logMessage("Exception occurred");
			e.printStackTrace();
		}
    
    	logMessage("Shutting down");
//    	if (kdenliveProcess != null) {
//    		logMessage("killing Kdenlive");
//    		kdenliveProcess.destroyForcibly();
//    	}
    	
    	System.exit(exitCode);
    } // end function
}
 