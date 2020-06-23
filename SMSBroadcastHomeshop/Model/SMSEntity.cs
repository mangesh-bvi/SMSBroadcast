using System;
using System.Collections.Generic;
using System.Text;

namespace SMSBroadcastHomeshop.Model
{
   

    public class SMSEntity
    {
        public string MobileNumber { get; set; }
        public string SMSText { get; set; }
        public string SMSSenderID { get; set; }
        public string SMSID { get; set; }
        public string SMSSEQ { get; set; }
        public string ApiPartnerCode { get; set; }
        public string ServerCode { get; set; }
    }

   
}
