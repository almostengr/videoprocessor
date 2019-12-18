package com.thealmostengineer.kdenlivetoyoutube;

import java.io.FileInputStream;
import java.io.InputStream;
import java.util.Properties;

/**
 * 
 * @author almostengr, Kenny Robinson
 *
 */
public class LoadProperties {

	/**
	 * Load the properties file
	 * 
	 * @param propertiesFileName	The name of the properties file
	 * @return						Returns loaded Properties object
	 * @throws Exception
	 */
	public static Properties loadProperties(String propertiesFileName) throws Exception {
		Logging.info("Loading properties");
		
		InputStream inputStream = new FileInputStream(propertiesFileName); // check for the file
		Properties properties = new Properties(); 
		properties.load(inputStream); // load the properties
		inputStream.close(); // close the input file
		
		return properties;
	} // end function
}
