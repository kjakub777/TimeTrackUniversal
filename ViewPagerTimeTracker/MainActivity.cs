using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using SQLite;
using System;
using System.IO;
using System.Linq;
using TimeTrackerUniversal.Assets;
using TimeTrackerUniversal.Database;
using TimeTrackerUniversal.Database.Schema;

namespace TimeTrackerUniversal
{
    [Activity(Label = "TimeTrackerUniversal", MainLauncher = true, Icon = "@drawable/ttu")]
    public class MainActivity : Activity
    {

        static readonly object _syncLock = new object();
        private static float initRate;
        public static string DB_PATH = string.Empty;
        Button btnAddPunch;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnChangeEmails;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnClockIn;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnClockOut;// = FindViewById<Button>(Resource.Id.btnClockIn); 
        Button btnEditPunch;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnExit;// = FindViewById<Button>(Resource.Id.btnClockIn);
        Button btnSqlQuery;// button
        Button btnViewHistory;// = FindViewById<Button>(Resource.Id.btnClockIn);



        private bool ClockInOut_Success = false;

        Switch ForReal;
        bool fresh = true;

        private bool OUT = false;

        bool RealClockPunch = false;
        private DateTime TimeIntervalBegin;
        private DateTime TimeIntervalEnd;
        TextView txtGrossPay;// = FindViewById<TextView>(Resource.Id.txtTimeClockOut);
        TextView txtLastPunch;// = FindViewById<TextView>(Resource.Id.txtTimeClockOut);
        TextView txtMainOut;// = FindViewById<TextView>(Resource.Id.txtCurrentPayRate);
        TextView txtMonthTotalHours;// = FindViewById<TextView>(Resource.Id.txtCurrentPayRate);
        TextView txtWeekTotalHours;// = FindViewById<TextView>(Resource.Id.txtCurrentPayRate);
        public string output = "..";

