using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using Android.Util;
using SQLite;
using TimeTrackerUniversal.Database;
using TimeTrackerUniversal.Database.Schema;
using System.Linq;
using System.IO;

namespace TimeTrackerUniversal
{
    [Activity(Label = "TimeTrackerUniversal", MainLauncher = true, Icon = "@drawable/ttu")]
    public class MainActivity : Activity
    {

        static readonly object _syncLock = new object();
        public static string DB_PATH = string.Empty;

        Button btnChangeEmails;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnAddPunch;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnEditPunch;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnSqlQuery;// button
        Button btnExit;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnViewHistory;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnClockIn;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnClockOut;// = FindViewById<Button>(Resource.Id.btnClockIn);
        TextView txtCurrentPayRate;// = FindViewById<TextView>(Resource.Id.txtCurrentPayRate);
        TextView txtLastPunch;// = FindViewById<TextView>(Resource.Id.txtTimeClockOut);
        TextView txtMainOut;// = FindViewById<TextView>(Resource.Id.txtCurrentPayRate);

        Switch ForReal;

        bool RealClockPunch = false;
        TextView txtMonthTotalHours;// = FindViewById<TextView>(Resource.Id.txtCurrentPayRate);
        TextView txtWeekTotalHours;// = FindViewById<TextView>(Resource.Id.txtCurrentPayRate);
        TextView txtGrossPay;// = FindViewById<TextView>(Resource.Id.txtTimeClockOut);



        Button btnClearDB;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnDayPicker;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnImportWorkInstances;// = FindViewById<Button>(Resource.Id.btnClockIn);
                                      //Button btnDayPicker;// = FindViewById<Button>(Resource.Id.btnDayPicker);
        Button btnMonthPicker;// = FindViewById<Button>(Resource.Id.btnMonthPicker);
                              //Button btnFromDate;// = FindViewById<Button>(Resource.Id.btnFromDate);
        Button btnToDate;// = FindViewById<Button>(Resource.Id.btnToDate);	   	   
        private bool ClockInOut_Success = false;

        EditText dateTxtMonth;
        EditText dateTxtYear;
        private bool Imported = false;
        private bool OUT = false;
        private string output = string.Empty;
        Switch switchUpdatePayRate;
        TextView txtImportOutput;// = FindViewById<TextView>(Resource.Id.txtCurrentPayRate);

        private static float initRate;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            txtLastPunch = FindViewById<TextView>(Resource.Id.txtLastPunch);
            txtWeekTotalHours = FindViewById<TextView>(Resource.Id.txtWeekTotalHours);
            txtMainOut = FindViewById<TextView>(Resource.Id.txtMainOut);
            txtGrossPay = FindViewById<TextView>(Resource.Id.txtGrossPay);
            txtMonthTotalHours = FindViewById<TextView>(Resource.Id.txtMonthTotalHours);
            btnEditPunch = FindViewById<Button>(Resource.Id.btnEditPunch);
            btnExit = FindViewById<Button>(Resource.Id.btnExit);
            btnEditPunch = FindViewById<Button>(Resource.Id.btnEditPunch);
            btnViewHistory = FindViewById<Button>(Resource.Id.btnViewHistory);
            btnClockIn = FindViewById<Button>(Resource.Id.btnClockIn);
            btnClockOut = FindViewById<Button>(Resource.Id.btnClockOut);
            ForReal = FindViewById<Switch>(Resource.Id.ForReal);
            btnSqlQuery = FindViewById<Button>(Resource.Id.btnSqlQuery);


            btnChangeEmails = FindViewById<Button>(Resource.Id.btnChangeEmails);
            btnExit = FindViewById<Button>(Resource.Id.btnExit);
            btnAddPunch = FindViewById<Button>(Resource.Id.btnAddPunch);


