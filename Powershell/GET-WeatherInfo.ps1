# Initialization
$result =$null

# Load MSAL Powershell Module
$installed = Get-Module | Select Name | ? {$_.Name -eq "MSAL.PS"}
if ($installed -eq $null) { Install-Module -Name "MSAL.PS" -Force -WarningAction SilentlyContinue } else {Write-Host -ForegroundColor Green "[INFORMATION]: MSAL.PS Already Installed!"}

#  Configuration Variables *************************
$TenantId = "<TENANT_ID_OF_APP_REG>" 
$ClientId = "<CLIENT_ID_OF_NATIVE_APP_REG>"
#  Configuration Variables *************************

try {

    # Get Token using MSAL with the proper Audience and Scope and using the Public Client Application in MSAL with Interactive Login (accommodates MFA and Conditional Access Policies)
    $token = Get-MsalToken -ClientId $ClientId -Scope 'api://<CLIENT_ID_OR_APP_FQDN>/Weather.Read' -TenantId $TenantId
    $Auth =  "Bearer " + $token.AccessToken

    # Call our Protected Web API

    $result = curl -Method Get 'https://localhost:7104/WeatherForecast' -H @{ "accept" = "text/plain"; "Content-Type" = "application/json; charset=utf-8"; "Authorization" = $Auth}

    write-host -ForegroundColor Green "[INFORMATION]: Api Result:" 
    write-host -ForegroundColor Yellow -BackgroundColor DarkCyan $result.Content

}

catch [System.SystemException] {
    
    write-host -ForegroundColor Red "[ERROR]: oopsie, something went wrong!?!!?" 
    Write-Host $_ -ForegroundColor Red

}



