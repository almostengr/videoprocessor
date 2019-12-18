package com.thealmostengineer.kdenlivetoyoutube;

import java.util.logging.Logger;

public class Logging {
	private static Logger logger = Logger.getAnonymousLogger();
	
	public Logging() {
		System.setProperty("java.util.logging.SimpleFormatter.format", "[%1$tF %1$tT] [%4$-7s] %5$s %n");
	}
	
	public static void info(String message) {
		logger.info(message);
	}
	
	public static void error(String message) {
		severe(message);
	}
	
	public static void severe(String message) {
		logger.severe(message);
	}
}
