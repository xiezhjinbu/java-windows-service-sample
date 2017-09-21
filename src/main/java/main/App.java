package main;

import java.io.File;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * Hello world!
 *
 */
public class App 
{
	private static final Logger logger = LoggerFactory.getLogger(App.class);  
	
    public static void main( String[] args )
    {
    	File file=new File("D://DB/FILE_20170518_105609_171_undefined_bom.xml");
    	
    	ChangeXmlFile.readFile(file);
    	
    	System.out.println("文件修改完成");
    	
    	logger.info("文件修改完成!");  
    }
}
