package main;

import java.io.File;
import java.util.concurrent.TimeUnit;

import org.apache.commons.io.filefilter.FileFilterUtils;
import org.apache.commons.io.monitor.FileAlterationListenerAdaptor;
import org.apache.commons.io.monitor.FileAlterationMonitor;
import org.apache.commons.io.monitor.FileAlterationObserver;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class FileListenerAdaptor  extends FileAlterationListenerAdaptor{
	
		private static final Logger logger = LoggerFactory.getLogger(FileListenerAdaptor.class);
		
		 /** 
		  * File system observer started checking event. 
		  */  
		 @Override  
		 public void onStart(FileAlterationObserver observer) {  
		  // TODO Auto-generated method stub  
		  super.onStart(observer);  
		  System.out.println("监听开始！");
		  logger.info("文件系统观察者开始检查事件");  
		 }  
		  
		 /** 
		  * File system observer finished checking event. 
		  */  
		 @Override  
		 public void onStop(FileAlterationObserver observer) {  
		  // TODO Auto-generated method stub  
		  super.onStop(observer);  
		  System.out.println("监听结束！");
		  logger.info("文件系统完成检查事件观测器");  
		 }  
		  
		 /** 
		  * Directory created Event. 
		  */  
		 @Override  
		 public void onDirectoryCreate(File directory) {  
		  // TODO Auto-generated method stub  
		  super.onDirectoryCreate(directory);  
		  System.out.println("目录创建成功！");
		  logger.info("目录创建事件");  
		 }  
		  
		 /** 
		  * Directory changed Event 
		  */  
		 @Override  
		 public void onDirectoryChange(File directory) {  
		  // TODO Auto-generated method stub  
		  super.onDirectoryChange(directory); 
		  System.out.println("目录更改成功！");
		  logger.info("目录改变事件");  
		 }  
		  
		 /** 
		  * Directory deleted Event. 
		  */  
		 @Override  
		 public void onDirectoryDelete(File directory) {  
		  // TODO Auto-generated method stub  
		  super.onDirectoryDelete(directory);  
		  System.out.println("目录删除成功！");
		  logger.info("目录删除事件");  
		 }  
		  
		 /** 
		  * File created Event. 
		  */  
		 @Override  
		 public void onFileCreate(File file) {  
		  // TODO Auto-generated method stub  
		  super.onFileCreate(file);  
		  System.out.println("文件创建成功！");
		  
		  logger.info("文件创建事件");  
		  logger.info("文件名称：" + file.getName());  
		  
		 }  
		  
		 /** 
		  * File changed Event. 
		  */  
		 @Override  
		 public void onFileChange(File file) {  
		  // TODO Auto-generated method stub  
		  super.onFileChange(file);  
		  System.out.println("文件修改成功！");
		  logger.info("文件改变事件");  
		 }  
		  
		 /** 
		  * File deleted Event. 
		  */  
		 @Override  
		 public void onFileDelete(File file) {  
		  // TODO Auto-generated method stub  
		  super.onFileDelete(file);  
		  System.out.println("文件删除成功！");
		  logger.info("文件删除事件:" + file.getName());  
		 }  
		
			public static void main(String[] args) {  
				  try {  

				        // 轮询间隔 5 秒  
				        long interval = TimeUnit.SECONDS.toMillis(5);  
				        //   
				        FileAlterationObserver observer = new FileAlterationObserver(  
				                                              "D://DB",   
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