using System.Text.Json.Serialization;

namespace callRecords.Models
{
public class pstnCallLogRow
    {
        [JsonPropertyName("@odata.type")]
        public string odatatype { get; set; }

        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("callId")]
        public string callId { get; set; }

        [JsonPropertyName("userId")]
        public string userId { get; set; }

        [JsonPropertyName("userPrincipalName")]
        public string userPrincipalName { get; set; }

        [JsonPropertyName("userDisplayName")]
        public string userDisplayName { get; set; }

        [JsonPropertyName("startDateTime")]
        public string startDateTime { get; set; }

        [JsonPropertyName("endDateTime")]
        public string endDateTime { get; set; }

        [JsonPropertyName("duration")]
        public string duration { get; set; }

        [JsonPropertyName("charge")]
        public string charge { get; set; }

        [JsonPropertyName("callType")]
        public string callType { get; set; }

        [JsonPropertyName("currency")]
        public string currency { get; set; }

        [JsonPropertyName("calleeNumber")]
        public string calleeNumber { get; set; }

        [JsonPropertyName("usageCountryCode")]
        public string usageCountryCode { get; set; }

        [JsonPropertyName("tenantCountryCode")]
        public string tenantCountryCode { get; set; }

        [JsonPropertyName("connectionCharge")]
        public string connectionCharge { get; set; }

        [JsonPropertyName("callerNumber")]
        public string callerNumber { get; set; }

        [JsonPropertyName("destinationContext")]
        public string destinationContext { get; set; }

        [JsonPropertyName("destinationName")]
        public string destinationName { get; set; }

        [JsonPropertyName("conferenceId")]
        public string conferenceId { get; set; }

        [JsonPropertyName("licenseCapability")]
        public string licenseCapability { get; set; }

        [JsonPropertyName("inventoryType")]
        public string inventoryType { get; set; }

        [JsonPropertyName("operator")]
        public string @operator { get; set; }

        [JsonPropertyName("callDurationSource")]
        public string callDurationSource { get; set; }
    }

}