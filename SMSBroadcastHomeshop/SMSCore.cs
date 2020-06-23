using Microsoft.Extensions.Configuration;
using SMSBroadcastHomeshop.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SMSBroadcastHomeshop
{
    public static class SMSCore
    {
        public static string PrepareBulkSMSXML(List<SMSEntity> lstSMSEntity)
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();

            StringBuilder sbSMSXML = null;
            string USERNAME = string.Empty;
            string PASSWORD = string.Empty;
            string UDH = string.Empty;
            string CODING = string.Empty;
            string TEXT = string.Empty;
            string PROPERTY = string.Empty;
            string ID = string.Empty;
            string DLR = string.Empty;
            string VALIDITY = string.Empty;
            string FROM = string.Empty;
            string TO = string.Empty;
            string SEQ = string.Empty;
            string TAG = string.Empty;
            string SEND_ON = string.Empty;
            string SENDONVAL = string.Empty;
            string TAGVAL = string.Empty;
            try
            {
                sbSMSXML = new StringBuilder();
                USERNAME =   config.GetSection("MySettings").GetSection("SMSAPIUserName").Value;
                PASSWORD =   config.GetSection("MySettings").GetSection("SMSAPIPassword").Value;
                UDH =        config.GetSection("MySettings").GetSection("UDH").Value;
                CODING =     config.GetSection("MySettings").GetSection("CODING").Value;
                PROPERTY =   config.GetSection("MySettings").GetSection("PROPERTY").Value;
                DLR =        config.GetSection("MySettings").GetSection("DLR").Value;
                VALIDITY =   config.GetSection("MySettings").GetSection("VALIDITY").Value;
                SENDONVAL =  config.GetSection("MySettings").GetSection("SENDONVAL").Value;
                TAGVAL =     config.GetSection("MySettings").GetSection("TAGVAL").Value;


                /* 
                    <?xml version="1.0" encoding="ISO-8859-1"?> 
                    <!DOCTYPE MESSAGE SYSTEM "http://127.0.0.1/psms/dtd/message.dtd" > 
                    <MESSAGE> <USER USERNAME="mycompany" PASSWORD="mycompany"/> 
                    <SMS UDH="0" CODING="1" TEXT="hey this is a real test" PROPERTY="" ID="1" DLR="0" VALIDITY="0"> 
                    <ADDRESS FROM="9812345678" TO="919812345678" SEQ="1" TAG="some client side random data" /> 
                    <ADDRESS FROM="9812345678" TO="mycompany" SEQ="2" /> <ADDRESS FROM="VALUEFIRST" TO="919812345678" SEQ="3" /> 
                    </SMS> 
                    <SMS UDH="0" CODING="1" TEXT="hey this is a real test" PROPERTY="" ID="2" SEND_ON="2007-10-15 20:10:10 +0530"> 
                    <ADDRESS FROM="9812345678" TO="919812345678" SEQ="1" /> <ADDRESS FROM="9812345678" TO="919812345678" SEQ="2" /> 
                    <ADDRESS FROM="VALUEFIRST" TO="919812345678" SEQ="3" /> <ADDRESS FROM="9812345678" TO="919812345678" SEQ="4" /> 
                    <ADDRESS FROM="VALUEFIRST" TO="919812345678" SEQ="5" /> <ADDRESS FROM="VALUEFIRST" TO="919812345678" SEQ="6" /> 
                    </SMS> 
                    </MESSAGE>
                */

                sbSMSXML.Append(@"<?xml version= ""1.0"" encoding= ""ISO-8859-1""?>");
                sbSMSXML.Append(@"<!DOCTYPE MESSAGE SYSTEM ""http://127.0.0.1:80/psms/dtd/messagev12.dtd"" >");
                sbSMSXML.Append(@"<MESSAGE VER=""1.2"">");
                sbSMSXML.Append(string.Format(@"<USER USERNAME=""{0}"" PASSWORD=""{1}""/>", USERNAME, PASSWORD));
                foreach (SMSEntity objSMSEntity in lstSMSEntity)
                {
                    TEXT = objSMSEntity.SMSText;
                    ID = objSMSEntity.SMSID;
                    FROM = objSMSEntity.SMSSenderID;
                    TO = objSMSEntity.MobileNumber;
                    SEQ = objSMSEntity.SMSSEQ;
                    TAG = ID + "-" + SEQ;
                    SEND_ON = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (SENDONVAL == "1")
                    {
                        //sbSMSXML.Append(string.Format(@"<SMS UDH=""{0}"" CODING=""{1}"" TEXT=""{2}"" PROPERTY=""{3}"" ID=""{4}"" SEND_ON=""{5}"">", UDH, CODING, (ID + TEXT), PROPERTY, ID, SEND_ON));
                        sbSMSXML.Append(string.Format(@"<SMS UDH=""{0}"" CODING=""{1}"" TEXT=""{2}"" PROPERTY=""{3}"" ID=""{4}"" SEND_ON=""{5}"">", UDH, CODING, TEXT, PROPERTY, ID, SEND_ON));
                    }
                    else
                    {
                        //sbSMSXML.Append(string.Format(@"<SMS UDH=""{0}"" CODING=""{1}"" TEXT=""{2}"" PROPERTY=""{3}"" ID=""{4}"" DLR=""{5}"" VALIDITY=""{6}"">", UDH, CODING, (ID + TEXT), PROPERTY, ID, DLR, VALIDITY));
                        sbSMSXML.Append(string.Format(@"<SMS UDH=""{0}"" CODING=""{1}"" TEXT=""{2}"" PROPERTY=""{3}"" ID=""{4}"" DLR=""{5}"" VALIDITY=""{6}"">", UDH, CODING, TEXT, PROPERTY, ID, DLR, VALIDITY));
                    }
                    if (TAGVAL == "1")
                    {
                        sbSMSXML.Append(string.Format(@"<ADDRESS FROM=""{0}"" TO=""{1}"" SEQ=""{2}"" TAG=""{3}"" />", FROM, TO, SEQ, TAG));
                    }
                    else
                    {
                        sbSMSXML.Append(string.Format(@"<ADDRESS FROM=""{0}"" TO=""{1}"" SEQ=""{2}"" />", FROM, TO, SEQ));
                    }
                    sbSMSXML.Append("</SMS>");
                }
                sbSMSXML.Append("</MESSAGE>");
            }
            catch (Exception ex)
            {
                sbSMSXML = null;
                throw ex;
            }

            return sbSMSXML.ToString();
        }

        public static string ProcessBulkSMSXML(string BulkXML, string UserName, string Password, string strAPIURL)
        {

            string responseFromServer = string.Empty;
            string EncodedBulkXML = string.Empty;
            string strPost = string.Empty;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                EncodedBulkXML = System.Web.HttpUtility.UrlEncode(BulkXML);

                //Creation of Request object to api 
                strPost = "data=" + EncodedBulkXML + "&action=send";

                HttpWebRequest objRequest = (HttpWebRequest)(WebRequest.Create(strAPIURL));
                objRequest.Method = "POST";
                objRequest.ContentType = "application/x-www-form-urlencoded";
                objRequest.ContentLength = strPost.Length;

                //Get the requeststream object and write characters to the stream
                StreamWriter myWriter = new StreamWriter(objRequest.GetRequestStream());
                objRequest.KeepAlive = false;
                myWriter.Write(strPost);
                myWriter.Flush();
                myWriter.Close();

                //Response from api 
                System.Net.HttpWebResponse objResponse = (System.Net.HttpWebResponse)(objRequest.GetResponse());
                StreamReader sr = new StreamReader(objResponse.GetResponseStream());
                responseFromServer = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception ex)
            {
                responseFromServer = ex.Message.ToString();
            }
            return responseFromServer;
        }

        public static List<SMSSendResponse> ParseSendBulkSMSResponse(string ResponseXML)
        {
            List<SMSSendResponse> lstSMSResponse = null;
            SMSSendResponse objSMSResponse = null;

            try
            {
                lstSMSResponse = new List<SMSSendResponse>();

                foreach (XElement level1Element in XElement.Parse(ResponseXML).Elements())
                {
                    objSMSResponse = new SMSSendResponse();
                    objSMSResponse.GUID = level1Element.Attribute("GUID") != null ? level1Element.Attribute("GUID").Value : string.Empty;
                    objSMSResponse.SubmitDate = level1Element.Attribute("SUBMITDATE") != null ? level1Element.Attribute("SUBMITDATE").Value : string.Empty;
                    objSMSResponse.ID = level1Element.Attribute("ID") != null ? level1Element.Attribute("ID").Value : string.Empty;

                    foreach (XElement level2Element in level1Element.Elements("ERROR"))
                    {
                        objSMSResponse.ErrorSEQ = level2Element.Attribute("SEQ") != null ? level2Element.Attribute("SEQ").Value : string.Empty;
                        objSMSResponse.ErrorCODE = level2Element.Attribute("CODE") != null ? level2Element.Attribute("CODE").Value : string.Empty;
                    }
                    if (!string.IsNullOrEmpty(objSMSResponse.GUID))
                        lstSMSResponse.Add(objSMSResponse);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lstSMSResponse;
        }
    }
}
