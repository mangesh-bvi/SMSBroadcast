using System;
using System.Collections.Generic;
using System.Text;

namespace SMSBroadcastHomeshop.Model
{
    public class ChatSendSMSResponse
    {
        /// <summary>
        /// Guid
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// SubmitDate
        /// </summary>
        public string SubmitDate { get; set; }
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// ErrorSEQ
        /// </summary>
        public string ErrorSEQ { get; set; }
        /// <summary>
        /// ErrorCODE
        /// </summary>
        public string ErrorCODE { get; set; }
    }

}
