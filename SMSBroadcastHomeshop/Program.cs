using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using SMSBroadcastHomeshop.Model;
using SMSBroadcastHomeshop.Service;
using System;
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

                ChatSendSMSResponse chatSendSMSResponse = new ChatSendSMSResponse();

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
                            ChatSendSMS chatSendSMS = new ChatSendSMS
                            {
                                MobileNumber = MobileNumber.Length > 10 ? MobileNumber : "91" + MobileNumber.TrimStart('0'),
                                SenderId = SenderId,
                                SmsText = MessageText
                            };

                            try
                            {
                                string apiReq = JsonConvert.SerializeObject(chatSendSMS);
                                apiResponse = CommonService.SendApiRequest(ClientAPIURL + "api/ChatbotBell/SendSMS", apiReq);

                                chatSendSMSResponse = JsonConvert.DeserializeObject<ChatSendSMSResponse>(apiResponse);

                                if (chatSendSMSResponse.ErrorCODE == null & chatSendSMSResponse.ErrorSEQ == null)
                                {
                                    string Responcetext = "Success";
                                    UpdateResponse(ID, chatSendSMSResponse.SubmitDate, Responcetext, 1);

                                }
                                else
                                {
                                    string Responcetext = "Fail";
                                    UpdateResponse(ID, chatSendSMSResponse.SubmitDate, Responcetext, 2);

                                }
                            }
                            catch (Exception ex)
                            {
                                string Responcetext = ex.ToString();
                                UpdateResponse(ID, DateTime.Now.ToString(), Responcetext, 2);
                            }

                        }

                    }
                }
            }
            catch
            {
                
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
        public static void UpdateResponse(int ID,string Date,string Responcetext,int IsSend)
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
