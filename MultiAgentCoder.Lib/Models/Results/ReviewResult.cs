using System.Text.Json.Serialization;

namespace MultiAgentCoder.Domain.Models.Results
{
    public sealed class ReviewResult
    {
        [JsonPropertyName("isApproved")]
        public bool IsApproved { get { return string.Compare(Feedback, "APPROVED", true) > 0; } }

        [JsonPropertyName("feedback")]
        public string Feedback { get; init; } = string.Empty;

        [JsonPropertyName("reviewComments")]
        public IReadOnlyList<string> ReviewComments { get; init; }
            = Array.Empty<string>();
    }

}
