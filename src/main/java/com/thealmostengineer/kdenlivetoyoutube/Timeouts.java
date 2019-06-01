package com.thealmostengineer.kdenlivetoyoutube;

public class Timeouts {
	int longTimeoutHours = 12;
	int shortTimeoutMinutes = 5;
	
	public int getLongTimeoutHours() {
		return longTimeoutHours;
	} // end function
	
	public int getLongTimeoutMinutes() {
		int timeoutMinutes = this.longTimeoutHours * 60;
		return timeoutMinutes;
	} // end function
	
	public int getShortTimeoutMinutes() {
		return shortTimeoutMinutes;
	} // end function
	
	public int getShortTimeoutSeconds() {
		int timeoutSeconds = this.shortTimeoutMinutes * 60;
		return timeoutSeconds;
	} // end function
}
