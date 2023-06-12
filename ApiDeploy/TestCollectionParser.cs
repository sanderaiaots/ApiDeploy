using System;
using Newtonsoft.Json;

namespace ApiDeploy;

public class TestCollectionParser {
        
    public static TestCase[] ParseCollection (string collectionJson) {
        dynamic testCollection = JsonConvert.DeserializeObject<dynamic>(collectionJson)!;

        List<TestCase> testCases = new List<TestCase>();

        foreach (dynamic folder in testCollection.item) {
            if (folder.item == null) {
                continue;
            }
            foreach (var item in folder.item) {

                testCases.Add(new TestCase {
                    Method = item.request.method,
                    Url = item.request.url.raw,
                    Payload = item.request.body?.raw,
                    ContentType = item.request.body?.options?.raw?.language,
                    Name = item.name,
                    Type = folder.name
                });
            }

        }
        return testCases.ToArray();
    }
}

public class TestCase {
    public string Url { get; set; }
    public string Method { get; set; }
    public string Payload { get; set; }
    public string ContentType { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
}
