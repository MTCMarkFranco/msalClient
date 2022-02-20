using Microsoft.Identity.Client;

string[] scopes = new string[] { "api://9e5c0e54-2126-4242-9ccd-d07405d5c170/Weather.Read" };
var app = PublicClientApplicationBuilder.Create("40ffedfe-faef-46a2-947c-5d64d2e7a50f")
        .WithTenantId("d7037d74-e72d-44e9-8f94-c0b2b4845174")
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
