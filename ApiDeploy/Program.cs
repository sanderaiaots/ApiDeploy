// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using ApiDeploy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


string configLoction = args.Length > 0 ? args[0] : "conf.json";
DeployConf deploy = JsonConvert.DeserializeObject<DeployConf>(File.ReadAllText(configLoction));
NodeConf[] conf = deploy.Nodes;
CopyTools tools = new CopyTools(deploy.IgnoreFiles);

Console.WriteLine("Will handle Web.Api deploy sequence");
NodeConf? inactive = null;
NodeConf? active = null;
string iisWebConficLocation = deploy.IISFolder + Path.DirectorySeparatorChar + "web.config";
/*
 //TODO: we currenly use IIS to detect active node
foreach (var dc in conf) {
//Step one: detmine actve node
	using ServiceController sc = new ServiceController(dc.ServiceName);
	if (sc.Status == ServiceControllerStatus.Running) {
		dc.IsActive = true;
		active = dc;
	}
	else {
		inactive = dc;
	}
}
*/

if (inactive == null) {
	Console.WriteLine(
		"NO inactive nodes found. Try to determinge on by running IIS config. Active=[" + string.Join(",", conf.Where(w => w.IsActive).Select(s => s.ServiceName)) + "]");
	string webConfigContent = File.ReadAllText(iisWebConficLocation);
	foreach (var dc in conf) {
		string config = dc.RunFolder + Path.DirectorySeparatorChar + "appsettings.json";
		Console.WriteLine("read data from: " + config);
		JObject x = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(config));
		var url = x["Kestrel"]["Endpoints"]["Http"]["Url"].ToString();
		dc.HostedUrl = url;

		Console.WriteLine("Object type: " + x.GetType() + "... " + url);
		if (webConfigContent.IndexOf(url, StringComparison.OrdinalIgnoreCase) > 0) {
			dc.IsActive = true;
			active = dc;
			deploy.ActiveNodeUrl = url;
		}
		else {
			dc.IsActive = false;
			inactive = dc;
			Console.WriteLine("Inactive node found as: " + dc.ServiceName + " on URL=" + url);
		}
	}
}

if (inactive != null) {
	Console.WriteLine("Will deploy to " + inactive.ServiceName + " path=" + inactive.RunFolder);
	using ServiceController sc = new ServiceController(inactive.ServiceName);
	if (sc.Status == ServiceControllerStatus.Running) {
		Console.WriteLine($"Inactive node {inactive.ServiceName} is running. Stoping down before deploy activity.");
		sc.Stop();
		sc.WaitForStatus(ServiceControllerStatus.Stopped);
	}
	if (!string.IsNullOrEmpty(deploy.BackupFolder) && !string.IsNullOrEmpty(deploy.BackupName)) {
		CopyTools.Backup(deploy.BackupFolder, inactive.RunFolder, deploy.BackupName);
	}
	Console.WriteLine("Will delete files in: " + inactive.RunFolder);
	tools.CleanFolder(inactive.RunFolder);
	Console.WriteLine("Copy new code to=" + inactive.RunFolder + " from=" + deploy.DeployInputFolder);
	tools.Copy(deploy.DeployInputFolder, inactive.RunFolder);
	Console.WriteLine($"Copied files={tools.CopyFileCount} size={tools.CopyFileSize / 1024.0 / 1024:0.0}MiB");
	Console.WriteLine("Start inactive service " + inactive.ServiceName);

	ServiceHelper.ChangeStartMode(sc, ServiceStartMode.Automatic);
	sc.Start();

	if (!deploy.BootUrls.All(url => Util.IsHttpCallOok(inactive.HostedUrl + url))) {
		Console.WriteLine("Boot URL.s returned error. Stop dpeloy process");
		return;
	}

	if (deploy.IsAlterIisWebConfig && deploy.ActiveNodeUrl != null) {
		string webConfigContent = File.ReadAllText(iisWebConficLocation);
		string newConfig = webConfigContent.Replace(deploy.ActiveNodeUrl, inactive.HostedUrl);
		Console.WriteLine("Replace active node in IIS config: " + iisWebConficLocation);
		File.WriteAllText(iisWebConficLocation, newConfig, Encoding.UTF8);
	}
	else {
		Console.WriteLine("Copy new IIS config to activate node");
		File.Copy(inactive.WebConfig, deploy.IISFolder + Path.DirectorySeparatorChar + "web.config", true);
	}

	//TODO: just in case sleep a bit in live
	if (active != null) {
		Console.WriteLine("Will stop active node " + active.ServiceName + " path=" + active.RunFolder);
		using ServiceController scA = new ServiceController(active.ServiceName);
		ServiceHelper.ChangeStartMode(scA, ServiceStartMode.Manual);
		if (scA.Status == ServiceControllerStatus.Running) {
			scA.Stop();
		}
	}
}