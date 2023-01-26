using System.Text.Json;
using Microsoft.Identity.Client;
using callRecords.Models;
using Microsoft.Extensions.Configuration;

// Initialize
int row = 1;
PlanDetails? planDetails = null;
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
    // Do Nothing
}

if (result != null)
{
    using (HttpClient httpClient = new HttpClient())
    {

        // Look for records from the start of the period(first of the month) to the end of the current day (11:59:59 PM)
        DateTime fromDateTime = new DateTime(DateTime.Now.Year,DateTime.Now.Month ,1); // Beginging of this month
        DateTime toDateTime = new DateTime(DateTime.Now.Year,DateTime.Now.Month ,DateTime.Now.Day,11,59,59); // End of Today

        // MS Graph Uri for the "getPstnCalls" API
        string url = string.Format("https://graph.microsoft.com/v1.0/communications/callRecords/getPstnCalls(fromDateTime={0},toDateTime={1})",fromDateTime.ToString("yyyy-MM-ddThh:mm:ss.sssZ"),toDateTime.ToString("yyyy-MM-ddThh:mm:ss.sssZ"));
        
        // Add Authorization Header
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
        
        // Get the Call Records
        var res = httpClient.GetAsync(url).Result;

        try
        {
            var jsonString = res.Content.ReadAsStringAsync().Result.ToString();
            var callLogRows = JsonSerializer.Deserialize<PstnLogCallRows>(jsonString);

            if (callLogRows != null && callLogRows.pstnLogCallRow != null )
            {
                // Step 1. Fill the PLanUsageTotals Dictionary with the plan details
                foreach (var call in callLogRows.pstnLogCallRow)
                {
                    #region Get Plan Details for Debugging 
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
                    #endregion
                
                    // Get the Current PLan Details and Limits
                    planDetails =  GetCurrentPlanTypeandLimits(call);
                   
                    // Add the current call to the total for the plan type and Add to the PlanUsageTotals Dictionary
                    int totalcurrentCallTypePlan;
                    if (PlanUsageTotals.TryGetValue(planDetails.planTypeFriendlyName , out totalcurrentCallTypePlan))
                        PlanUsageTotals[planDetails.planTypeFriendlyName] = (int)(totalcurrentCallTypePlan + (call.duration / 60));
                    else
                        PlanUsageTotals.Add(planDetails.planTypeFriendlyName, (int)(call.duration / 60));
                }
                
                // Step 2. Output the Plan Usage Totals
                // a. loop through PlanUsageTotals and Display Totals by licenseCapability
                // b. Determine if we are under / over the plan limit
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

/// <summary>
/// Get the Current Plan Type and Limits
/// </summary>
PlanDetails GetCurrentPlanTypeandLimits(PstnLogCallRow call)
{
    
   bool InSelectCountriesFlag = false;
   bool callIsDomestic = false;
           
   // Check if call.usageCountryCode has one of these values: "UK","US","PR","CA"
   if (call.usageCountryCode == "UK" || call.usageCountryCode == "US" || call.usageCountryCode == "PR" || call.usageCountryCode == "CA")
        InSelectCountriesFlag = true;
   
   // Check if they are using the domestic or international Minutes
   if (call.destinationContext == "Domestic")
	   callIsDomestic = true;
       
    // Get the limit for the plan buckets (DOMESTIC_US_PR_CA_UK_OutBound_Limit,DOMESTIC_Other_OutBound_Limit,INTERNATIONAL_ALL_OutBound_Limit )
    KeyValuePair<string,int> currentCallTypePlanLimit = getPlanLimitByLicenseCapability(call.licenseCapability, callIsDomestic, InSelectCountriesFlag);

    // Return PLan Details
    return new PlanDetails { planTypeFriendlyName = currentCallTypePlanLimit.Key, planLimit = currentCallTypePlanLimit.Value};
}

/// <summary>
/// Get the Plan Limit for the License Capability
/// </summary>
KeyValuePair<string,int> getPlanLimitByLicenseCapability(string licenseCapability, bool callIsDomestic, bool inSelectCountriesFlag)
{
            // Deserialize plans.json to get the plan limits
            string plansJson = File.ReadAllText("plans.json");
            List<Plan> callingPlans = JsonSerializer.Deserialize<List<Plan>>(plansJson);
                        
            // linq query to get element in List<Plan> where Plan.LicenseCapability == licenseCapability
            Plan plan = callingPlans.Where(p => p.LicenseCapability == licenseCapability).FirstOrDefault();

            if (callIsDomestic)
            {
                if (inSelectCountriesFlag)
                    return new KeyValuePair<string, int>(string.Format("{0}_DOMESTIC_US_PR_CA_UK_OutBound_Type",plan.LicenseCapability), (int)plan.DOMESTIC_US_PR_CA_UK_OutBound_Limit);
                else
                    return new KeyValuePair<string, int>(string.Format("{0}_DOMESTIC_Other_OutBound_Type",plan.LicenseCapability), (int)plan.DOMESTIC_Other_OutBound_Limit);
            }
            else
            {
                return new KeyValuePair<string, int>(string.Format("{0}_INTERNATIONAL_ALL_OutBound_Type",plan.LicenseCapability), (int)plan.INTERNATIONAL_ALL_OutBound_Limit);
            }
            
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