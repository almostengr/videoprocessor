package com.thealmostengineer.kdenlivetoyoutube;

import java.io.FileInputStream;
import java.io.InputStream;
import java.util.Properties;
import java.util.logging.Logger;

/**
 * 
 * @author almostengr, Kenny Robinson
 *
 */
public class LoadProperties {
	static Logger logger = App.logger;

	/**
	 * Load the properties file
	 * 
	 * @param propertiesFileName	The name of the properties file
	 * @return						Returns loaded Properties object
	 * @throws Exception
	 */
	public static Properties loadProperties(String propertiesFileName) throws Exception {
		logger.info("Loading properties");
		
		InputStream inputStream = new FileInputStream(propertiesFileName); // check for the file
		Properties properties = new Properties(); 
		properties.load(inputStream); // load the properties
		inputStream.close(); // close the input file
		
		return properties;
	} // end function
}
