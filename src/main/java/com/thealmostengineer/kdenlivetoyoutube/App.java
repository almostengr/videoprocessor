package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.lang.ProcessBuilder.Redirect;
import java.util.Properties;
import java.util.concurrent.TimeUnit;

/**
 * Hello world!
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
	
	static File[] getPendingFiles(String pendingDirectory) {
		File file = new File(pendingDirectory);
		File[] fileList = file.listFiles();
		return fileList;
	}
	
	static void unpackageCompressTar(String filePathToGz, String outputDirectory) throws Exception {
		logMessage("Gz file: " + filePathToGz);
		ProcessBuilder pbGunzip = new ProcessBuilder("/bin/gunzip", filePathToGz);
		pbGunzip.inheritIO();
		pbGunzip.redirectError(Redirect.INHERIT);
		pbGunzip.redirectOutput(Redirect.INHERIT);
		
		logMessage("Starting gunzip for " + filePathToGz);
		Process processGunzip = pbGunzip.start();
		processGunzip.waitFor(60, TimeUnit.SECONDS);
		
		String filePathToTar = filePathToGz.substring(0, filePathToGz.lastIndexOf(".gz"));
		logMessage("Tar file: " + filePathToTar);
		ProcessBuilder pbUntar = new ProcessBuilder("/bin/tar", "-xf", filePathToTar, "-C", outputDirectory);
		pbUntar.inheritIO();
		pbUntar.redirectError(Redirect.INHERIT);
		pbUntar.redirectOutput(Redirect.INHERIT);
		
		logMessage("Starting gunzip for " + filePathToTar);
		logMessage("Untarring to " + outputDirectory);
		Process processUntar = pbUntar.start();
		processUntar.waitFor(60, TimeUnit.SECONDS);
		
		logMessage("Done untarring");
	}
	
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
			
			File[] pendingFiles = getPendingFiles(appProperty.getProperty("pendingDirectory"));
			
//			for (int i = 0; i < pendingFiles.length; i++) {
			for (int i = 0; i < 1; i++) {
				logMessage(pendingFiles[i].getAbsolutePath());
				unpackageCompressTar(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("outputDirectory"));
			}

			exitCode = 0;
		} catch (Exception e) {
			e.printStackTrace();
		}
    	
    	logMessage("Shutting down");
//    	if (kdenliveProcess != null) {
//    		logMessage("killing Kdenlive");
//    		kdenliveProcess.destroyForcibly();
//    	}
    	
    	System.exit(exitCode);
    }
}
