#Install-Module MSAL.PS -SkipPublisherCheck -Force
#Install-Module -name MSAL.PS -Force -AcceptLicense


$TenantId = "d7037d74-e72d-44e9-8f94-c0b2b4845174"  # aka Directory ID. This value is Microsoft tenant ID
$ClientId = "40ffedfe-faef-46a2-947c-5d64d2e7a50f"  # aka Application ID

$token = Get-MsalToken -ClientId $ClientId -Scope 'api://9e5c0e54-2126-4242-9ccd-d07405d5c170/Weather.Read' -TenantId $TenantId -RedirectUri "http://localhost"

$Auth =  "Bearer " + $token.AccessToken


$result = curl -Method Get 'https://localhost:7104/WeatherForecast' -H @{ "accept" = "text/plain"; "Authorization" = $Auth}


$result