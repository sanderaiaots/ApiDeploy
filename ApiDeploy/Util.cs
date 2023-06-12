using System;
using System.Text;

namespace ApiDeploy; 

public static class Util {

	public static bool IsTestCaseOk (TestCase testCase, string apiBaseUrl)
	{
        testCase.Url = testCase.Url.Replace("{{ColtApiUrl}}", apiBaseUrl);
        try
        {
            using HttpClient http = new HttpClient();
            Console.WriteLine("Making call to: " + testCase.Url);
            HttpResponseMessage resp;
            if (testCase.Method == "GET")
            {
                resp = http.GetAsync(new Uri(testCase.Url)).Result;
            }
            else
            {
                resp = http.PostAsync(new Uri(testCase.Url), new StringContent(testCase.Payload, Encoding.UTF8, $"application/{testCase.ContentType}" )).Result;
            }
            if (resp.IsSuccessStatusCode)
            {
                var data = resp.Content.ReadAsByteArrayAsync().Result;
                var respContent = Encoding.UTF8.GetString(data);
                if (testCase.Type == "DeliveryTimes")
                {
                    var isOk = respContent.Contains("DeliveryFrom");
                    Console.WriteLine($"{(isOk ? "OK" : "FAIL")} test {testCase.Type} from {testCase.Url}");
                    return isOk;
                }
                else if (testCase.Type == "Availability")
                {
                    var isOk = respContent.Contains("DcQtyAvailable");
                    Console.WriteLine($"{(isOk ? "OK" : "FAIL")} test {testCase.Type} from {testCase.Url}");
                    return isOk;
                }
                else
                {
                    Console.WriteLine($"Unknown test type: {testCase.Type}");
                    return false;
                };
            }
            else
            {
                var data = resp.Content.ReadAsByteArrayAsync().Result;
                Console.WriteLine($"FAIL {resp.StatusCode} with {data.Length} bytes from: " + testCase.Url);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Call failed to: " + testCase.Url + " message=" + e.Message);

        }

        return false;
    }

}