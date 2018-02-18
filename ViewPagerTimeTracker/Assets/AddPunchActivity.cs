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
        DateTime? dateTimeIn;
        DateTime? dateTimeOut;
        Dialog dlgFinding;
        DatePicker datePicker;
        TimePicker timePicker;
        TextView txtOut;
        Button btnExit;
        Button btnExitPass;
        Button btnPunch;
        CheckBox IsClockIn;
        CheckBox chkForce;
        CheckBox IsClockOut;
        TextView punchOut;


        public AddPunchActivity()
        {


        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.AddPunch);

            btnExitPass = FindViewById<Button>(Resource.Id.btnExitPass);
            btnPunch = FindViewById<Button>(Resource.Id.btnPunch);
            punchOut = FindViewById<TextView>(Resource.Id.punchOut);
            IsClockIn = FindViewById<CheckBox>(Resource.Id.IsClockIn);
            chkForce = FindViewById<CheckBox>(Resource.Id.chkForce);
            IsClockOut = FindViewById<CheckBox>(Resource.Id.IsClockOut);

            DateTime dt = MainActivity.GetLocalTime();//.Month 
            Java.Lang.Boolean b = Java.Lang.Boolean.True;
            //timePickIn.SetIs24HourView(b);
            //timePickOut.SetIs24HourView(b);
            btnPunch.Click += btnPunch_Click;
            btnExitPass.Click += btnExitPass_Click;

        }


        private void btnExitPass_Click(object sender, EventArgs e)
        {
            SetResult(Result.Ok);
            this.OnBackPressed();
        }

        private void DoPunch()
        {
            punchOut.Text = $"Clocking...";
            try
            {
                using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                {

                    //makesure valid punch
                    WorkInstance wi = connection.Table<WorkInstance>().Last();
                    if (inOutBoth123 == 1 || inOutBoth123 == 3)
                    {

                        if ((wi.ClockIn != wi.ClockOut) || chkForce.Checked)
                        {
                            //System.Globalization.Calendar.CurrentEra;                                                                    //String.Format("{0:HH:mm:ss}", wi.ClockIn)
                            //    DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, timePickIn.Hour, timePickIn.Minute, 0, 0, DateTimeKind.Local);

                            DateTime timein = MainActivity.GetLocalTime(dateTimeIn.Value);
                            //    dateTime = new DateTime(date.Year, date.Month, date.Day, timePickOut.Hour, timePickOut.Minute, 0, 0, DateTimeKind.Local);
                            DateTime timeout = dateTimeOut == null ? MainActivity.GetLocalTime() : MainActivity.GetLocalTime(dateTimeOut.Value);
                            var hourlyRate = (connection.Table<HourlyRate>().Last()).Rate;
                            connection.Insert(new WorkInstance()
                            {
                                Date = dateTimeIn.Value.Date,
                                IsValid = true,
                                ClockIn = timein,
                                ClockOut = inOutBoth123 == 3 ? timeout : timein,
                                HourlyRate = hourlyRate,
                            });
                            punchOut.Text = $"You are clocked!";
                        }
                        else
                        {
                            punchOut.Text = $"You still need to clock out from last clock in {wi.ClockIn}";
                        }
                    }
                    else if (inOutBoth123 == 2)
                    {
                        if (wi.ClockIn == wi.ClockOut || chkForce.Checked)
                        {
                            //  DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, timePickOut.Hour, timePickOut.Minute, 0, 0, DateTimeKind.Local);
                            DateTime timeout = MainActivity.GetLocalTime(dateTimeOut.Value);

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
            catch (Exception Ex)
            {

                punchOut.Text = Ex.Message;
            }
            punchOut.Text += "  ||";
            dateTimeIn = dateTimeOut = null;
        }

        private void Dialog_btnExit_Click_Out(object sender, EventArgs e)
        {
            dateTimeIn = new DateTime(datePicker.Year, datePicker.Month + 1, datePicker.DayOfMonth, timePicker.Hour, timePicker.Minute, 0, 0, DateTimeKind.Local);
            dlgFinding.OnBackPressed();

            GetClockOutDateTime(true);

        }
        int inOutBoth123 = 1;

        private void Dialog_btnExit_Click(object sender, EventArgs e)
        {
            try
            {
                if (inOutBoth123 != 1)
                {
                    dateTimeOut = new DateTime(datePicker.Year, datePicker.Month + 1, datePicker.DayOfMonth, timePicker.Hour, timePicker.Minute, 0, 0, DateTimeKind.Local);
                    dlgFinding.OnBackPressed();
                }
                else
                {
                    dateTimeIn = new DateTime(datePicker.Year, datePicker.Month + 1, datePicker.DayOfMonth, timePicker.Hour, timePicker.Minute, 0, 0, DateTimeKind.Local);
                    dlgFinding.OnBackPressed();
                }
                DoPunch();
            }
            catch (Exception ex)
            {
                Android.Widget.Toast.MakeText(this, $"EX {ex} ", Android.Widget.ToastLength.Short).Show();
                punchOut.Text = $"Clock in failed, {ex}";
            }
        }
        private void btnPunch_Click(object sender, EventArgs e)
        {
            if (IsClockIn.Checked)
            {
                GetClockInDateTime(IsClockOut.Checked);
            }

            else if (IsClockOut.Checked)
            {
                GetClockOutDateTime(false);
            }


            ////////*/*********************************************
        }
        private void GetClockOutDateTime(bool clockin)
        {
            dlgFinding = new Dialog(this);
            dlgFinding.SetContentView(Resource.Layout.PickDate);
            //@+id/txtOid            //@+id/txtDate            //@+id/txtClockIn            //@+id/txtClockOut            //@+id/txtRate            //@+id/txtHours            //@+id/btnSave            //@+id/btnOk            //spinner

            datePicker = dlgFinding.FindViewById<DatePicker>(Resource.Id.datePicker);
            timePicker = dlgFinding.FindViewById<TimePicker>(Resource.Id.timePicker);
            txtOut = dlgFinding.FindViewById<TextView>(Resource.Id.txtOut);
            btnExit = dlgFinding.FindViewById<Button>(Resource.Id.btnExit);
            dlgFinding.SetTitle("Pick ClockOut Date/Time");
            btnExit.Click += Dialog_btnExit_Click;

            inOutBoth123 = clockin ? 3 : 2;

            dlgFinding.Show();
        }

        private void GetClockInDateTime(bool clockOut)
        {
            dlgFinding = new Dialog(this);
            dlgFinding.SetContentView(Resource.Layout.PickDate);
            //@+id/txtOid            //@+id/txtDate            //@+id/txtClockIn            //@+id/txtClockOut            //@+id/txtRate            //@+id/txtHours            //@+id/btnSave            //@+id/btnOk            //spinner

            datePicker = dlgFinding.FindViewById<DatePicker>(Resource.Id.datePicker);
            timePicker = dlgFinding.FindViewById<TimePicker>(Resource.Id.timePicker);
            txtOut = dlgFinding.FindViewById<TextView>(Resource.Id.txtOut);
            btnExit = dlgFinding.FindViewById<Button>(Resource.Id.btnExit);
            dlgFinding.SetTitle("Pick ClockIn Date/Time");

            if (clockOut)
            {
                inOutBoth123 = 3;
                btnExit.Click += Dialog_btnExit_Click_Out;
            }

            else
            {
                inOutBoth123 = 1;
                btnExit.Click += Dialog_btnExit_Click;
            }

            dlgFinding.Show();
        }



        //private void btnPunch_Click(object sender, EventArgs e)
        //{
        //    punchOut.Text = $"Clocking...";
        //    DateTime date;
        //    try
        //    {
        //        using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
        //        {
        //            if (DateTime.TryParse(dateDate.Text.ToString(), out date) && (IsClockIn.Checked || IsClockOut.Checked))
        //            {
        //                //makesure valid punch
        //                WorkInstance wi = connection.Table<WorkInstance>().Last();
        //                if (IsClockIn.Checked)
        //                {

        //                    if ((wi.ClockIn != wi.ClockOut) || chkForce.Checked)
        //                    {
        //                        //System.Globalization.Calendar.CurrentEra;                                                                    //String.Format("{0:HH:mm:ss}", wi.ClockIn)
        //                        DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, timePickIn.Hour, timePickIn.Minute, 0, 0, DateTimeKind.Local);

        //                        DateTime timein = MainActivity.GetLocalTime(dateTime);
        //                        dateTime = new DateTime(date.Year, date.Month, date.Day, timePickOut.Hour, timePickOut.Minute, 0, 0, DateTimeKind.Local);
        //                        DateTime timeout = MainActivity.GetLocalTime(dateTime);
        //                        var hourlyRate = (connection.Table<HourlyRate>().Last()).Rate;
        //                        connection.Insert(new WorkInstance()
        //                        {
        //                            Date = date,
        //                            IsValid = true,
        //                            ClockIn = timein,
        //                            ClockOut = IsClockOut.Checked ? timeout : timein,
        //                            HourlyRate = hourlyRate,
        //                        });
        //                        punchOut.Text = $"You are clocked!";
        //                    }
        //                    else
        //                    {
        //                        punchOut.Text = $"You still need to clock out from last clock in {wi.ClockIn}";
        //                    }
        //                }
        //                else if (IsClockOut.Checked)
        //                {
        //                    if (wi.ClockIn == wi.ClockOut || chkForce.Checked)
        //                    {
        //                        DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, timePickOut.Hour, timePickOut.Minute, 0, 0, DateTimeKind.Local);
        //                        DateTime timeout = MainActivity.GetLocalTime(dateTime);

        //                        wi.ClockOut = timeout;
        //                        connection.Update(wi);
        //                        punchOut.Text = $"You are clocked!";
        //                    }
        //                    else
        //                    {
        //                        punchOut.Text = $"You are not clocked in";
        //                    }
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception Ex)
        //    {

        //        punchOut.Text = Ex.Message;
        //    }
        //    punchOut.Text += "  ||";
        //}



    }
}