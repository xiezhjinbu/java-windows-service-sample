package service;


import org.apache.log4j.Logger;

/**
 * Created by Administrator on 2017/9/18.
 */
public class FileLogger {
    private static Logger logger = Logger.getLogger(FileLogger.class);

    public void logInfo2file()
    {
        for (int i = 0; i < 100; i++)
        {
            logger.info("我的测试：my test"+i);
        }
    }
}
