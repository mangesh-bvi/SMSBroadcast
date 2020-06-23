using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using SMSBroadcastHomeshop.Model;
using SMSBroadcastHomeshop.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace SMSBroadcastHomeshop
{
    class Program
    {
        public static int delaytime = 0;

        static void Main(string[] args)
        {

            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
            delaytime = Convert.ToInt32(config.GetSection("MySettings").GetSection("IntervalInMinutes").Value);
            Thread _Individualprocessthread = new Thread(new ThreadStart(InvokeMethod));
            _Individualprocessthread.Start();
        }

        public static void InvokeMethod()
        {
            while (true)
            {

                //GetConnectionStrings();
                GetdataFromMySQL();
                Thread.Sleep(delaytime);
            }
        }
        //public static void GetConnectionStrings()
        //{
        //    string ServerName = string.Empty;
        //    string ServerCredentailsUsername = string.Empty;
        //    string ServerCredentailsPassword = string.Empty;
        //    string DBConnection = string.Empty;


        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
        //        var constr = config.GetSection("ConnectionStrings").GetSection("HomeShop").Value;
        //        MySqlConnection con = new MySqlConnection(constr);
        //        MySqlCommand cmd = new MySqlCommand("SP_HSGetAllConnectionstrings", con);
        //        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //        cmd.Connection.Open();
        //        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
        //        da.Fill(dt);
        //        cmd.Connection.Close();

        //        if (dt.Rows.Count > 0)
        //        {
        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                DataRow dr = dt.Rows[i];
        //                ServerName = Convert.ToString(dr["ServerName"]);
        //                ServerCredentailsUsername = Convert.ToString(dr["ServerCredentailsUsername"]);
        //                ServerCredentailsPassword = Convert.ToString(dr["ServerCredentailsPassword"]);
        //                DBConnection = Convert.ToString(dr["DBConnection"]);

        //                string ConString = "Data Source = " + ServerName + " ; port = " + 3306 + "; Initial Catalog = " + DBConnection + " ; User Id = " + ServerCredentailsUsername + "; password = " + ServerCredentailsPassword + "";
        //                GetdataFromMySQL(ConString);
        //            }
        //        }
        //    }
        //    catch 
        //    {


        //    }
        //    finally
        //    {

        //        GC.Collect();
        //    }


        //}


        public static void GetdataFromMySQL()
        {
            List<SMSEntity> lstSMSEntity = null;
            List<SMSSendResponse> lstSMSSendResponse = null;
            string SMSRequest = string.Empty;
            string SMSResponse = string.Empty;
            string UserName = string.Empty;
            string Password = string.Empty;
            string SMSURL = string.Empty;
            int ID = 0;
            var Programcode = string.Empty;
            var StoreCode = string.Empty;
            var CampaignCode = string.Empty;
            var MobileNumber = string.Empty;
            var EmailID = string.Empty;
            var MessageText = string.Empty;
            var SenderId = string.Empty;
            int ClientID = 0;
            string apiResponse = string.Empty;
            MySqlConnection con = null;
            try
            {
                DataTable dt = new DataTable();

                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                var constr = config.GetSection("ConnectionStrings").GetSection("HomeShop").Value;
                string ClientAPIURL = config.GetSection("ConnectionStrings").GetSection("ClientAPIURL").Value;
                con = new MySqlConnection(constr);
                MySqlCommand cmd = new MySqlCommand("SP_HSGetDetailforSMSBroadcast", con)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Connection.Open();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(dt);
                cmd.Connection.Close();


                lstSMSEntity = new List<SMSEntity>();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        ID = Convert.ToInt32(dr["ID"]);
                        Programcode = Convert.ToString(dr["Programcode"]);
                        StoreCode = Convert.ToString(dr["StoreCode"]);
                        CampaignCode = Convert.ToString(dr["CampaignCode"]);
                        MobileNumber = Convert.ToString(dr["MobileNumber"]);
                        EmailID = Convert.ToString(dr["EmailID"]);
                        MessageText = Convert.ToString(dr["MessageText"]);
                        SenderId = Convert.ToString(dr["SenderId"]);
                        ClientID = Convert.ToInt32(dr["ClientID"]);
                        if (!String.IsNullOrEmpty(MessageText))
                        {

                            SMSEntity objSMSEntity = new SMSEntity()
                            {
                                SMSText = MessageText,
                                SMSID = ID.ToString(),
                                SMSSenderID = SenderId,
                                MobileNumber = MobileNumber,
                                SMSSEQ = ID.ToString()
                            };
                            lstSMSEntity.Add(objSMSEntity);
                        }


                    }
                    SMSRequest = SMSCore.PrepareBulkSMSXML(lstSMSEntity);
                    SMSSendResponse chatSendSMSResponse = new SMSSendResponse();
                    if (!string.IsNullOrEmpty(SMSRequest))
                    {
                        UserName = config.GetSection("MySettings").GetSection("SMSAPIUserName").Value;
                        Password = config.GetSection("MySettings").GetSection("SMSAPIPassword").Value;
                        SMSURL = config.GetSection("MySettings").GetSection("SMSAPIURL").Value;
                        SMSResponse = SMSCore.ProcessBulkSMSXML(SMSRequest, UserName, Password, SMSURL);
                        if (!string.IsNullOrEmpty(SMSResponse))
                        {
                            lstSMSSendResponse = new List<SMSSendResponse>();
                            lstSMSSendResponse = SMSCore.ParseSendBulkSMSResponse(SMSResponse);

                            for (int j = 0; j < lstSMSSendResponse.Count; j++)
                            {

                                if (lstSMSSendResponse[j].ErrorCODE == null & lstSMSSendResponse[j].ErrorSEQ == null)
                                {
                                    string Responcetext = "Success";
                                    UpdateResponse(Convert.ToInt32(lstSMSSendResponse[j].ID), lstSMSSendResponse[j].SubmitDate, Responcetext, 1);

                                }
                                else
                                {
                                    string Responcetext = "Fail";
                                    UpdateResponse(Convert.ToInt32(lstSMSSendResponse[j].ID), lstSMSSendResponse[j].SubmitDate, Responcetext, 2);

                                }

                            }

                        }
                    }

                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
                GC.Collect();
            }
        }
        public static void UpdateResponse(int ID, string Date, string Responcetext, int IsSend)
        {

            try
            {
                DataTable dt = new DataTable();
                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                var constr = config.GetSection("ConnectionStrings").GetSection("HomeShop").Value;
                MySqlConnection con = new MySqlConnection(constr);
                MySqlCommand cmd = new MySqlCommand("SP_HSUpdateBroadcastResponce", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@_iD", ID);
                cmd.Parameters.AddWithValue("@_date", Date);
                cmd.Parameters.AddWithValue("@_responcetext", Responcetext);
                cmd.Parameters.AddWithValue("@_isSend", IsSend);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch
            {

            }
            finally
            {
                GC.Collect();
            }

        }

    }
}
