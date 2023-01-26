using System.Text.Json.Serialization;

namespace callRecords.Models
{

    public class PstnLogCallRows
    {
        [JsonPropertyName("@odata.context")]
       public string? odatacontext { get; set; }

        [JsonPropertyName("@odata.count")]
        public int? odatacount { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string? odatanextlink { get; set; }
        
        [JsonPropertyName("value")]
        public List<PstnLogCallRow>? pstnLogCallRow { get; set; }
    }

    public class PstnLogCallRow
    {
        [JsonPropertyName("id")]
       public string? id { get; set; }

        [JsonPropertyName("callId")]
       public string? callId { get; set; }

        [JsonPropertyName("userId")]
       public string? userId { get; set; }

        [JsonPropertyName("userPrincipalName")]
       public string? userPrincipalName { get; set; }

        [JsonPropertyName("userDisplayName")]
       public string? userDisplayName { get; set; }

        [JsonPropertyName("startDateTime")]
        public DateTime? startDateTime { get; set; }

        [JsonPropertyName("endDateTime")]
        public DateTime? endDateTime { get; set; }

        [JsonPropertyName("duration")]
        public int? duration { get; set; }

        [JsonPropertyName("charge")]
        public int? charge { get; set; }

        [JsonPropertyName("callType")]
       public string? callType { get; set; }

        [JsonPropertyName("currency")]
       public string? currency { get; set; }

        [JsonPropertyName("calleeNumber")]
       public string? calleeNumber { get; set; }

        [JsonPropertyName("usageCountryCode")]
       public string? usageCountryCode { get; set; }

        [JsonPropertyName("tenantCountryCode")]
       public string? tenantCountryCode { get; set; }

        [JsonPropertyName("connectionCharge")]
        public int? connectionCharge { get; set; }

        [JsonPropertyName("callerNumber")]
       public string? callerNumber { get; set; }

        [JsonPropertyName("destinationContext")]
       public string? destinationContext { get; set; }

        [JsonPropertyName("destinationName")]
       public string? destinationName { get; set; }

        [JsonPropertyName("conferenceId")]
        public object? conferenceId { get; set; }

        [JsonPropertyName("licenseCapability")]
       public string? licenseCapability { get; set; }

        [JsonPropertyName("inventoryType")]
       public string? inventoryType { get; set; }

        [JsonPropertyName("operator")]
        public object? @operator { get; set; }

        [JsonPropertyName("callDurationSource")]
       public string? callDurationSource { get; set; }
    }


}