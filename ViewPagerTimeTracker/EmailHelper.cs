using Android.Util;
using SQLite;
using System;
using System.Linq;
using System.Net.Mail;
using System.Text;
using TimeTrackerUniversal.Database;
using TimeTrackerUniversal.Database.Schema;

namespace TimeTrackerUniversal
{
    class EmailHelper
    {
        #region MyRegion
        //const string server = "smtp.office365.com";
        //const string myEmailAddress = "NewNetValidator@outlook.com";
        //const string myEmailAddress = "kevin.jakub@newnetservices.net";
        //const string myPassword = "KJkj!!21";
        //string[] toEmails = {"kevin.jakub@newnetservices.net" };GetString(Resource.String.delete));
        //static string toEmail = 
        //FindViewById<CheckBox>(Resource.Id.IsReal).Checked ? FindViewById<ImageButton>(Resource.Id.clockInButton);
        //"kevin.jakub@newnetservices.net", "kjakub777@gmail.com" ;/*Resource.String.to_email_address*/
        #endregion


        public static string SendEmail(string message, bool IsOut, bool IsReal, Func<int, string> GetString)
        {
            using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
            {
                string output = string.Empty;
                var mapp = connection.TableMappings;
                foreach (TableMapping tm in mapp)
                {
                    Log.Debug("MAPPING", tm.ToString());
                }
                EmailAddresses toEmailReal = connection.Table<EmailAddresses>().Where(x => x.EmailType == 1).Last();

                EmailAddresses myEmailAddress = connection.Table<EmailAddresses>().Where(x => x.EmailType == 2).Last();
                EmailAddresses myBCCAddress = connection.Table<EmailAddresses>().Where(x => x.EmailType == 3).Last();
                string server = GetString(Resource.String.server);

                FromPassword fromPassword = connection.Table<FromPassword>().Last();

                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = server;
                client.EnableSsl = true;
                client.Timeout = 120000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(myEmailAddress.Email, fromPassword.Pass);
                MailAddressCollection to = new MailAddressCollection();

                MailMessage mm = new MailMessage();
                mm.From = new MailAddress(myEmailAddress.Email);
                if (myBCCAddress != null)
                    mm.Bcc.Add(myBCCAddress.Email);
                if (IsReal)
                {
                    mm.To.Add(toEmailReal.Email);
                    output = "REAL to " + toEmailReal.Email;
                }
                else
                {
                    mm.To.Add(myEmailAddress.Email);
                    output = "TEST to " + myEmailAddress.Email;
                }
                if (IsOut)
                    mm.Subject = "Clock out";
                else
                    mm.Subject = "Clock in";
                output += " " + mm.Subject;
                mm.Body = string.Empty;// message;
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                try
                {
                    client.Send(mm);
                    return output;
                }
                catch (Exception ex)
                {
                    output += "ERROR | " + ex.Message + " Make sure you go to Change Emails and set values!";
                    return output;
                }
            }
        }//end mail
    };

}