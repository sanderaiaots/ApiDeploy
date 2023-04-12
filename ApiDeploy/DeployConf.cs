namespace ApiDeploy; 

public class DeployConf {
	public string IISFolder;
	public string DeployInputFolder;
	public string IgnoreFiles;
	public string[] BootUrls;
	public NodeConf[] Nodes;
	/// <summary>
	/// If true alter web.config on IIS instead of copy over new web config from node configuration.
	/// New proxy config will be determine 
	/// </summary>
	public bool IsAlterIisWebConfig;
	/// <summary>
	/// what is active node url where we redirect traffic
	/// </summary>
	public string? ActiveNodeUrl;
}