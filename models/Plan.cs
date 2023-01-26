using System.Text.Json.Serialization;

    public class Plan
    {
        [JsonPropertyName("licenseCapability")]
        public string? LicenseCapability { get; set; }

        [JsonPropertyName("DOMESTIC_US_PR_CA_UK_OutBound_Limit")]
        public int DOMESTIC_US_PR_CA_UK_OutBound_Limit { get; set; }

        [JsonPropertyName("DOMESTIC_Other_OutBound_Limit")]
        public int DOMESTIC_Other_OutBound_Limit { get; set; }

        [JsonPropertyName("INTERNATIONAL_ALL_OutBound_Limit")]
        public int INTERNATIONAL_ALL_OutBound_Limit { get; set; }
    }

