using System;
using System.Collections.Generic;

namespace SFA.DAS.Tools.JiraDataAnalyser.Domain
{
    public class SinkMessage
    {
        public string BoardId { get; set; }
        public IEnumerable<TransitionData> Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}