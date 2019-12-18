package com.thealmostengineer.kdenlivetoyoutube;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.concurrent.TimeUnit;
import java.util.logging.Logger;

/**
 * 
 * @author almostengineer, Kenny Robinson
 *
 */
public class CheckFreeSpace {
	static Logger logger = App.logger;

	public static void checkFreeSpace() throws IOException, InterruptedException, Exception {
		// df -h . | awk '{print $5}'
		logger.info("Checking disk space");
		ArrayList<String> pbArguments = new ArrayList<String>();
		
		pbArguments.add("df");
		pbArguments.add("-h");
		pbArguments.add(".");
		pbArguments.add("|");
		pbArguments.add("awk");
		pbArguments.add("'{print $5}'");
		
		ProcessBuilder pbCheckDiskSpace = new ProcessBuilder(pbArguments);
		pbCheckDiskSpace.inheritIO();
		pbCheckDiskSpace.redirectErrorStream(true);
		
		Process process = pbCheckDiskSpace.start();
		process.waitFor(20, TimeUnit.SECONDS);
		BufferedReader bufferedReader = new BufferedReader(new InputStreamReader(process.getInputStream()));
		String output = null;
		
		while ((output = bufferedReader.readLine()) != null) {
			if (output.toLowerCase().contains("use")) {
				logger.info("Header line. Skipping");
			}
			else {
				logger.info("Disk space: " + output);
				int diskSpacePct = Integer.parseInt(output.replace("%", ""));
				logger.info("Disk space: " + diskSpacePct);
				if (diskSpacePct >= 80) {
					throw new Exception("Not generating video as process may run out of disk space.");
				} // end if
			} // end if
		} // end while
		
		logger.info("Done checking disk space");
	}
}
