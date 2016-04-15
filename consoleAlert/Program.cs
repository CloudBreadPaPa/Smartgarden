using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using Slack.Webhooks;

namespace consoleAlert
{
    class Program
    {
        public static void SendSlackMsg(string Channel, string Text, string Username)
        {
            var slackClient = new SlackClient("mywebhooklink");

            var slackMessage = new SlackMessage
            {
                Channel = Channel,
                Text = Text,
                Username = Username
            };

            try
            {
                slackClient.Post(slackMessage);

            }
            catch (Exception)
            {

                throw;
            }
        }

        static void Main(string[] args)
        {

            while (true)
            {

                using (SqlConnection connection = new SqlConnection("connectionstring"))
                {
                    using (SqlCommand command = new SqlCommand("select top 1 prediction from garden order by SendDT desc", connection))
                    {
                        command.CommandType = CommandType.Text;
                        //command.Parameters.Add("@MemberID", SqlDbType.NVarChar, -1).Value = p.MemberID;
                        connection.Open();

                        using (SqlDataReader dreader = command.ExecuteReader())
                        {
                            while (dreader.Read())
                            {
                                //("41.00027357629127", CultureInfo.InvariantCulture.NumberFormat);
                                if (float.Parse(dreader[0].ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat) > 0.5)
                                {
                                    // send slack message
                                    SendSlackMsg("#general", "SmartGarden Alert", "The prediction value is : " + dreader[0].ToString());
                                }

                            }
                            dreader.Close();
                        }
                        connection.Close();
                    }
                }

                Task.Delay(1000).Wait();
            }
        }
    }
}
