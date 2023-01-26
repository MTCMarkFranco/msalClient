using System.Text.Json;
using Microsoft.Identity.Client;
using callRecords.Models;
using Microsoft.Extensions.Configuration;

// Initialize
AuthenticationResult? result = null;
Dictionary<string,int> PlanUsageTotals = new Dictionary<string,int>(16);

// Initialize Configuration object
var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>();

var configurationRoot = builder.Build();

var msalConfig = configurationRoot.GetSection("MSAL").Get<MSALConfig>();

string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
var app = ConfidentialClientApplicationBuilder.Create(msalConfig.ClientID)
                                          .WithClientSecret(msalConfig.ClientSecret)
                                          .WithAuthority(new Uri(string.Format("https://login.microsoftonline.com/{0}", msalConfig.TenantID)))
                                          .Build();

try
{
    result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
}
catch (MsalUiRequiredException)
{
    
}

if (result != null)
{
    using (HttpClient httpClient = new HttpClient())
    {

        DateTime fromDateTime = new DateTime(DateTime.Now.Year,DateTime.Now.Month ,1); // beginging of this month
        DateTime toDateTime = DateTime.Now + TimeSpan.FromDays(1); // inclusive till the end of day today

        string url = string.Format("https://graph.microsoft.com/v1.0/communications/callRecords/getPstnCalls(fromDateTime={0},toDateTime={1})",fromDateTime.ToString("yyyy-MM-dd"),toDateTime.ToString("yyyy-MM-dd"));
        
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
        var res = httpClient.GetAsync(url).Result;

        try
        {
            var jsonString = res.Content.ReadAsStringAsync().Result.ToString();
            var callLogRows = JsonSerializer.Deserialize<PstnLogCallRows>(jsonString);

            if (callLogRows != null && callLogRows.pstnLogCallRow != null )
            {
                //int row = 1;
                PlanDetails? planDetails = null;
                foreach (var call in callLogRows.pstnLogCallRow)
                {
                    
                    // Console.ForegroundColor = (ConsoleColor)(row++ % 14);
                    // Console.WriteLine(string.Format("Called: {0:#-###-###-####} : for '{1}' minutes",Convert.ToInt64(call.calleeNumber),call.duration / 60));
                    // Console.WriteLine(string.Format("charge: {0}",call.charge));
                    // Console.WriteLine(string.Format("connectionCharge: {0}",call.connectionCharge));
                    // Console.WriteLine(string.Format("currency: {0}",call.currency));
                    // Console.WriteLine(string.Format("callType: {0}",call.callType));
                    // Console.WriteLine(string.Format("startDateTime: {0}",call.startDateTime));
                    // Console.WriteLine(string.Format("endDateTime: {0}",call.endDateTime));
                    // Console.WriteLine(string.Format("userPrincipalName: {0}",call.userPrincipalName));
                    // Console.WriteLine(string.Format("userDisplayName: {0}",call.userDisplayName));
                    // Console.WriteLine(string.Format("userId: {0}",call.userId));
                    // Console.WriteLine(string.Format("callId: {0}",call.callId));
                    // Console.WriteLine(string.Format("id: {0}",call.id));
                    // Console.WriteLine(string.Format("usageCountryCode: {0}",call.usageCountryCode));
                    // Console.WriteLine(string.Format("tenantCountryCode: {0}",call.tenantCountryCode));
                    // Console.WriteLine(string.Format("licenseCapability: {0}",call.licenseCapability));
                    // Console.WriteLine(string.Format("destinationContext: {0}",call.destinationContext));
                
                    // Get the Current PLan Details and Limits
                    planDetails =  GetCurrentPlanTypeandLimits(call);
                   
                    // Add the current call to the total for the plan type
                    int totalcurrentCallTypePlan;
                    if (PlanUsageTotals.TryGetValue(planDetails.planTypeFriendlyName , out totalcurrentCallTypePlan))
                        PlanUsageTotals[planDetails.planTypeFriendlyName] = (int)(totalcurrentCallTypePlan + (call.duration / 60));
                    else
                        PlanUsageTotals.Add(planDetails.planTypeFriendlyName, (int)(call.duration / 60));
                
                }

                
                // loop through each keyvaluepari in PlanUsageTotals
                // and determine if we are over the plan limit
                int row = 0;
                foreach (KeyValuePair<string, int> kvp in PlanUsageTotals)
                {
                    Console.ForegroundColor = (ConsoleColor)(row++ % 14);

                    if (kvp.Value > planDetails.planLimit)
                        Console.WriteLine(string.Format("You are over the {0} limit of {1} minutes, with {2} minutes consumed for the period(month).", kvp.Key, planDetails.planLimit,kvp.Value));
                    else
                        Console.WriteLine(string.Format("You are under the {0} limit of {1} minutes, with {2} minutes consumed for the period(month).", kvp.Key, planDetails.planLimit,kvp.Value));
                }
                                

            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}

PlanDetails GetCurrentPlanTypeandLimits(PstnLogCallRow call)
{
    
   bool InSelectCountriesFlag = false;
   bool callIsDomestic = false;
   
        
    // check if call.usageCountryCode has one of these values: "UK","US","PR","CA"
    // if so, set InSelectCountriesFlag to true
    if (call.usageCountryCode == "UK" || call.usageCountryCode == "US" || call.usageCountryCode == "PR" || call.usageCountryCode == "CA")
        InSelectCountriesFlag = true;
    
   
    // Check if they are using the domestic or international Minutes
    if (call.destinationContext == "Domestic")
	    callIsDomestic = true;
       
    // Get the limit for the plan buckets (DOMESTIC_US_PR_CA_UK_OutBound_Limit,DOMESTIC_Other_OutBound_Limit,INTERNATIONAL_ALL_OutBound_Limit )
     KeyValuePair<string,int> currentCallTypePlanLimit = getCurrentCallTypePlanLimitAndType(call.licenseCapability, callIsDomestic, InSelectCountriesFlag);

     return new PlanDetails { planTypeFriendlyName = currentCallTypePlanLimit.Key, planLimit = currentCallTypePlanLimit.Value};
}

KeyValuePair<string,int> getCurrentCallTypePlanLimitAndType(string licenseCapability, bool callIsDomestic, bool inSelectCountriesFlag)
{
            // return plan limit and type for the licenseCapability. plan limits are as follows:
            // if licenseCapability == "MCOPSTN2" then
            //   return PL_MCOPSTN2

            switch (licenseCapability)
            {
                case "MCOPSTN2":
                {
                    if (callIsDomestic)
                    {
                        if (inSelectCountriesFlag)
                            return new KeyValuePair<string, int>("MCOPSTN2_DOMESTIC_US_PR_CA_UK_OutBound_Type", (int)PL_MCOPSTN2.DOMESTIC_US_PR_CA_UK_OutBound_Limit);
                        else
                            return new KeyValuePair<string, int>("MCOPSTN2_DOMESTIC_Other_OutBound_Type", (int)PL_MCOPSTN2.DOMESTIC_Other_OutBound_Limit);
                    }
                    else
                       {
                        return new KeyValuePair<string, int>("MCOPSTN2_INTERNATIONAL_ALL_OutBound_Type", (int)PL_MCOPSTN2.INTERNATIONAL_ALL_OutBound_Limit);
                       }
                 
                }
                case "MCOPSTN1":
                {
                    if (callIsDomestic)
                    {
                        if (inSelectCountriesFlag)
                            return new KeyValuePair<string, int>("MCOPSTN1_DOMESTIC_US_PR_CA_UK_OutBound_Type", (int)PL_MCOPSTN1.DOMESTIC_US_PR_CA_UK_OutBound_Limit);
                        else
                            return new KeyValuePair<string, int>("MCOPSTN1_DOMESTIC_Other_OutBound_Type", (int)PL_MCOPSTN1.DOMESTIC_Other_OutBound_Limit);
                    }
                    else
                    {
                        return new KeyValuePair<string, int>("MCOPSTN1_INTERNATIONAL_ALL_OutBound_Type", (int)PL_MCOPSTN1.INTERNATIONAL_ALL_OutBound_Limit);
                    }
                }
                default:
                    return new KeyValuePair<string, int>("", 0);
            }

}
// Plan Limits
public enum PL_MCOPSTN2
{
    DOMESTIC_US_PR_CA_UK_OutBound_Limit = 3000,
    DOMESTIC_Other_OutBound_Limit = 1200,
    INTERNATIONAL_ALL_OutBound_Limit = 600
}

public enum PL_MCOPSTN1
{
    DOMESTIC_US_PR_CA_UK_OutBound_Limit = 3000,
    DOMESTIC_Other_OutBound_Limit = 1200,
    INTERNATIONAL_ALL_OutBound_Limit = 0
}


public class PlanDetails
{
    public string planTypeFriendlyName { get; set; }
    public int planLimit { get; set; }
}

public  class MSALConfig
{
    public string ClientID { get; set; }
    public string ClientSecret { get; set; }
    public string TenantID {get; set; }
}