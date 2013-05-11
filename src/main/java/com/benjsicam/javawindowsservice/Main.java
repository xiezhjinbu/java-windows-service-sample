package com.benjsicam.javawindowsservice;

import org.tanukisoftware.wrapper.WrapperListener;
import org.tanukisoftware.wrapper.WrapperManager;

public class Main implements WrapperListener {
	WindowsServiceApp javaWindowsServiceSample;

	public static void main(String[] args) {
		WrapperManager.start(new Main(), args);
	}

	public void controlEvent(int event) {
		if ((event == WrapperManager.WRAPPER_CTRL_LOGOFF_EVENT )
				&& ( WrapperManager.isLaunchedAsService() || WrapperManager.isIgnoreUserLogoffs())) {
			//Ignore
		}
		else {
			WrapperManager.stop( 0 );
		}
	}

	public Integer start(String[] arg0) {
		javaWindowsServiceSample = new WindowsServiceApp();
		javaWindowsServiceSample.start();
		
		return null;
	}

	public int stop(int arg0) {
		return 0;
	}
}
