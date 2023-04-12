namespace ApiDeploy; 

public static class Util {
	public static bool IsHttpCallOok(string url) {
		try {
			using HttpClient http = new HttpClient();
			Console.WriteLine("Making call to: " + url);
			using HttpResponseMessage resp = http.GetAsync(new Uri(url)).Result;
			if (resp.IsSuccessStatusCode) {
				var data = resp.Content.ReadAsByteArrayAsync().Result;
				Console.WriteLine($"OK with {data.Length} bytes from: " + url);
				return true;
			}
			else {
				var data = resp.Content.ReadAsByteArrayAsync().Result;
				Console.WriteLine($"FAIL {resp.StatusCode} with {data.Length} bytes from: " + url);
			}
		}
		catch (Exception e) {
			Console.WriteLine("Call failed to: " + url + " message=" + e.Message);
			
		}

		return false;
	}
}