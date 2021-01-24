using Xamarin.Essentials;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TimeTrackerUniversal.Assets;
using TimeTrackerUniversal.Database;
using TimeTrackerUniversal.Database.Schema;
using Android.Views;

namespace TimeTrackerUniversal
{
    [Activity(Label = "TimeTrackerUniversal", MainLauncher = true, Icon = "@drawable/ttu")]
    public class MainActivity : Activity
    {
        Vibrator vb;
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

        //public boolean onTouch(View v, MotionEvent e) {
        //    // TODO Auto-generated method stub
        //    Vibrator vb = (Vibrator)getSystemService(Context.VIBRATOR_SERVICE);
        //    vb.vibrate(100);
        //    return false;
        //}
        //b.setOnTouchListener(new OnTouchListener()
        //{

        //    @Override


        //});
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState); // add this line to your code, it may also be called: bundle

            Acr.UserDialogs.UserDialogs.Init(this);

            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                if (status == PermissionStatus.Denied)
                {
                    // status = await Permissions.RequestAsync<Permissions.StorageRead>();
                    status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                    status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                    if (status == PermissionStatus.Denied)
                    {
                        // status = await Permissions.RequestAsync<Permissions.StorageRead>(); 
                        status = await Permissions.RequestAsync<Permissions.StorageRead>().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {

                Acr.UserDialogs.UserDialogs.Instance.Alert($"{ex}");
                Console.WriteLine($"{ex}");
            }
            Gross_net = true;
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            txtLastPunch = FindViewById<TextView>(Resource.Id.txtLastPunch);
            txtMainOut = FindViewById<TextView>(Resource.Id.txtMainOut);

            txtWeekTotalHours = FindViewById<TextView>(Resource.Id.txtWeekTotalHours);
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

            vb = (Vibrator)this.GetSystemService(VibratorService);
            btnClockIn.Click += ButtonClockedInOut_Handler;// delegate { btnClockIn_Clicked(); };
            btnClockOut.Click += ButtonClockedInOut_Handler;// delegate { btnClockIn_Clicked(); };
            btnAddPunch.Click += btnAddPunch_Click;// delegate { btnClockIn_Clicked(); };
            ForReal.CheckedChange += ForReal_CheckedChanged;
            btnExit.Click += ExitDoer;
            btnEditPunch.Click += btnEditPunch_Click;// = FindViewById<Button>(Resource.Id.btnClockIn);
            btnSqlQuery.Click += btnSqlQuery_Click;// = FindViewById<Button>(Resource.Id.btnClockIn);
            btnViewHistory.Click += btnViewHistory_Click;// = FindViewById<Button>(Resource.Id.btnClockIn);

            //btnChangeEmails.Touch += MyHaptic;
            btnChangeEmails.Touch += BtnChangeEmails_Clicked;
            txtWeekTotalHours.Click += txtWeekTotalHours_Click;
            txtGrossPay.Click += setStats;
            txtMonthTotalHours.Click += setStats;
            RealClockPunch = ForReal.Checked;
            setStats(null, null);
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
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
                    MainActivity.initRate = connection.Table<HourlyRate>().Any() ? connection.Table<HourlyRate>().Last().Rate : 0;

                    var v = connection.Table<WorkInstance>().Any() ? connection.Table<WorkInstance>().Last() : null;
                    txtLastPunch.Text = v != null ? v.ClockIn == v.ClockOut ? $"In {v.ClockIn.ToString()}" : $"Out {v.ClockOut.ToString()}" : "NULL";

                }
            }
            catch (Exception Ex)
            {
                txtMainOut.Text = $"ERR {Ex.Message}";
            }
            currentHours = false;
        }
        private void btnAddPunch_Click(object sender, EventArgs e)
        {
            try
            {
                vb.Vibrate(50);
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
                vb.Vibrate(50);
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
            try
            {
                vb.Vibrate(50);
                lock (_syncLock)
                {
                    Intent intent = new Intent(this.ApplicationContext, typeof(EditPunchActivity));
                    intent.SetFlags(ActivityFlags.ForwardResult);
                    intent.SetFlags(ActivityFlags.ReceiverForeground);
                    StartActivity(intent);
                }
            }
            catch (Exception exx)
            {
                Toast.MakeText(ApplicationContext, exx.Message + "\t      " + exx.StackTrace, ToastLength.Long).Show();
                Log.Error("EXCEPTION", exx.Message + "\t      " + exx.StackTrace);
            }
            //Toast.MakeText(ApplicationContext, "Need to Implement!!", ToastLength.Long).Show();
        }

        private void btnSqlQuery_Click(object sender, EventArgs e)
        {
            try
            {
                vb.Vibrate(50);
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
                vb.Vibrate(50);
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
            vb.Vibrate(50);
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
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        protected bool AfterEmailSent()
        {
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
            {
                if (ClockInOut_Success)
                {
                    string stamp = DateTime.Now.ToString() + "," + (OUT ? "OUT" : "IN") + "," + (RealClockPunch ? "REAL," + (connection.Table<EmailAddresses>().Where(x => x.EmailType == 1).Last()).Email : "FAKE," + (connection.Table<EmailAddresses>().Where(x => x.EmailType == 2).Last()).Email) + "\n";

                    string fileName = "/sdcard/punches/ClockPunches.txt";
                    string dirName = "/sdcard/punches";
                    if (!Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }
                    if (!File.Exists(fileName))
                    {
                        using (var writer = new StreamWriter(File.Create(fileName)))
                        {
                            writer.WriteLine(stamp);
                        }
                    }
                    else
                    {
                        using (var writer = new StreamWriter(File.OpenWrite(fileName)))
                        {
                            writer.WriteLine(stamp);
                        }

                    }
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

        private void txtWeekTotalHours_Click(object sender, EventArgs e)
        {
            vb.Vibrate(50);
            currentHours = !currentHours;
            setStats(null, EventArgs.Empty);
        }

        void setStats(object sender, EventArgs args)
        {
            vb.Vibrate(50);
            //if(sender is string && (string)sender=="Current")
            //{
            //    // how many hours so far today?
            //}
            CopyDB();
            //Toast.MakeText(ApplicationContext, "DB copied!!", ToastLength.Short).Show();
            float totalFromPeriod = 0f;
            string na = string.Empty;
            Helper.SetWeekFrame(ref TimeIntervalBegin, ref TimeIntervalEnd);
            var weekhrs = Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref na, ref totalFromPeriod);
            if (currentHours)
            {
                // how many our so far today
                weekhrs += TodaysHours();
            }
            txtWeekTotalHours.Text = string.Empty + String.Format("{0:0.00}", weekhrs);
            TimeIntervalBegin = Convert.ToDateTime(MainActivity.GetLocalTime().Month + "/" + "01" + "/" + MainActivity.GetLocalTime().Year);
            TimeIntervalEnd = Convert.ToDateTime((MainActivity.GetLocalTime().Month < 12 ? MainActivity.GetLocalTime().Month + 1 : 1) + "/" + "01" + "/" + (MainActivity.GetLocalTime().Month < 12 ? MainActivity.GetLocalTime().Year : MainActivity.GetLocalTime().Year + 1));

            string grossPayStr = string.Empty;
            float hrs = Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref grossPayStr, ref totalFromPeriod);

            grossPayStr = string.Format("${0:f}", (Gross_net ? totalFromPeriod : totalFromPeriod * .795));

            txtMonthTotalHours.Text = string.Empty + String.Format("{0:0.00}", hrs);
            txtGrossPay.Text = grossPayStr; ;

            Gross_net = !Gross_net;

        }
        private static float TodaysHours()
        {
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
            {
                try
                {
                    DateTime oneDayAgo = DateTime.Now.ToLocalTime().AddDays(-1);
                    var today = connection.Table<WorkInstance>().FirstOrDefault(x => x.IsValid && x.ClockIn == x.ClockOut && x.ClockIn >= oneDayAgo);
                    if (today != null)
                    {
                        //TimeSpan.TicksPerDay
                        TimeSpan diff = DateTime.Now.ToLocalTime() - today.ClockIn;
                        var hrs = diff.Hours + (diff.Minutes / 60f);
                        Log.Info("HOURSSS", hrs.ToString());
                        return hrs;
                    }
                    return 0f;
                }
                catch (Exception ex)
                {
                    Log.Error("ERRORRR", $"{ex}");
                    Android.Widget.Toast.MakeText(Application.Context, $"{ex}", Android.Widget.ToastLength.Long).Show();
                    return 0f;
                }
            }
        }
        private static bool _gross_net;
        private static bool currentHours;
        public static bool Gross_net
        {
            get => _gross_net; set => _gross_net = value;
        }
        public void ButtonClockedInOut_Handler(object sender, EventArgs e)
        {
            vb.Vibrate(50);
            output = " ";

            Button ib = ((Button)sender);

            int start = System.Environment.TickCount & Int32.MaxValue;
            SetOutput("Punch attempted..");
            OnClockPunchButtonsPushed(ib, e);

            AfterEmailSent();
            output += "  Time to run " + ((System.Environment.TickCount & Int32.MaxValue) - start);
            SetOutput(output);

        }

        public static void CopyDB()
        {
            //backup db
            try
            {
                string backupDir = "/sdcard/";
                string fullFileName = Path.Combine(backupDir, GetDigitDate() + SqlConnectionFactory.fileName);

                if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
                if (!File.Exists(fullFileName))
                {
                    var fs = File.Create(fullFileName);
                    //fs.
                }
                //copy
                if (File.Exists(SqlConnectionFactory.FULLDBFILEPATH))
                {
                    File.Copy(SqlConnectionFactory.FULLDBFILEPATH, fullFileName, true);

                    //purge old copies
                    var files = GetFileNamesOlderThan_Days(backupDir, "*.db3", -7);
                    foreach (var item in files)
                    {
                        File.Delete(Path.Combine(backupDir, item));
                    }
                }


            }
            catch (Exception ex)
            {
                //  throw;
                Acr.UserDialogs.UserDialogs.Instance.Toast("Couldn't back up!!");

            }
        }
        private static long DaysInTicks(int days = 1)
        {
            return DateTime.Now.AddDays(days).ToLocalTime().Ticks - DateTime.Now.ToLocalTime().Ticks;
        }
        private static string[] GetFileNamesOlderThan_Days(string path, string filter, int days = 7)
        {
            var files = Directory.GetFiles(path, filter).Where(x => File.GetLastWriteTime(x) < DateTime.Now.AddDays(days).ToLocalTime()).ToArray();
            for (int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileName(files[i]);
            return files;
        }
        public static string GetDigitDate(int daysToAdd = 0)
        {
            return $"{DateTime.Now.AddDays(daysToAdd).ToLocalTime().Month.ToString("00")}" +
                $"{DateTime.Now.AddDays(daysToAdd).ToLocalTime().Day.ToString("00")}" +
                $"{DateTime.Now.AddDays(daysToAdd).ToLocalTime().Year}";
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

