package main;

import org.apache.commons.io.filefilter.FileFilterUtils;
import org.apache.commons.io.monitor.FileAlterationMonitor;
import org.apache.commons.io.monitor.FileAlterationObserver;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Service;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.concurrent.TimeUnit;

@Service
public class Worker {
	
	@Scheduled(fixedRate=5000)
	public void executeTask() {

		String subPath="";
		Date today=new Date();
		SimpleDateFormat sdf=new SimpleDateFormat("yyyyMMdd");
		subPath=sdf.format(today);
		String AllPath="D:\\EportPisServer\\Transmission\\Backup\\FileReceive\\"+subPath;
		//Do your work here.
		try {

			// 轮询间隔 5 秒
			long interval = TimeUnit.SECONDS.toMillis(5);
			//
			FileAlterationObserver observer = new FileAlterationObserver(
					AllPath,
					FileFilterUtils.and(
							FileFilterUtils.fileFileFilter(),
							FileFilterUtils.suffixFileFilter(".java")),
					null);
			observer.addListener(new FileListenerAdaptor());
			// 配置Monitor，第一个参数单位是毫秒，是监听的间隔；第二个参数就是绑定我们之前的观察对象。
			FileAlterationMonitor fileMonitor = new FileAlterationMonitor(interval,
					new FileAlterationObserver[] { observer });
			// 启动开始监听
			fileMonitor.start();

		} catch (Exception e) {

			e.printStackTrace();

		}
	}
}
