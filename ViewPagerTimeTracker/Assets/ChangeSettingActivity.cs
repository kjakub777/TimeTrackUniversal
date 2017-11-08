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
	[Activity(Label = "Change Email Pass", MainLauncher = false, Icon = "@drawable/icon")]
	public class ChangeSettingActivity : Activity
	{
		TextView lblPass;
		EditText pass1;
		TextView lblOut;
		Button btnOk;
		TextView lblEmail;
		EditText txtNewEmail;
		EditText sqlQuery;
		EditText dateDate;
		TextView lblOutEmail;
		TextView punchOut;
		Button btnOkEmail;
		Button btnExitPass;
		Button btnPunch;
		Button btnExecuteSql;//sqlQuery btnExecuteSql
		TimePicker timePickIn;
		TimePicker timePickOut;
		CheckBox IsClockIn;
		CheckBox IsClockOut;


		public ChangeSettingActivity()
		{


		}
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.ChangeSettings);

			lblPass = FindViewById<TextView>(Resource.Id.lblPass);
			btnExecuteSql = FindViewById<Button>(Resource.Id.btnExecuteSql);
			sqlQuery = FindViewById<EditText>(Resource.Id.sqlQuery);
			pass1 = FindViewById<EditText>(Resource.Id.pass1);
			dateDate = FindViewById<EditText>(Resource.Id.dateDate);
			lblOut = FindViewById<TextView>(Resource.Id.lblOut);
			btnOk = FindViewById<Button>(Resource.Id.btnOk);
			lblEmail = FindViewById<TextView>(Resource.Id.lblEmail);
			punchOut = FindViewById<TextView>(Resource.Id.punchOut);
			txtNewEmail = FindViewById<EditText>(Resource.Id.txtNewEmail);
			lblOutEmail = FindViewById<TextView>(Resource.Id.lblOutEmail);
			btnOkEmail = FindViewById<Button>(Resource.Id.btnOkEmail);
			btnExitPass = FindViewById<Button>(Resource.Id.btnExitPass);
			btnPunch = FindViewById<Button>(Resource.Id.btnPunch);
			timePickIn = FindViewById<TimePicker>(Resource.Id.timePickIn);
			timePickOut = FindViewById<TimePicker>(Resource.Id.timePickOut);
			IsClockIn = FindViewById<CheckBox>(Resource.Id.IsClockIn);
			IsClockOut = FindViewById<CheckBox>(Resource.Id.IsClockOut);

			DateTime dt = DateTime.Now.ToLocalTime();//.Month
			dateDate.Text = $"{dt.Month}/{dt.Day}/{dt.Year}";
			Java.Lang.Boolean b = Java.Lang.Boolean.True;
			timePickIn.SetIs24HourView(b);
			timePickOut.SetIs24HourView(b);
			btnPunch.Click += btnPunch_Click;
			btnOk.Click += btnOk_Click;
			//btnOkEmail.Click += btnOkEmail_Click;
			btnExitPass.Click += btnExitPass_Click;
			btnExecuteSql.Click += btnExecuteSql_Click;

		}

		private void btnExecuteSql_Click(object sender, EventArgs e)
		{
		 
				using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
			{						   
				int res = connection.Execute(sqlQuery.Text);
				sqlQuery.Text += " || > " + res;
			}
		}

		private void btnPunch_Click(object sender, EventArgs e)
		{
			punchOut.Text = $"Clocking...";
			DateTime date;
			try
			{
				using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
				{
					if (DateTime.TryParse(dateDate.Text.ToString(), out date) && (IsClockIn.Checked || IsClockOut.Checked))
					{
						//makesure valid punch
						WorkInstance wi = connection.Table<WorkInstance>().Last();
						if (IsClockIn.Checked)
						{

							if (wi.ClockIn != wi.ClockOut)
							{
								DateTime timein = new DateTime(date.Year, date.Month, date.Day, timePickIn.Hour, timePickIn.Minute, 0);
								DateTime timeout = new DateTime(date.Year, date.Month, date.Day, timePickOut.Hour, timePickOut.Minute, 0);
								var hourlyRate = (connection.Table<HourlyRate>().Last()).Rate;
								connection.Insert(new WorkInstance()
								{
									Date = date,
									IsValid = true,
									ClockIn = MainActivity.GetLocalTime(  timein),
									ClockOut = IsClockOut.Checked ? MainActivity.GetLocalTime(  timeout) : MainActivity.GetLocalTime(timein),
									HourlyRate = hourlyRate,
								});
								punchOut.Text = $"You are clocked!";
							}
							else
							{
								punchOut.Text = $"You still need to clock out from last clock in {wi.ClockIn.ToLongTimeString()}";
							}
						}
						else if (IsClockOut.Checked)
						{
							if (wi.ClockIn == wi.ClockOut)
							{
								DateTime timeout = new DateTime(date.Year, date.Month, date.Day, timePickOut.Hour, timePickOut.Minute, 0);
								wi.ClockOut = MainActivity.GetLocalTime(  timeout);
								connection.Update(wi);
								punchOut.Text = $"You are clocked!";
							}
							else
							{
								punchOut.Text = $"You are not clocked in";
							}
						}

					}
				}
			}
			catch (Exception Ex)
			{

				punchOut.Text = Ex.Message;
			}
			punchOut.Text += "  ||";
		}

		private void btnExitPass_Click(object sender, EventArgs e)
		{
			SetResult(Result.Ok);
			this.OnBackPressed();
		}

		//private void btnOkEmail_Click(object sender, EventArgs e)
		//{
		//	using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
		//	{
		//		DateTime dt = MainActivity.GetLocalTime();
		//		connection.Insert(new EmailAddresses()
		//		{
		//			Date = dt,
		//			IsValid = true,
		//			EmailType = true,
		//			Email = txtNewEmail.Text
		//		});
		//		EmailAddresses ea = connection.Table<EmailAddresses>().Last();
		//		lblOutEmail.Text = $"OK => {ea.ToString()}";
		//	}
		//}

		private void btnOk_Click(object sender, EventArgs e)
		{
			using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
			{
				DateTime dt = MainActivity.GetLocalTime();
				connection.Insert(new FromPassword()
				{
					Date = dt,
					IsValid = true,
					Pass = pass1.Text,
				});
				FromPassword ea = connection.Table<FromPassword>().Last();
				lblOut.Text = $"OK => {ea.ToString()}";
			}
		}


	}
}