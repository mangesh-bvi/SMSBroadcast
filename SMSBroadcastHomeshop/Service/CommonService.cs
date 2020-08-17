using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace SMSBroadcastHomeshop.Service
{
   public class CommonService
    {
        /// <summary>
        ///SEND api request
        /// </summary>
        /// 
        public static string SendApiRequest(string url, string Request)
        {
            string strresponse = "";
            try
            {
                var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "text/json";

                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    if (!string.IsNullOrEmpty(Request))
                        streamWriter.Write(Request);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    strresponse = streamReader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return strresponse;

        }

        public static string SendParamsApiRequest(string url, NameValueCollection Request)
        {
            string strresponse = string.Empty;
            try
            {
                WebClient wc = new WebClient();
                wc.QueryString = Request;

                var data = wc.UploadValues(url, "POST", wc.QueryString);

                // data here is optional, in case we recieve any string data back from the POST request.
                strresponse = UnicodeEncoding.UTF8.GetString(data);
            }
            catch (Exception)
            {

                throw;
            }

            return strresponse;


        }

    }
}