        private void btnAddPunch_Click(object sender, EventArgs e)
        {
            try
            {
                lock (_syncLock)
                {
                    Intent intent = new Intent(this.ApplicationContext, typeof(AddPunchActivity));
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

        private void btnEditPunch_Click(object sender, EventArgs e)
        {
            Toast.MakeText(ApplicationContext, "Need to Implement!!", ToastLength.Long).Show();
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
        private bool EmailTaskMethod()
        {
            try
            {
                output += EmailHelper.SendEmail(string.Empty, OUT, RealClockPunch, GetString);
                return (!output.Contains("ERROR"));
            }
            catch (Exception ex)
            {
                output += "Error: " + ex.Message;
                return (false);
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
        private void SetOutput(string o)
        {
            lock (_syncLock)
            {
                txtMainOut.Text = o;
            }
        }

        protected bool AfterEmailSent()
        {
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            //backup db
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            path = Path.Combine(path, SqlConnectionFactory.fileName);
            string backupDir = "/sdcard/";
            if (fresh)
            {
                if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
                if (!File.Exists(Path.Combine(backupDir, SqlConnectionFactory.fileName))) File.Create(Path.Combine(backupDir, SqlConnectionFactory.fileName));
                if (File.Exists(path))
                    File.Copy(path, Path.Combine(backupDir, SqlConnectionFactory.fileName), true);
                fresh = false;
            }
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


            btnClockIn.Click += ButtonClockedInOut_Handler;// delegate { btnClockIn_Clicked(); };
            btnClockOut.Click += ButtonClockedInOut_Handler;// delegate { btnClockIn_Clicked(); };
            btnAddPunch.Click += btnAddPunch_Click;// delegate { btnClockIn_Clicked(); };
            ForReal.CheckedChange += ForReal_CheckedChanged;
            btnExit.Click += ExitDoer;
            btnEditPunch.Click += btnEditPunch_Click;// = FindViewById<Button>(Resource.Id.btnClockIn);
            btnSqlQuery.Click += btnSqlQuery_Click;// = FindViewById<Button>(Resource.Id.btnClockIn);
            btnViewHistory.Click += btnViewHistory_Click;// = FindViewById<Button>(Resource.Id.btnClockIn);

            btnChangeEmails.Click += BtnChangeEmails_Clicked;

            RealClockPunch = ForReal.Checked;

            using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnection())
            {
                Log.Debug("DATABASE", connection.ExecuteScalar<string>("PRAGMA database_list"));

                int c = connection.CreateTable<HourlyRate>(CreateFlags.AutoIncPK);


                c = connection.CreateTable<WorkInstance>(SQLite.CreateFlags.AutoIncPK);

                c = connection.CreateTable<FromPassword>(SQLite.CreateFlags.AutoIncPK);
                c = connection.CreateTable<ServerOut>(SQLite.CreateFlags.AutoIncPK);
                c = connection.CreateTable<EmailAddresses>(SQLite.CreateFlags.AutoIncPK);

            }
            try
            {
                using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                {
                    var hr = connection.Table<HourlyRate>().Any() ? connection.Table<HourlyRate>().Last() : null;
                    if (hr is HourlyRate)
                    {
                        MainActivity.initRate = hr.Rate;
                    }
                    else
                    {
                        MainActivity.initRate = 0f;
                    }
                    var v = connection.Table<WorkInstance>().Any() ? connection.Table<WorkInstance>().Last() : null;
                    txtLastPunch.Text = v != null ? v.ClockIn == v.ClockOut ? $"In {v.ClockIn.ToString()}" : $"Out {v.ClockOut.ToString()}" : "NULL";

                }
            }
            catch (Exception Ex)
            {
                txtMainOut.Text = $"ERR {Ex.Message}";
            }

            string na = string.Empty;
            Helper.SetWeekFrame(ref TimeIntervalBegin, ref TimeIntervalEnd);
            txtWeekTotalHours.Text = string.Empty + String.Format("{0:0.00}", Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref na));
            TimeIntervalBegin = Convert.ToDateTime(MainActivity.GetLocalTime().Month + "/" + "01" + "/" + MainActivity.GetLocalTime().Year);
            TimeIntervalEnd = Convert.ToDateTime((MainActivity.GetLocalTime().Month < 12 ? MainActivity.GetLocalTime().Month + 1 : 1) + "/" + "01" + "/" + (MainActivity.GetLocalTime().Month < 12 ? MainActivity.GetLocalTime().Year : MainActivity.GetLocalTime().Year + 1));

            string grossPayStr = string.Empty;
            float hrs = Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref grossPayStr);
            txtMonthTotalHours.Text = string.Empty + String.Format("{0:0.00}", hrs);
            txtGrossPay.Text = grossPayStr;
        }

        public void ButtonClockedInOut_Handler(object sender, EventArgs e)
        {
            output = " ";

            Button ib = ((Button)sender);

            int start = System.Environment.TickCount & Int32.MaxValue;
            SetOutput("Punch attempted..");
            OnClockPunchButtonsPushed(ib, e);

            AfterEmailSent();
            output += "  Time to run " + ((System.Environment.TickCount & Int32.MaxValue) - start);
            SetOutput(output);

        }
        public static DateTime GetLocalTime()
        {

            string datetime = $"{DateTime.Now.ToLocalTime().ToShortDateString()} {DateTime.Now.ToLocalTime().ToLongTimeString()}";

            return Convert.ToDateTime(datetime);
        }
        public static DateTime GetLocalTime(DateTime dt)
        {
            string datetime = $"{dt.ToLocalTime().ToShortDateString()} {dt.ToLocalTime().ToLongTimeString()}";

            dt = Convert.ToDateTime(datetime);
            return dt;
        }

        public void OnClockPunchButtonsPushed(Button ib, EventArgs e)
        {
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
            {
                WorkInstance wi = null;
                OUT = Resource.Id.btnClockOut == ib.Id;
                try
                {
                    wi = connection.Table<WorkInstance>().Last();
                }
                catch (Exception)
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
    }
}

