package com.sample.javawindowsservice;

import java.util.Date;

import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Service;

@Service
public class Worker {
	
	@Scheduled(fixedRate=5000)
	public void executeTask() {
		//Do your work here.
		System.out.println("Date/Time now: " + new Date());
	}
}
