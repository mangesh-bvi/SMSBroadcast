using System;
using System.Collections.Generic;
using System.Text;

namespace SMSBroadcastHomeshop.Model
{
    public class SMSSendResponse
    {
        public string GUID { get; set; }
        public string SubmitDate { get; set; }
        public string ID { get; set; }
        public string ErrorSEQ { get; set; }
        public string ErrorCODE { get; set; }
    }
}
