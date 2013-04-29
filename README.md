Java Windows Service Sample
========================

A simple example of a Java + Spring Framework Program that can be run as a background process or a Windows Service.

This sample application makes use of [Java Service Wrapper](http://wrapper.tanukisoftware.com). It specifically uses the [Integration Method 3](http://wrapper.tanukisoftware.com/doc/english/integrate-listener.html) of Java Service Wrapper in which the Main class implements the WrapperListener interface and makes use of the WrapperManager to start the application. The application then has Spring Framework's scheduling mechanism (@Scheduled) on the worker class to execute a task on a fixed rate. 

There are lots of configuration to make this work as a Windows service and run when you boot your PC. I have posted a [tutorial](http://benjsicam.me/blog/running-a-java-application-as-a-windows-service-part-1-tutorial/) about it on my blog [No-nonsense](http://benjsicam.me). Hope this helps someone.

Note: Add Java Service Wrapper's wrapper.jar on your classpath. Download it from their website. It is not available as a Maven dependency.