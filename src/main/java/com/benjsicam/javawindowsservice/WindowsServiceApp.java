package com.benjsicam.javawindowsservice;

import org.springframework.context.annotation.AnnotationConfigApplicationContext;

public class WindowsServiceApp {
	
	@SuppressWarnings("resource")
	public void start() {
		new AnnotationConfigApplicationContext(AppConfiguration.class);
	}
}
