package main;

import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Service;
import service.FileLogger;

@Service
public class Worker {
	
	@Scheduled(fixedRate=5000)
	public void executeTask() {
		//Do your work here.
		//日志输出到程序根目录(classpath)
		String workDir = FileLogger.class.getResource("/").getPath();
		System.setProperty("WORKDIR", workDir);

		FileLogger logger = new FileLogger();
		logger.logInfo2file();
	}
}
