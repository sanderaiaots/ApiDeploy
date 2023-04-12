using System.ServiceProcess;

namespace ServiceStopped; 

public class ServiceStatus {
	public static ServiceControllerStatus GetServiceStatus(string serviceName) {
		using ServiceController sc = new ServiceController(serviceName);
		return sc.Status;
	}
}