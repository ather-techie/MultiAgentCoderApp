using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiAgentCoder.Domain.Models.Results;

public class CodeResult
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }
}
