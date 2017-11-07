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

namespace TimeTrackerUniversal
{
	[Activity(Label = "Change Emails", MainLauncher = false, Icon = "@drawable/icon")]
	public class ChangeEmailsActivity : Activity
	{
		TextView lblPass;
		EditText pass1;
		TextView lblOut;
		Button btnOk;
		TextView lblEmail;
		EditText txtToNewEmail;
        EditText txtFromNewEmail;
        EditText txtBCCNewEmail;
        EditText txtRate;
        TextView lblOutEmail;
		Button btnExitPass;


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
            lblOut = FindViewById<TextView>(Resource.Id.lblOut);
			btnOk = FindViewById<Button>(Resource.Id.btnOk);
			lblEmail = FindViewById<TextView>(Resource.Id.lblEmail);
			txtToNewEmail = FindViewById<EditText>(Resource.Id.txtToNewEmail);
            txtFromNewEmail = FindViewById<EditText>(Resource.Id.txtFromNewEmail);
            txtBCCNewEmail = FindViewById<EditText>(Resource.Id.txtBCCNewEmail);
            lblOutEmail = FindViewById<TextView>(Resource.Id.lblOutEmail);
			btnExitPass = FindViewById<Button>(Resource.Id.btnExitPass);
		
			DateTime dt = DateTime.Now.ToLocalTime();//.Month
			Java.Lang.Boolean b = Java.Lang.Boolean.True;
			btnOk.Click += btnOk_Click;
			btnExitPass.Click += btnExitPass_Click;
			
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
                    lblOutEmail.Text += $"OK => {newto.ToString()}\n";
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
                    lblOutEmail.Text += $"OK => {newfrom.ToString()}\n";
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
                    lblOutEmail.Text += $"OK => {newbcc.ToString()}\n";
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
                    lblOut.Text = $"OK => {ea.ToString()}";
                }
                if (!string.IsNullOrWhiteSpace(txtRate.Text))
                {
                    connection.Insert(new HourlyRate()
                    {
                        Date = dt,
                        IsValid = true,
                        Rate = (float)Convert.ToDouble(txtRate.Text),
                    });
                    FromPassword ea = connection.Table<FromPassword>().Last();
                    lblOut.Text = $"OK => {ea.ToString()}";
                }
            }
        }


	}
}