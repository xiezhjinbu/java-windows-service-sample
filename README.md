JavaWindowsServiceSample
========================

A simple example of a Java + Spring Framework Program that can be run as a background process or a Windows Service.

This sample application makes use of [Java Service Wrapper](http://wrapper.tanukisoftware.com). It specifically uses the [Integration Method 3](http://wrapper.tanukisoftware.com/doc/english/integrate-listener.html) of Java Service Wrapper in which the Main class implements the WrapperListener interface and makes use of the WrapperManager to start the application.

There are lots of configuration to make this work as a Windows service and run when you boot your PC. I will be posting a tutorial about this on my blog [No-nonsense](http://benjsicam.me) so stay tuned.
