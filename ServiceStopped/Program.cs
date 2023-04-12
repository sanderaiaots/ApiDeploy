// See https://aka.ms/new-console-template for more information

using System;
using System.ServiceProcess;
using System.Threading;
using ServiceStopped;

if (args.Length == 0) {
	Console.WriteLine("Please provide service name as first argument");
	return;
}
string serviceName = args[0];
Console.WriteLine($"Will check if service: '{args[0]}' is stopped");
int i = 0;
while (ServiceStatus.GetServiceStatus(serviceName) != ServiceControllerStatus.Stopped) {
	if (i > 0) {
		Console.Write(".");
	}
	Thread.Sleep(1000);
	i++;
}
Console.WriteLine();
Console.WriteLine($"Service {serviceName} is stopped");
