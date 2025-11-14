using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    private static readonly HttpClient client = new HttpClient();
    
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogInformation($"FunctionHandler received: {input}");

        dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());


        context.Logger.LogInformation($"Body: {json.body}");
        dynamic body = JsonConvert.DeserializeObject<dynamic>(json.body.ToString());

    
        string issueUrl = body.issue.html_url;
        context.Logger.LogInformation($"Issue URL: {issueUrl}");

        string payload = JsonConvert.SerializeObject(new
        {
            text = $"Issue Created: {issueUrl}"
        });

        // 5) Read Slack webhook from env var (NO hard-coding)
        string slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");

        var request = new HttpRequestMessage(HttpMethod.Post, slackUrl)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        var response = client.Send(request);
        string responseBody = response.Content.ReadAsStringAsync().Result;

        return responseBody;
    }
}
