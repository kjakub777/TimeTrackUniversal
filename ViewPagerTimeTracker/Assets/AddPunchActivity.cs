using Android.App;
using Android.OS;

using Android.Widget;
using SQLite;
using System;
using System.Linq;
using TimeTrackerUniversal.Database;
using TimeTrackerUniversal.Database.Schema;

namespace TimeTrackerUniversal
{
    [Activity(Label = "Add Punch", MainLauncher = false, Icon = "@drawable/icon")]
    public class AddPunchActivity : Activity
    {
        Button btnExitPass;
        Button btnPunch; 
        EditText dateDate;
        CheckBox IsClockIn;
        CheckBox chkForce;
        CheckBox IsClockOut;
        TextView punchOut;
        TimePicker timePickIn;
        TimePicker timePickOut;


        public AddPunchActivity()
        {


        }

        private void btnExitPass_Click(object sender, EventArgs e)
        {
            SetResult(Result.Ok);
            this.OnBackPressed();
        }

        private void btnPunch_Click(object sender, EventArgs e)
        {
            punchOut.Text = $"Clocking...";
            DateTime date;
            try
            {
                using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                {
                    if (DateTime.TryParse(dateDate.Text.ToString(), out date) && (IsClockIn.Checked || IsClockOut.Checked))
                    {
                        //makesure valid punch
                        WorkInstance wi = connection.Table<WorkInstance>().Last();
                        if (IsClockIn.Checked)
                        {

                            if ((wi.ClockIn != wi.ClockOut) || chkForce.Checked)
                            {
                                //System.Globalization.Calendar.CurrentEra;                                                                    //String.Format("{0:HH:mm:ss}", wi.ClockIn)
                                DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, timePickIn.Hour, timePickIn.Minute, 0, 0, DateTimeKind.Local);

                                DateTime timein = MainActivity.GetLocalTime(dateTime);
                                dateTime = new DateTime(date.Year, date.Month, date.Day, timePickOut.Hour, timePickOut.Minute, 0, 0, DateTimeKind.Local);
                                DateTime timeout = MainActivity.GetLocalTime(dateTime);
                                var hourlyRate = (connection.Table<HourlyRate>().Last()).Rate;
                                connection.Insert(new WorkInstance()
                                {
                                    Date = date,
                                    IsValid = true,
                                    ClockIn = timein,
                                    ClockOut = IsClockOut.Checked ? timeout : timein,
                                    HourlyRate = hourlyRate,
                                });
                                punchOut.Text = $"You are clocked!";
                            }
                            else
                            {
                                punchOut.Text = $"You still need to clock out from last clock in {wi.ClockIn}";
                            }
                        }
                        else if (IsClockOut.Checked)
                        {
                            if (wi.ClockIn == wi.ClockOut || chkForce.Checked)
                            {
                                DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, timePickOut.Hour, timePickOut.Minute, 0, 0, DateTimeKind.Local);
                                DateTime timeout = MainActivity.GetLocalTime(dateTime);

                                wi.ClockOut = timeout;
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AddPunch);

            btnExitPass = FindViewById<Button>(Resource.Id.btnExitPass);
            btnPunch = FindViewById<Button>(Resource.Id.btnPunch);
            dateDate = FindViewById<EditText>(Resource.Id.dateDate);
            punchOut = FindViewById<TextView>(Resource.Id.punchOut);
            timePickIn = FindViewById<TimePicker>(Resource.Id.timePickIn);
            timePickOut = FindViewById<TimePicker>(Resource.Id.timePickOut);
            IsClockIn = FindViewById<CheckBox>(Resource.Id.IsClockIn);
            chkForce = FindViewById<CheckBox>(Resource.Id.chkForce);
            IsClockOut = FindViewById<CheckBox>(Resource.Id.IsClockOut);

            DateTime dt = MainActivity.GetLocalTime();//.Month
            dateDate.Text = $"{dt.Month}/{dt.Day}/{dt.Year}";
            Java.Lang.Boolean b = Java.Lang.Boolean.True;
            //timePickIn.SetIs24HourView(b);
            //timePickOut.SetIs24HourView(b);
            btnPunch.Click += btnPunch_Click;
            btnExitPass.Click += btnExitPass_Click;

        }


    }
}