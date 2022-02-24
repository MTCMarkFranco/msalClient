using Microsoft.Identity.Client;

// Initialize
AuthenticationResult result;

// Get Token using MSAL with the proper Audience and Scope and using the Public Client Application in MSAL with Interactive Login (accommodates MFA and Conditional Access Policies)
string[] scopes = new string[] { "api://<CLIENT_ID_OR_APP_FQDN>/Weather.Read" };
var app = PublicClientApplicationBuilder.Create("<CLIENT_ID_OF_NATIVE_APP_REG>")
        .WithTenantId("<TENANT_ID_OF_APP_REG>")
        .WithDefaultRedirectUri()
        .Build();

var accounts = await app.GetAccountsAsync();

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

// DEBUG
Console.WriteLine(result.AccessToken);

// Call our Protected Web API
using (HttpClient httpClient = new HttpClient())
{
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);

    var res = httpClient.GetAsync("https://localhost:7104/WeatherForecast").Result;

}
