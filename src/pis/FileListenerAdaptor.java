package pis;

import java.io.File;

import org.apache.commons.io.monitor.FileAlterationListenerAdaptor;
import org.apache.commons.io.monitor.FileAlterationObserver;
import org.apache.log4j.Logger;

public class FileListenerAdaptor  extends FileAlterationListenerAdaptor{

private static final Logger logger = Logger.getLogger(FileListenerAdaptor.class);
	
	@Override
	public void onFileChange(File file) {
		// TODO 自动生成的方法存根
		super.onFileChange(file);
		//System.out.println("文件修改成功！");
		logger.info("文件改变事件");  
	}

	@Override
	public void onFileCreate(File file) {
		// TODO 自动生成的方法存根
		super.onFileCreate(file);
		String prefix=file.getName().substring(file.getName().lastIndexOf("."));
		//logger.info(prefix+"文件改变事件");  
		if (".xml".equals(prefix.toLowerCase()))
		{
			ChangeXmlFile.readFile(file);
		} 
		else 
		{
			logger.info(file.getName()+"  :报文异常!,请重新导出!!");
		}
		//System.out.println("文件修改完成");
		//logger.info("文件创建成功");
		logger.info("文件名称：" + file.getName());  
		file.delete();
	}

	@Override
	public void onFileDelete(File file) {
		// TODO 自动生成的方法存根
		super.onFileDelete(file);
		//System.out.println("文件删除成功！");
		logger.info("文件删除事件:" + file.getName());  
	}

	@Override
	public void onStart(FileAlterationObserver observer) {
		// TODO 自动生成的方法存根
		super.onStart(observer);
		//System.out.println(observer.getDirectory()+"监听开始！");
		//logger.info(observer.getDirectory()+"监听开始！");  
	}

	@Override
	public void onStop(FileAlterationObserver observer) {
		// TODO 自动生成的方法存根
		super.onStop(observer);
		//System.out.println(observer.getDirectory()+"监听结束！");
		logger.info(observer.getDirectory()+"监听结束！");  
	}

	@Override
	public void onDirectoryChange(File directory) {
		// TODO 自动生成的方法存根
		super.onDirectoryChange(directory);
		//System.out.println("文件夹修改完成");
	}

	@Override
	public void onDirectoryCreate(File directory) {
		// TODO 自动生成的方法存根
		super.onDirectoryCreate(directory);
		//System.out.println("文件夹创建完成");
	}

	@Override
	public void onDirectoryDelete(File directory) {
		// TODO 自动生成的方法存根
		super.onDirectoryDelete(directory);
		//System.out.println("文件夹删除完成");
	}
	
	
}
