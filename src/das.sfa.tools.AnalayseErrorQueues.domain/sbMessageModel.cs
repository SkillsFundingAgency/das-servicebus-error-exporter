﻿using System;

namespace das.sfa.tools.AnalayseErrorQueues.domain
{
    public class sbMessageModel
    {
        public string MessageId { get; set; }
        public string TimeOfFailure { get; set; }
        public string ExceptionType { get; set; }
        public string OriginatingEndpoint { get; set; }
        public string ProcessingEndpoint { get; set; }
        public string EnclosedMessageTypes { get; set; }
        public string ExceptionMessage { get; set; }
        public object StackTrace { get; set; }
    }
}
