package pis;

import java.util.concurrent.TimeUnit;

import org.apache.commons.io.filefilter.FileFilterUtils;
import org.apache.commons.io.monitor.FileAlterationMonitor;
import org.apache.commons.io.monitor.FileAlterationObserver;
import org.apache.log4j.Logger;


public class MainClass {

	private static final Logger logger = Logger.getLogger(MainClass.class);
	/**
	 * @param args
	 */
	public static void main(String[] args) {
		// TODO Auto-generated method stub
		logger.info("服务开始!!!");  
		
		
		String filebom="D:\\EportPisServer\\message\\Transmission\\FileReceiveHHB\\bom";
        String fileimg="D:\\EportPisServer\\message\\Transmission\\FileReceiveHHB\\img";
        String filedec="D:\\EportPisServer\\message\\Transmission\\FileReceiveHHB\\dec";
		//Do your work here.
		try {

			// 轮询间隔 5 秒
			long interval = TimeUnit.SECONDS.toMillis(5);
			//
			FileAlterationObserver observerbom = new FileAlterationObserver(
					filebom,
					FileFilterUtils.and(
							FileFilterUtils.fileFileFilter()),null);
			FileAlterationObserver observerimg = new FileAlterationObserver(
					fileimg,
					FileFilterUtils.and(
							FileFilterUtils.fileFileFilter()),
					null);
			FileAlterationObserver observerdec = new FileAlterationObserver(
					filedec,
					FileFilterUtils.and(
							FileFilterUtils.fileFileFilter()),
					null);
			observerbom.addListener(new FileListenerAdaptor());
			// 配置Monitor，第一个参数单位是毫秒，是监听的间隔；第二个参数就是绑定我们之前的观察对象。
			FileAlterationMonitor filebomMonitor = new FileAlterationMonitor(interval,
					new FileAlterationObserver[] { observerbom });
			// 启动开始监听
			filebomMonitor.start();

			
			observerimg.addListener(new FileListenerAdaptor());
			// 配置Monitor，第一个参数单位是毫秒，是监听的间隔；第二个参数就是绑定我们之前的观察对象。
			FileAlterationMonitor fileimgMonitor = new FileAlterationMonitor(interval,
					new FileAlterationObserver[] { observerimg });
			// 启动开始监听
			fileimgMonitor.start();
			
			observerdec.addListener(new FileListenerAdaptor());
			// 配置Monitor，第一个参数单位是毫秒，是监听的间隔；第二个参数就是绑定我们之前的观察对象。
			FileAlterationMonitor filedecMonitor = new FileAlterationMonitor(interval,
					new FileAlterationObserver[] { observerdec });
			// 启动开始监听
			filedecMonitor.start();
			
		} catch (Exception e) {

			e.printStackTrace();

		}

	}

}
