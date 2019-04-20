package com.thealmostengineer.kdenlivetoyoutube;

import java.io.FileInputStream;
import java.io.IOException;
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
	
    public static void main( String[] args )
    {
    	int exitCode = 1;
    	Process kdenliveProcess = null;
    	
    	try {
			Properties appProperty = loadProperties("testing.properties");
			kdenliveProcess = startKdenlive(appProperty.getProperty("kdenlivePath"));

	    	TimeUnit.SECONDS.sleep(15);
			exitCode = 0;
		} catch (Exception e) {
			e.printStackTrace();
		}
    	
    	logMessage("Shutting down");
    	if (kdenliveProcess != null) {
    		logMessage("killing Kdenlive");
    		kdenliveProcess.destroyForcibly();
    	}
    	
    	System.exit(exitCode);
    }
}
