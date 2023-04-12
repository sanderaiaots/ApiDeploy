namespace ApiDeploy; 

public class NodeConf {
	public string ServiceName;
	public string RunFolder;
	/// <summary>
	/// Web config location that is used for IIS proxy
	/// </summary>
	public string WebConfig;
	public bool IsActive;
	/// <summary>
	/// Url where this node is running
	/// </summary>
	public string HostedUrl;
}

