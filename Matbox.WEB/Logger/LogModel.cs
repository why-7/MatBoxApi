﻿using System;

namespace Matbox.WEB.Logger
{
    public class LogModel
    {
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string Method { get; set; }
        public string RequestBody { get; set; } = string.Empty;
        public string Response { get; set; }
        public string ResponseCode { get; set; }
        public DateTime RequestedOn { get; set; }
        public DateTime RespondedOn { get; set; }
    }
}