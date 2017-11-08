using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using TimeTrackerUniversal.Database;
using TimeTrackerUniversal.Database.Schema;
using SQLite;
using System.IO;

namespace TimeTrackerUniversal
{
    [Activity(Label = "Change Emails", MainLauncher = false, Icon = "@drawable/icon")]
    public class ChangeEmailsActivity : Activity
    {
        static readonly object _syncLock = new object();
        TextView lblPass;
        EditText pass1;
        TextView lblOut;
        Button btnOk;
        TextView lblEmail;
        EditText txtToNewEmail;
        EditText txtFromNewEmail;
        EditText txtBCCNewEmail;
        EditText txtRate;
        EditText txtServer;
        TextView lblOutEmail;
        Button btnExitPass;
        Button btnExport;
        Button btnImport;


        public ChangeEmailsActivity()
        {


        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ChangeEmails);

            lblPass = FindViewById<TextView>(Resource.Id.lblPass);
            pass1 = FindViewById<EditText>(Resource.Id.pass1);
            txtRate = FindViewById<EditText>(Resource.Id.txtRate);
            txtServer = FindViewById<EditText>(Resource.Id.txtServer);
            lblOut = FindViewById<TextView>(Resource.Id.lblOut);
            btnOk = FindViewById<Button>(Resource.Id.btnOk);
            lblEmail = FindViewById<TextView>(Resource.Id.lblEmail);
            txtToNewEmail = FindViewById<EditText>(Resource.Id.txtToNewEmail);
            txtFromNewEmail = FindViewById<EditText>(Resource.Id.txtFromNewEmail);
            txtBCCNewEmail = FindViewById<EditText>(Resource.Id.txtBCCNewEmail);
            lblOutEmail = FindViewById<TextView>(Resource.Id.lblOutEmail);
            btnExport = FindViewById<Button>(Resource.Id.btnExport);
            btnImport = FindViewById<Button>(Resource.Id.btnImport);
            btnExitPass = FindViewById<Button>(Resource.Id.btnExitPass);

            DateTime dt = DateTime.Now.ToLocalTime();//.Month
            Java.Lang.Boolean b = Java.Lang.Boolean.True;
            btnOk.Click += btnOk_Click;
            btnExitPass.Click += btnExitPass_Click;
            btnExport.Click += btnExport_Click;
            btnImport.Click += btnImport_Click;

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            lock (_syncLock)
            {
                string[] fileStr;
                //1=to,2=from,3=BCC
                using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
                {
                    var to = connection.Table<EmailAddresses>().Where(x => x.EmailType == 1).Last();
                    var from = connection.Table<EmailAddresses>().Where(x => x.EmailType == 2).Last();
                    var bcc = connection.Table<EmailAddresses>().Where(x => x.EmailType == 3).Last();
                    var hr = connection.Table<HourlyRate>().Last();
                    var pass = connection.Table<FromPassword>().Last();
                    fileStr = new string[] { $"{to.Email}{System.Environment.NewLine}",
                    $"{from.Email}{System.Environment.NewLine}",
                    $"{bcc.Email}{System.Environment.NewLine}",
                    $"{hr.Rate}{System.Environment.NewLine}",
                    $"{pass.Pass}{System.Environment.NewLine}"
              };
                }

                //****
                string filename = "backupEmails.txt";
                var SystemPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                SystemPath = Path.Combine(SystemPath, filename);
                if (!File.Exists(SystemPath))
                {
                    try
                    {
                        File.Create(SystemPath);
                    }
                    catch (Exception exception)
                    {

                        Toast.MakeText(ApplicationContext, "INTERNAL " + exception.ToString(), ToastLength.Long).Show();
                    }
                }
                // File.OpenWrite(fileName); 

                File.AppendAllLines(SystemPath, fileStr);

                string backupDir = "/sdcard/";

                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);
                if (!File.Exists(Path.Combine(backupDir, filename)))
                {
                    try
                    {

                        File.Create(Path.Combine(backupDir, filename));
                    }
                    catch (Exception exception)
                    {

                        Toast.MakeText(ApplicationContext, "LOCAL "+exception.ToString(), ToastLength.Long).Show();
                    }
                }
                if (File.Exists(SystemPath))
                    File.Copy(SystemPath, Path.Combine(backupDir, filename), true);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            lock (_syncLock)
            {
                string filename = "backupEmails.txt";
                var SystemPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                SystemPath = Path.Combine(SystemPath, filename);
                string[] fileStr = File.ReadAllLines(SystemPath);
                //1=to,2=from,3=BCC
                using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
                {
                    int c = connection.CreateTable<HourlyRate>(CreateFlags.AutoIncPK);
                    c = connection.CreateTable<EmailAddresses>(SQLite.CreateFlags.AutoIncPK);
                    c = connection.CreateTable<WorkInstance>(SQLite.CreateFlags.AutoIncPK);
                    c = connection.CreateTable<FromPassword>(SQLite.CreateFlags.AutoIncPK);
                    c = connection.CreateTable<ServerOut>(SQLite.CreateFlags.AutoIncPK);

                    connection.Insert(new EmailAddresses()
                    {
                        Date = MainActivity.GetLocalTime(),
                        IsValid = true,
                        Email = fileStr[0],
                        EmailType = 1
                    });
                    connection.Insert(new EmailAddresses()
                    {
                        Date = MainActivity.GetLocalTime(),
                        IsValid = true,
                        Email = fileStr[1],
                        EmailType = 2
                    });
                    connection.Insert(new EmailAddresses()
                    {
                        Date = MainActivity.GetLocalTime(),
                        IsValid = true,
                        Email = fileStr[2],
                        EmailType = 3
                    });
                    connection.Insert(new HourlyRate()
                    {
                        Date = MainActivity.GetLocalTime(),
                        IsValid = true,
                        Rate = (float)Convert.ToDouble(fileStr[3]),
                    });
                    connection.Insert(new FromPassword()
                    {
                        Date = MainActivity.GetLocalTime(),
                        IsValid = true,
                        Pass = fileStr[4],
                    });
                }
            }
        }

