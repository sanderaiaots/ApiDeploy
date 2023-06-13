using System;
using Newtonsoft.Json;

namespace ApiDeploy;

public class TestCollectionParser {

    // helps us find the text that test is searching in reponse to do valdiation
   static string validationTextMarker = "to.include(\"";

        
    public static TestCase[] ParseCollection (string collectionJson) {
        dynamic testCollection = JsonConvert.DeserializeObject<dynamic>(collectionJson)!;

        List<TestCase> testCases = new List<TestCase>();

        foreach (dynamic folder in testCollection.item) {
            if (folder.item == null) {
                continue;
            }

            string textToValidate = null;
            dynamic events = folder["event"];
            if (events != null) {
                foreach (dynamic evnt in events) {
                    if (evnt.listen == "test") {
                        if (evnt.script.exec == null) {
                            continue;
                        }
                        string testScript = string.Concat(evnt.script.exec);
                        if (string.IsNullOrEmpty(testScript)) {
                            continue;
                        }
                        int textToValidatePos = testScript.IndexOf(validationTextMarker);
                        if (textToValidatePos > 0) {
                            textToValidate = testScript.Substring(textToValidatePos + validationTextMarker.Length);
                            textToValidate = textToValidate.Substring(0, textToValidate.IndexOf("\""));
                        }
                    }
                }
            }

            foreach (var item in folder.item) {

                TestCase testCase =  new TestCase {
                    Method = item.request.method,
                    Url = item.request.url.raw,
                    Payload = item.request.body?.raw,
                    ContentType = item.request.body?.options?.raw?.language,
                    Name = item.name,
                    Type = folder.name,
                    ValidationText = textToValidate
                };
                if (folder["event"] == null) {
                    continue;
                }
                
                testCases.Add(testCase);
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
    public string ValidationText { get; set; }
}
