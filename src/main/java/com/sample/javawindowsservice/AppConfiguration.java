package com.sample.javawindowsservice;

import org.springframework.context.annotation.ComponentScan;
import org.springframework.context.annotation.Configuration;
import org.springframework.scheduling.annotation.EnableScheduling;

@Configuration
@EnableScheduling
@ComponentScan(basePackages="com.sample.javawindowsservice")
public class AppConfiguration {
	
}
