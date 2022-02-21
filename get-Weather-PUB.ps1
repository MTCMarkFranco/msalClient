# Initialization
$result =$null

# Load MSAL Powershell Module
$installed = Get-Module | Select Name | ? {$_.Name -eq "MSAL.PS"}
if ($installed -eq $null) { Install-Module -Name "MSAL.PS" -Force -WarningAction SilentlyContinue } else {Write-Host -ForegroundColor Green "[INFORMATION]: MSAL.PS Already Installed!"}

#  Configuration Variables *************************
$TenantId = "d7037d74-e72d-44e9-8f94-c0b2b4845174" 
$ClientId = "40ffedfe-faef-46a2-947c-5d64d2e7a50f"
#  Configuration Variables *************************

try {

    # Get Token using MSAL with the proper Audience and Scope and using the Public Client Application in MSAL with Interactive Login (most secure and accommodates MFA)
    $token = Get-MsalToken -ClientId $ClientId -Scope 'api://9e5c0e54-2126-4242-9ccd-d07405d5c170/Weather.Read' -TenantId $TenantId
    $Auth =  "Bearer " + $token.AccessToken

    # Call our Protected Web API

    $result = curl -Method Get 'https://localhost:7104/WeatherForecast' -H @{ "accept" = "text/plain"; "Content-Type" = "application/json; charset=utf-8"; "Authorization" = $Auth}

    write-host -ForegroundColor Green "[INFORMATION]: Api Result:" 
    write-host -ForegroundColor Yellow -BackgroundColor DarkCyan $result.Content

}

catch [System.SystemException] {
    
    write-host -ForegroundColor Red "[ERROR]: oopsie, something went wrong:" 
    Write-Host $_ -ForegroundColor Red

}



