﻿using System.Text.Json;
using Microsoft.Identity.Client;
using callRecords.Models;

// Initialize
AuthenticationResult? result = null;

string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
var app = ConfidentialClientApplicationBuilder.Create("f976c105-c8e4-4635-8ddf-e8acab8c7345")
                                          .WithClientSecret("Fzn8Q~JjY2iv33ZFAGikNwpfE0ht_uoT-flc2aoS")
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
Console.WriteLine(result.AccessToken);

using (HttpClient httpClient = new HttpClient())
{
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);

    var res = httpClient.GetAsync("https://graph.microsoft.com/v1.0/communications/callRecords/getPstnCalls(fromDateTime=2023-01-09,toDateTime=2023-01-11)").Result;

    Console.WriteLine(res.Content.ReadAsStringAsync().Result.ToString());

    var callLogRows = JsonSerializer.Deserialize<pstnCallLogRow>(res.Content.ReadAsStringAsync().Result.ToString());

}
