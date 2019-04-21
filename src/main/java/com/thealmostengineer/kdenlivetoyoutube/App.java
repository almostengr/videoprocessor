package com.thealmostengineer.kdenlivetoyoutube;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileReader;
import java.io.InputStream;
import java.lang.ProcessBuilder.Redirect;
import java.util.ArrayList;
import java.util.Properties;
import java.util.concurrent.TimeUnit;

import com.google.common.io.Files;

/**
 * Application to render videos with Kdenlive and upload them to YouTube
 * 
 * @author almostengineer, Kenny Robinson
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
		inputStream.close(); // close the input file
		
		logMessage("Done loading properties");
		return properties;
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

	static void createDirectory(String directoryPath) {
		File outputDirectoryFile = new File(directoryPath);
		if (outputDirectoryFile.exists() == false) {
			outputDirectoryFile.mkdirs();
			logMessage("Created directory " + directoryPath);
		}
	} // end function
	
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
		
		createDirectory(outputDirectory);
		
		String filePathToTar; 
		try {
			filePathToTar = filePathToGz.substring(0, filePathToGz.lastIndexOf(".gz"));
		} catch (Exception e) {
			filePathToTar = filePathToGz;
		}
		
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
		
		createDirectory(archiveDirectory);
		
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
	
	static void renderVideo(String meltPath, String kdenliveFileName, String videoOutputFileName) throws Exception {
		
		if (meltPath.isEmpty()) {
			throw new Exception("Melt path is empty");
		}
		if (kdenliveFileName.isEmpty()) {
			throw new Exception("Kdenlive project file name is empty");
		}
		if (videoOutputFileName.isEmpty()) {
			throw new Exception("Video output file name is empty");
		}
		
		ArrayList<String> pbArguments = new ArrayList<String>();
		pbArguments.add(meltPath);
		
		logMessage("Reading kdenlive file for resolution information");
		File kdenliveFile = new File(kdenliveFileName);
		BufferedReader bufferedReader = new BufferedReader(new FileReader(kdenliveFile));
		String line = "";
		String resolution = "";
		while((line = bufferedReader.readLine()) != null) {
			if (line.contains("kdenlive:docproperties.profile")) {
				resolution = line.substring(line.indexOf(">")+1, line.indexOf("</prop"));
			} // end if
		} // end while
		bufferedReader.close();
		
//		pbArguments.add("atsc_720p_30"); 		// frame and resolution
		logMessage("Resolution: " + resolution);
//		pbArguments.add(resolution);			// frame and resolution
//		pbArguments.add("avformat");
//		pbArguments.add("-");
		pbArguments.add(kdenliveFileName); 			// source file
		pbArguments.add("-consumer");
//		pbArguments.add(videoOutputFileName);		// target file
		pbArguments.add("avformat:" + videoOutputFileName);		// target file
		pbArguments.add("properties=x264-medium");
		pbArguments.add("f=mp4");
		pbArguments.add("vcodec=libx264");
		pbArguments.add("acodec=aac");
		pbArguments.add("g=120");
		pbArguments.add("crf=23");
		pbArguments.add("ab=160k");
		pbArguments.add("preset=faster");
		pbArguments.add("threads=4");
		pbArguments.add("real_time=-1");
		pbArguments.add("-silent");
		
		ProcessBuilder pbRenderVideo = new ProcessBuilder(pbArguments);
		pbRenderVideo.inheritIO();
		pbRenderVideo.redirectError(Redirect.INHERIT);
		pbRenderVideo.redirectOutput(Redirect.INHERIT);
		
		logMessage("Rendering video " + videoOutputFileName);
		Process processRenderVideo = pbRenderVideo.start();
		processRenderVideo.waitFor(10, TimeUnit.HOURS);	// wait for video to render
		logMessage("Done rendering video " + videoOutputFileName);
	}
	
    public static void main( String[] args )
    {
    	int exitCode = 1;
    	Properties appProperty;
    	
    	try {
//    		appProperty = loadProperties("testing.properties");
    		if (args[0].isEmpty()) {
    			throw new Exception("Properties file not provided.");
    		}
    		
    		appProperty = loadProperties(args[0]);
			
			File[] pendingFiles = getFilesInDirectory(appProperty.getProperty("pendingDirectory"));
			for (int i = 0; i < pendingFiles.length; i++) {
				try {
					logMessage("Processing " + pendingFiles[i].getAbsolutePath());
					
					// clean up the render directory
					cleanupRenderDirectory(appProperty.getProperty("renderDirectory"));
					
					unpackageCompressTar(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("renderDirectory"));
					
					String kdenliveFileName = null;
					String videoOutputFileName = null;
					File renderDir = new File(appProperty.getProperty("renderDirectory"));
					File[] renderDirFile = renderDir.listFiles();
					
					for (int j = 0; j < renderDirFile.length; j++) {
						if (renderDirFile[j].getAbsolutePath().endsWith("kdenlive")) {
							kdenliveFileName = renderDirFile[j].getAbsolutePath();
							videoOutputFileName = appProperty.getProperty("outputDirectory") + kdenliveFileName.substring(kdenliveFileName.lastIndexOf("/")) + ".mp4";
							logMessage("Kdenlive: " + kdenliveFileName);
							logMessage("Video Output: " + videoOutputFileName);
							break;
						} // end if
					} // end for

					createDirectory(appProperty.getProperty("outputDirectory"));
					renderVideo(appProperty.getProperty("meltPath"), kdenliveFileName, videoOutputFileName);// run the kdenlive melt command
					
					// archive the tar ball
					archiveProject(pendingFiles[i].getAbsolutePath(), appProperty.getProperty("archiveDirectory"));
				} catch (Exception e) {
					cleanupRenderDirectory(appProperty.getProperty("renderDirectory"));
					e.getMessage();
					e.printStackTrace();
				} // end try
				
				logMessage("Done processing " + pendingFiles[i].getAbsolutePath());
			} // end for
			
			File[] videoFiles = getFilesInDirectory(appProperty.getProperty("outputDirectory"));
			for (int i = 0; i < videoFiles.length; i++) {
				// TODO login to api on first go
				// TODO upload video via YouTube API
			} // end for
			
			exitCode = 0;
		} catch (Exception e) {
//			logMessage("Exception occurred");
			logMessage(e.getMessage());
			e.printStackTrace();
		}
    
    	System.exit(exitCode);
    } // end function
}
 