        private void btnExitPass_Click(object sender, EventArgs e)
        {
            SetResult(Result.Ok);
            this.OnBackPressed();
        }


        private void btnOk_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
            {
                DateTime dt = MainActivity.GetLocalTime();
                if (!string.IsNullOrWhiteSpace(txtToNewEmail.Text))
                {
                    connection.Insert(new EmailAddresses()
                    {
                        Date = dt,
                        IsValid = true,
                        EmailType = 1,
                        Email = txtToNewEmail.Text
                    });
                    EmailAddresses newto = connection.Table<EmailAddresses>().Where(x => x.EmailType == 1).Last();
                    lblOutEmail.Text += $"OK TO=> {newto.ToString()}\n";
                }
                if (!string.IsNullOrWhiteSpace(txtFromNewEmail.Text))
                {
                    connection.Insert(new EmailAddresses()
                    {
                        Date = dt,
                        IsValid = true,
                        EmailType = 2,
                        Email = txtFromNewEmail.Text
                    });
                    EmailAddresses newfrom = connection.Table<EmailAddresses>().Where(x => x.EmailType == 2).Last();
                    lblOutEmail.Text += $"OK FROM=> {newfrom.ToString()}\n";
                }
                if (!string.IsNullOrWhiteSpace(txtBCCNewEmail.Text))
                {
                    connection.Insert(new EmailAddresses()
                    {
                        Date = dt,
                        IsValid = true,
                        EmailType = 3,
                        Email = txtBCCNewEmail.Text
                    });
                    EmailAddresses newbcc = connection.Table<EmailAddresses>().Where(x => x.EmailType == 3).Last();
                    lblOutEmail.Text += $"OK BCC=> {newbcc.ToString()}\n";
                }
                if (!string.IsNullOrWhiteSpace(pass1.Text))
                {
                    connection.Insert(new FromPassword()
                    {
                        Date = dt,
                        IsValid = true,
                        Pass = pass1.Text,
                    });
                    FromPassword ea = connection.Table<FromPassword>().Last();
                    lblOut.Text = $"OK pw=> {ea.ToString()}";
                }
                if (!string.IsNullOrWhiteSpace(txtRate.Text))
                {
                    connection.Insert(new HourlyRate()
                    {
                        Date = dt,
                        IsValid = true,
                        Rate = (float)Convert.ToDouble(txtRate.Text),
                    });
                    HourlyRate ea = connection.Table<HourlyRate>().Last();
                    lblOut.Text = $"OK Hourlyrate=> {ea.ToString()}";
                }
                if (!string.IsNullOrWhiteSpace(txtServer.Text))
                {
                    connection.Insert(new ServerOut()
                    {
                        Date = dt,
                        IsValid = true,
                        Server = txtServer.Text,
                    });
                    ServerOut ea = connection.Table<ServerOut>().Last();
                    lblOut.Text = $"OK Server=> {ea.ToString()}";
                }
                else if (connection.Table<ServerOut>().Count() == 0)
                {
                    connection.Insert(new ServerOut()
                    {
                        Date = dt,
                        IsValid = true,
                        Server = "smtp.office365.com",
                    });
                    ServerOut ea = connection.Table<ServerOut>().Last();
                    lblOut.Text = $"OK Server=> {ea.ToString()}";
                }
            }
        }


    }
}