            using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnection())
            {
                Log.Debug("DATABASE", connection.ExecuteScalar<string>("PRAGMA database_list"));

                int c = connection.CreateTable<HourlyRate>(CreateFlags.AutoIncPK);


                c = connection.CreateTable<WorkInstance>(SQLite.CreateFlags.AutoIncPK);

                c = connection.CreateTable<FromPassword>(SQLite.CreateFlags.AutoIncPK);
                c = connection.CreateTable<EmailAddresses>(SQLite.CreateFlags.AutoIncPK);

                //if (!connection.Table<FromPassword>().Any())
                //{
                //    DateTime dt = MainActivity.GetLocalTime(); ;
                //    connection.Insert(new FromPassword()
                //    //connection.Insert(new HourlyRate()
                //    {
                //        Date = dt,
                //        IsValid = true,
                //        Pass = GetString(Resource.String.pass)
                //    });
                //}

                // connection.
            }
            try
            {
                using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
                {
                    HourlyRate hr = connection.Table<HourlyRate>().Last();
                    if (hr != null)
                    {
                        txtCurrentPayRate.Text = string.Empty + hr.Rate;
                        MainActivity.initRate = hr.Rate;
                    }
                    else
                    {
                        txtCurrentPayRate.Text = string.Empty + Convert.ToDouble("0");
                        MainActivity.initRate = (float)Convert.ToDouble(GetString(Resource.String.initialPay));
                    }
                    WorkInstance v = connection.Table<WorkInstance>().Last();
                    txtLastPunch.Text = v != null ? v.ClockIn == v.ClockOut ? $"In {v.ClockIn.ToString()}" : $"Out {v.ClockOut.ToString()}" : "NULL";

                }
            }
            catch (Exception Ex)
            {
                txtMainOut.Text = $"ERR {Ex.Message}";
            }
            //dateTxtMonth.Text = MainActivity.GetLocalTime().Month.ToString();
            //dateTxtYear.Text = MainActivity.GetLocalTime().Year.ToString();

            btnClockIn.Click += ButtonClockedInOut_Handler;// delegate { btnClockIn_Clicked(); };
            btnClockOut.Click += ButtonClockedInOut_Handler;// delegate { btnClockIn_Clicked(); };
            ForReal.CheckedChange += ForReal_CheckedChanged;
            btnExit.Click += ExitDoer;
            btnEditPunch.Click += btnEditPunch_Click;// = FindViewById<Button>(Resource.Id.btnClockIn);
            btnSqlQuery.Click += btnSqlQuery_Click;// = FindViewById<Button>(Resource.Id.btnClockIn);
            btnViewHistory.Click += btnViewHistory_Click;// = FindViewById<Button>(Resource.Id.btnClockIn);
                                                         // = FindViewById<Button>(Resource.Id.btnClockIn);
                                                         // = FindViewById<Button>(Resource.Id.btnClockIn);

            btnChangeEmails.Click += BtnChangeEmails_Clicked;

