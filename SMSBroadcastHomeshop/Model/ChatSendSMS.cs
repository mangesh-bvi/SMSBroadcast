using System;
using System.Collections.Generic;
using System.Text;

namespace SMSBroadcastHomeshop.Model
{
    public class ChatSendSMS
    {
        /// <summary>
        /// MobileNumber
        /// </summary>
        public string MobileNumber { get; set; }
        /// <summary>
        /// SenderId
        /// </summary>
        public string SenderId { get; set; }
        /// <summary>
        /// SmsText
        /// </summary>
        public string SmsText { get; set; }

    }
}
