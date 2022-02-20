#Install-Module MSAL.PS -SkipPublisherCheck -Force
#Install-Module -name MSAL.PS -Force -AcceptLicense


$TenantId = "***REMOVED***"  # aka Directory ID. This value is Microsoft tenant ID
$ClientId = "***REMOVED***"  # aka Application ID

$token = Get-MsalToken -ClientId $ClientId -Scope 'api://***REMOVED***/Weather.Read' -TenantId $TenantId -RedirectUri "http://localhost"

$Auth =  "Bearer " + $token.AccessToken


$result = curl -Method Get 'https://localhost:7104/WeatherForecast' -H @{ "accept" = "text/plain"; "Authorization" = $Auth}


$result