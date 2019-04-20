package com.thealmostengineer.kdenlivetoyoutube;

import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;

/**
 * Hello world!
 *
 */
public class App 
{
    public static void main( String[] args )
    {
    	// read the properties file
    	try {
			InputStream inputStream = new FileInputStream("testing.properties");
			
			Properties properties = new Properties();
			
			properties.load(inputStream);
			
			System.out.println("Website: " + properties.getProperty("website"));
			System.out.println("First Name: " + properties.getProperty("firstname"));
			
			inputStream.close();
		} catch (FileNotFoundException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
    }
}
