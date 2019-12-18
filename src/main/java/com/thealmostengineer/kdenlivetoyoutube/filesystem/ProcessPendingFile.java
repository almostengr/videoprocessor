package com.thealmostengineer.kdenlivetoyoutube.filesystem;

import java.io.File;

import com.thealmostengineer.kdenlivetoyoutube.App;
import com.thealmostengineer.kdenlivetoyoutube.Logging;
import com.thealmostengineer.kdenlivetoyoutube.ffmpeg.RenderTimelapse;
import com.thealmostengineer.kdenlivetoyoutube.kdenlive.RenderFullLength;

/**
 * 
 * @author almostengr, Kenny Robinson
 *
 */
public class ProcessPendingFile {
	
	public static void processPendingFile(File pendingFile) {
		try {
			if (pendingFile.getAbsolutePath().toLowerCase().endsWith(".gz") || 
					pendingFile.getAbsolutePath().toLowerCase().endsWith(".tar")) {
			
				Logging.info("Processing " + pendingFile.getAbsolutePath());
				
				FileOperations.deleteFolder(App.appProperty.getProperty("renderDirectory")); // clean render directory
				ExtractProject.unpackageCompressTar(pendingFile.getAbsolutePath(), App.appProperty.getProperty("renderDirectory"));
				
				String kdenliveFileName = null;
				String videoOutputFileName = null;
				File[] renderDirFile = FileOperations.getFilesInFolder(App.appProperty.getProperty("renderDirectory")); // renderDir.listFiles();
				
				for (int i2 = 0; i2 < renderDirFile.length; i2++) {
					if (renderDirFile[i2].getAbsolutePath().endsWith("kdenlive")) {
						kdenliveFileName = renderDirFile[i2].getAbsolutePath();
						videoOutputFileName = App.appProperty.getProperty("outputDirectory") + 
								kdenliveFileName.substring(kdenliveFileName.lastIndexOf("/")) + ".mp4";
						videoOutputFileName = videoOutputFileName.replace(".kdenlive", "");
						Logging.info("Kdenlive: " + kdenliveFileName);
						Logging.info("Video Output: " + videoOutputFileName);
						break;
					} // end if
				} // end for
				
				FileOperations.createFolder(App.appProperty.getProperty("outputDirectory"));
				RenderFullLength.renderVideo(App.appProperty.getProperty("meltPath"), kdenliveFileName, videoOutputFileName); // run the kdenlive melt command
				
				if (App.appProperty.getProperty("timelapse").equalsIgnoreCase("yes")) {
					RenderTimelapse.renderTimelapse(videoOutputFileName);
				}
				ArchiveProject.archiveProject(pendingFile.getAbsolutePath(), App.appProperty.getProperty("archiveDirectory"));
			} // end if
		} catch (Exception e) {
			Logging.info(e.getMessage());
			e.printStackTrace();
		} // end try
		
		Logging.info("Done processing " + pendingFile.getAbsolutePath());
	}
}
