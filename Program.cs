using System.Text.Json;
using Microsoft.Identity.Client;
using callRecords.Models;

// Initialize
AuthenticationResult? result = null;

string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
var app = ConfidentialClientApplicationBuilder.Create("f976c105-c8e4-4635-8ddf-e8acab8c7345")
                                          .WithClientSecret("***REMOVED***")
                                          .WithAuthority(new Uri("https://login.microsoftonline.com/b8ba91e8-4402-47de-9d97-12a0faaa0115"))
                                          .Build();

try
{
    result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
}
catch (MsalUiRequiredException)
{
    
}

// DEBUG
//Console.WriteLine(result.AccessToken);
if (result != null)
{
    using (HttpClient httpClient = new HttpClient())
    {
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
        var res = httpClient.GetAsync("https://graph.microsoft.com/v1.0/communications/callRecords/getPstnCalls(fromDateTime=2022-12-30,toDateTime=2023-01-30)").Result;

        //Console.WriteLine(res.Content.ReadAsStringAsync().Result.ToString());

        try
        {
            var jsonString = res.Content.ReadAsStringAsync().Result.ToString();
            var callLogRows = JsonSerializer.Deserialize<PstnLogCallRows>(jsonString);

            if (callLogRows != null && callLogRows.pstnLogCallRow != null )
            {
                foreach (var calls in callLogRows.pstnLogCallRow)
                {
                    Console.ForegroundColor = (ConsoleColor)(new Random()).Next(0,14);
                    Console.WriteLine(string.Format("Called: {0:#-###-###-####} : for '{1}' minutes",Convert.ToInt64(calls.calleeNumber),calls.duration / 60));
                    Console.WriteLine(string.Format("charge: {0}",calls.charge));
                    Console.WriteLine(string.Format("charge: {0}",calls.connectionCharge));
                    Console.WriteLine(string.Format("currency: {0}",calls.currency));
                    Console.WriteLine(string.Format("callType: {0}",calls.callType));
                    Console.WriteLine(string.Format("startDateTime: {0}",calls.startDateTime));
                    Console.WriteLine(string.Format("endDateTime: {0}",calls.endDateTime));
                    Console.WriteLine(string.Format("userPrincipalName: {0}",calls.userPrincipalName));
                    Console.WriteLine(string.Format("userDisplayName: {0}",calls.userDisplayName));
                    Console.WriteLine(string.Format("userId: {0}",calls.userId));
                    Console.WriteLine(string.Format("callId: {0}",calls.callId));
                    Console.WriteLine(string.Format("id: {0}",calls.id));
                    Console.WriteLine(string.Format("usageCountryCode: {0}",calls.usageCountryCode));
                    Console.WriteLine(string.Format("tenantCountryCode: {0}",calls.tenantCountryCode));
                    
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}
