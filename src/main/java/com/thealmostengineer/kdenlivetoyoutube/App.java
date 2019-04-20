package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.lang.ProcessBuilder.Redirect;
import java.util.Properties;
import java.util.concurrent.TimeUnit;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.firefox.FirefoxDriver;

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
	
	static File[] getFilesInDirectory(String pendingDirectory) {
		File file = new File(pendingDirectory);
		File[] fileList = file.listFiles();
		return fileList;
	}

	static void deleteFolder(File folder) {
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
	}

	static WebDriver setTimeouts(WebDriver wDriver, int timeInSeconds) {
		wDriver.manage().timeouts().implicitlyWait(timeInSeconds, TimeUnit.SECONDS);
		wDriver.manage().timeouts().pageLoadTimeout(timeInSeconds, TimeUnit.SECONDS);
		return wDriver;
	}
	
	static void cleanRenderDirectory(String renderDirectory) {
		File directory = new File(renderDirectory);
		deleteFolder(directory);
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
	
	static void archiveProject(String filePathToTar, String archiveDirectory) throws Exception {
		
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
			
//			File[] pendingFiles = getFilesInDirectory(appProperty.getProperty("pendingDirectory"));
//			
//			for (int i = 0; i < pendingFiles.length; i++) {
//				logMessage(pendingFiles[i].getAbsolutePath());
//				
//				unpackageCompressTar(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("outputDirectory"));
//				
//				// TODO run the kdenlive melt command
//				
//				// TODO clean up the render directory 
//				
//				// TODO archive the tar ball
//			} // end for
//			
//			File[] videoFiles = getFilesInDirectory(appProperty.getProperty("outputDirectory"));
			WebDriver driver = null;
			
//			for (int i = 0; i < videoFiles.length; i++) {
				// if first loop, login to the youtube and to go upload page
//				if (i == 0) {
			if (true) {
//					System.setProperties(appProperty);
					System.setProperty("webdriver.gecko.driver", appProperty.getProperty("webdriver.gecko.driver")); // set location of gecko driver for Firefox
					
					driver = new FirefoxDriver();
					driver = setTimeouts(driver, 600);
					
					driver.manage().window().maximize();
					
					driver.get(appProperty.getProperty("youtubeUrl"));
					
					driver.findElement(By.xpath("//button[@aria-label='Create a video or post']")).click();
					
					driver.findElement(By.xpath("//yt-formatted-string[contains(text(),'Upload video')]")).click();
					
					// google login screen 
					driver.findElement(By.id("identifierId")).sendKeys(appProperty.getProperty("webUsername"));
					driver.findElement(By.name("password")).sendKeys(appProperty.getProperty("webPassword"));
					
					// after login
					
					driver.findElement(By.id("upload-button-text")).click();
//				}
				
				// TODO upload each video file
			}
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
