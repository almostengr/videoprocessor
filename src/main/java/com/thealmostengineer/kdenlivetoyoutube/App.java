package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.Properties;

/**
 * Hello world!
 *
 */
public class App 
{
	static void writeProperty(String propertyKey, String propertyValue) throws IOException {
		File propertyFile = File.createTempFile("prop", "properties");
		OutputStream outputStream = new FileOutputStream(propertyFile);
		Properties properties = new Properties();
		
		System.out.println(propertyFile.getAbsolutePath());
		
		properties.setProperty(propertyKey, propertyValue);
		
		properties.store(outputStream, null); // save the property to the file
		
		outputStream.close(); // close the file
	}
	
	static void readProperty(String propertyKey) throws Exception {
		File propertyFile = new File("/tmp/prop5097072791725845215properties");
		InputStream inputStream = new FileInputStream(propertyFile);
		Properties properties = new Properties();
		
		properties.load(inputStream);
		
		System.out.println("Key: " + propertyKey  + "; Value: " + properties.getProperty(propertyKey));
		
		inputStream.close();
	}
	
    public static void main( String[] args )
    {
//        System.out.println( "Hello World!" );
		try {
//			writeProperty("firstname", "Kenny");
			readProperty("firstname");
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
    }
}
