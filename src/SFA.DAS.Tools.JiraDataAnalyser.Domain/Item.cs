using Newtonsoft.Json;

namespace SFA.DAS.Tools.JiraDataAnalyser.Domain
{
    public class Item
    {
        public string FieldId { get; set; }
        public string Field { get; set; }
        
        [JsonProperty("fromString")]
        public string FromState { get; set; }
        
        [JsonProperty("toString")]
        public string ToState { get; set; }
    }
}