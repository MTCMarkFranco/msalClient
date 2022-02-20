using Microsoft.Identity.Client;

string[] scopes = new string[] { "api://***REMOVED***/Weather.Read" };
var app = PublicClientApplicationBuilder.Create("***REMOVED***")
        .WithTenantId("***REMOVED***")
        .WithDefaultRedirectUri()
        .Build();
var accounts = await app.GetAccountsAsync();
AuthenticationResult result;
try
{
    result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                .ExecuteAsync();
}
catch (MsalUiRequiredException)
{
    result = await app.AcquireTokenInteractive(scopes)
                .ExecuteAsync();
}

Console.WriteLine(result.AccessToken);

using (HttpClient httpClient = new HttpClient())
{
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);

    var res = httpClient.GetAsync("https://localhost:7104/WeatherForecast").Result;

}