            RealClockPunch = ForReal.Checked;

        }

        private void btnEditPunch_Click(object sender, EventArgs e)
        {
            try
            {
                lock (_syncLock)
                {
                    Intent intent = new Intent(this.ApplicationContext, typeof(ChangeSettingActivity));
                    intent.SetFlags(ActivityFlags.ForwardResult);
                    intent.SetFlags(ActivityFlags.ReceiverForeground);
                    StartActivity(intent);
                }
            }
            catch (Exception exx)
            {
                Log.Error("EXCEPTION", exx.Message + "\t      " + exx.StackTrace);
            }
        }

        private void btnSqlQuery_Click(object sender, EventArgs e)
        {
            try
            {
                lock (_syncLock)
                {
                    Intent intent = new Intent(this.ApplicationContext, typeof(RunSQLActivity));
                    intent.SetFlags(ActivityFlags.ForwardResult);
                    intent.SetFlags(ActivityFlags.ReceiverForeground);
                    StartActivity(intent);
                }
            }
            catch (Exception exx)
            {
                Log.Error("EXCEPTION", exx.Message + "\t      " + exx.StackTrace);
            }
        }

        private void btnViewHistory_Click(object sender, EventArgs e)
        {
            try
            {
                lock (_syncLock)
                {
                    Intent intent = new Intent(this.ApplicationContext, typeof(ViewHistoryActivity));
                    intent.SetFlags(ActivityFlags.ForwardResult);
                    intent.SetFlags(ActivityFlags.ReceiverForeground);
                    StartActivity(intent);
                }
            }
            catch (Exception exx)
            {
                Log.Error("EXCEPTION", exx.Message + "\t      " + exx.StackTrace);
            }
        }

        private void BtnChangeEmails_Clicked(object sender, EventArgs e)
        {
            try
            {
                lock (_syncLock)
                {
                    Intent intent = new Intent(this.ApplicationContext, typeof(ChangeEmailsActivity));
                    intent.SetFlags(ActivityFlags.ForwardResult);
                    intent.SetFlags(ActivityFlags.ReceiverForeground);
                    StartActivity(intent);
                }
            }
            catch (Exception exx)
            {
                Log.Error("EXCEPTION", exx.Message + "\t      " + exx.StackTrace);
            }

        }
        public void ButtonClockedInOut_Handler(object sender, EventArgs e)
        {
            output = string.Empty;
            //if (!ClockInOut_Success)
            //{
            Button ib = ((Button)sender);

            int start = System.Environment.TickCount & Int32.MaxValue;
            SetOutput("Punch attempted..");
            OnClockPunchButtonsPushed(ib, e);

            AfterEmailSent();
            output += "  Time to run " + ((System.Environment.TickCount & Int32.MaxValue) - start);
            SetOutput(output);
            //}
            //else
            //    SetOutput("You already clocked!! ");
        }
        protected bool AfterEmailSent()
        {
            using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
            {
                if (ClockInOut_Success)
                {
                    string fileName = "/sdcard/punches/ClockPunches.txt";
                    string dirName = "/sdcard/punches";
                    if (!Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }
                    if (!File.Exists(fileName))
                    {
                        File.Create(fileName);
                    }
                    string stamp = DateTime.Now.ToString() + "," + (OUT ? "OUT" : "IN") + "," + (RealClockPunch ? "REAL," + (connection.Table<EmailAddresses>().Where(x => x.EmailType == 1).Last()).Email : "FAKE," + (connection.Table<EmailAddresses>().Where(x => x.EmailType == 2).Last()).Email) + "\n";
                    File.AppendAllText(fileName, stamp);
                    if (RealClockPunch)
                    {
                        if (!OUT)
                        {//in

                            float hourlyRate = (connection.Table<HourlyRate>().Last()).Rate;
                            connection.Insert(new WorkInstance()
                            {
                                IsValid = true,
                                Date = MainActivity.GetLocalTime().Date,
                                HourlyRate = hourlyRate,
                                ClockIn = MainActivity.GetLocalTime()
                            });
                        }
                        else
                        {
                            WorkInstance wi = connection.Table<WorkInstance>().Last();
                            wi.ClockOut = MainActivity.GetLocalTime();
                            connection.Update(wi);
                        }
                    }
                }
            }
            return true;
        }

        public void OnClockPunchButtonsPushed(Button ib, EventArgs e)
        {
            using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
            {
                WorkInstance wi=null;
                OUT = Resource.Id.btnClockOut == ib.Id;
                try
                {
                      wi = connection.Table<WorkInstance>().Last();
                }
                catch (Exception )
                { 

                }
                if (wi != null)
                {
                    if (OUT)
                    {
                        if ((wi.ClockOut != wi.ClockIn))
                        {
                            ClockInOut_Success = false;
                            output += "Need To CLOCK IN first";
                            SetOutput(output);
                            return;
                        }
                        ClockInOut_Success = EmailTaskMethod();
                    }
                    else
                    {
                        if ((wi.ClockOut == wi.ClockIn))
                        {
                            ClockInOut_Success = false;
                            output += "Need To CLOCK OUT first";
                            SetOutput(output);
                            return;
                        }
                        ClockInOut_Success = EmailTaskMethod();
                    }
                }
                else
                {
                    ClockInOut_Success = EmailTaskMethod();
                }
            }
            if (ClockInOut_Success)
            {
                output += "   seemed successful..";
            }
            else
            {
                output += "   something broke..";
            }
        }
        private bool EmailTaskMethod()
        {
            try
            {
                if (!OUT)
                {
                    Log.Debug("PunchClock.PunchClock", "IN");
                    return (EmailHelper.SendEmail(string.Empty, OUT, RealClockPunch, ref output, FindViewById, GetString));
                }
                else
                {
                    Log.Debug("PunchClock.PunchClock", "OUT");
                    return (EmailHelper.SendEmail(string.Empty, OUT, RealClockPunch, ref output, FindViewById, GetString));
                }
            }
            catch (Exception ex)
            {
                output += "Error: " + ex.Message;
                return (false);
            }
        }
        private void SetOutput(string o)
        {
            lock (_syncLock)
            {
                txtMainOut.Text = o;
            }
        }
        private void ExitDoer(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void ForReal_CheckedChanged(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            RealClockPunch = ForReal.Checked;
            Log.Error("Hourly", RealClockPunch ? "TRUE" : "FALSE");
        }
        public static DateTime GetLocalTime()
        {
            TimeSpan ts;
            DateTime dt = DateTime.Now.ToUniversalTime();
            if (DateTime.Now.IsDaylightSavingTime())
            {
                ts = new TimeSpan(TimeSpan.TicksPerHour * 7);
            }
            else
            {
                ts = new TimeSpan(TimeSpan.TicksPerHour * 6);
            }

            dt = dt.Subtract(ts);
            return dt;
        }
        public static DateTime GetLocalTime(ref DateTime dt)
        {
            TimeSpan ts;
            if (dt.IsDaylightSavingTime())
            {
                ts = new TimeSpan(TimeSpan.TicksPerHour * 7);
            }
            else
            {
                ts = new TimeSpan(TimeSpan.TicksPerHour * 6);
            }

            dt = dt.Subtract(ts);
            return dt;
        }
    }
}

