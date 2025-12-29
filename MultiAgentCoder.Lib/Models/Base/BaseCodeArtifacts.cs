using MultiAgentCoder.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiAgentCoder.Domain.Models.Base
{
    public class BaseCodeArtifacts
    {

        public string ReferrenceFileName { get; set; }

        public string WorkingDirectory { get; set; } = string.Empty;

        public string SuggestedFileName { get; set; } = "GeneratedCode.cs"; // later on genertae based on content

        /// <summary>
        /// Current version of the source code.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Build output (compiler messages).
        /// </summary>
        public string? BuildOutput { get; private set; }

        /// <summary>
        /// Number of iterations this artifact has gone through.
        /// </summary>
        public int Revision { get; set; } = 0;

        public CodeType CodeType { get; set; } = CodeType.SourceCode;

        public List<string> Feedbacks { get; set; } = new List<string>();

        /// <summary>
        /// Timestamp when this artifact was first created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when this artifact was last modified.
        /// </summary>
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        /* ---------- Behavior ---------- */

        public void UpdateSourceCode(string updatedCode)
        {
            Content = updatedCode ?? throw new ArgumentNullException(nameof(updatedCode));
            IncrementRevision();
        }

      
        public void RecordBuildOutput(string buildOutput)
        {
            BuildOutput = buildOutput;
            LastUpdatedAt = DateTime.UtcNow;
        }

        private void IncrementRevision()
        {
            Revision++;
            LastUpdatedAt = DateTime.UtcNow;
        }

        /* ---------- Diagnostics ---------- */

        public virtual string GetSummary()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Revision: {Revision}");
            sb.AppendLine($"Last Updated: {LastUpdatedAt:u}");

            sb.AppendLine($"Build Output: {(BuildOutput is null ? "N/A" : "Available")}");

            return sb.ToString();
        }
    }
}
