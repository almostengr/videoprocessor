package com.thealmostengineer.kdenlivetoyoutube;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Scanner;

public class PropertiesOperations {

	File outputFile = null;
	ArrayList<String> propertiesList = null;
	
	public void setOutputFile(File outputFile) {
		this.outputFile = outputFile;
	}

	public void setPropertiesList(ArrayList<String> propertiesList) {
		this.propertiesList = propertiesList;
	}

	public PropertiesOperations(String filePath, ArrayList<String> properties) throws Exception {
		// TODO Auto-generated constructor stub
		if (filePath.isEmpty()) {
			setOutputFile(File.createTempFile("temp", ".properties"));
		}
		else {
			setOutputFile(new File(filePath));
		}
		
		setPropertiesList(propertiesList);
	}
	
	void createPropertiesFile() throws FileNotFoundException, IOException {
		Scanner scanner = new Scanner(System.in);
		
		for (int i = 0; i < this.propertiesList.size(); i++) {
			System.out.println("Enter value for " + this.propertiesList.get(i) + ":");
			
			String input = scanner.nextLine();
			
			FileOperations fileOperations = new FileOperations();
			fileOperations.saveProperty(this.propertiesList.get(i), input, outputFile.getAbsolutePath());
		}
		scanner.close();
		
		App.logMessage("Properties file saved to " + outputFile.getAbsolutePath());
	}
}
