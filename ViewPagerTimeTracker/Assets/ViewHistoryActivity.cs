using Android.App;
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

namespace TimeTrackerUniversal
{
    [Activity(Label = "History", MainLauncher = false, Icon = "@drawable/icon")]
    public class ViewHistoryActivity : Activity
    {
        static readonly object _syncLock = new object();
        Button btnClearDB;//sqlQuery btnExecuteSql 
        Button btnDeleteLastPunch;//sqlQuery btnExecuteSql 
        Button btnExport;//sqlQuery btnExecuteSql 
        Button btnImport;//sqlQuery btnExecuteSql 
        Button btnViewHistoryExit;//sqlQuery btnExecuteSql 
        Button btnViewTimeframe;
        CheckBox chkClearAll;
        DatePicker datePickerFromDate;
        DatePicker datePickerToDate;
        private bool Exported;
        private bool Imported;
        DateTime TimeIntervalBegin;// = Convert.ToDateTime(MainActivity.GetLocalTime().Month + "/" + "01" + "/" + MainActivity.GetLocalTime().Year);
        DateTime TimeIntervalEnd;// = Convert.ToDateTime((MainActivity.GetLocalTime().Month < 12 ? MainActivity.GetLocalTime().Month + 1 : 1) + "/" + "01" + "/" + MainActivity.GetLocalTime().Year);
        EditText txtFileName;
        TextView txtGrossPay;
        TextView txtImportOutput;
        TextView txtIntervalTotalHours;
        TextView txtWeekTotalHours;

        public ViewHistoryActivity()
        {


        }

        private void btnClearDB_Click(object sender, EventArgs e)
        {
            txtFileName.Text = "deletedWorkinstances.csv";
            btnExport_Click(sender, e);
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
            {

                if (chkClearAll.Checked)
                {
                    connection.DropTable<WorkInstance>();
                    connection.DropTable<HourlyRate>();
                    connection.DropTable<FromPassword>();
                    connection.DropTable<EmailAddresses>();
                    connection.DropTable<ServerOut>();
                    txtImportOutput.Text = $"Dropped tables";
                }
                else
                {
                    connection.DropTable<WorkInstance>();
                    txtImportOutput.Text = $"Dropped WorkInstances";
                }
                int c = connection.CreateTable<HourlyRate>(CreateFlags.AutoIncPK);

                c = connection.CreateTable<WorkInstance>(SQLite.CreateFlags.AutoIncPK);

                c = connection.CreateTable<FromPassword>(SQLite.CreateFlags.AutoIncPK);

                c = connection.CreateTable<ServerOut>(SQLite.CreateFlags.AutoIncPK);

                c = connection.CreateTable<EmailAddresses>(SQLite.CreateFlags.AutoIncPK);

            }

        }
        private void btnDeleteLastPunch_Click(object sender, EventArgs e)
        {
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
            {
                var id = connection.Table<WorkInstance>().Last();
                txtImportOutput.Text = $"Deleted {id.ToString()}";
                connection.Delete<WorkInstance>(id.Oid);
            }
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            SetResult(Result.Ok);
            this.OnBackPressed();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                lock (_syncLock)
                {
                    Exported = Export();
                }
            }
            catch (Exception exx)
            {
                Log.Error("EXCEPTION", exx.Message + "\t      " + exx.StackTrace);
            }

        }
        private void btnImport_Click(object sender, EventArgs e)
        {
            if (!Imported)
            {
                int recordCount = 0;
                try
                {
                    Imported = true;
                    btnImport.Text = "Now imported";
                    List<string> dateStrings = new List<string>();
                    string[] lines = File.ReadAllLines("/sdcard/" + txtFileName.Text);

                    foreach (string line in lines)
                    {
                        int localCount = 0;
                        string DATE = string.Empty;
                        string punchIN = string.Empty;
                        string punchOUT = string.Empty;
                        string[] cols = line.Split(',');
                        foreach (string col in cols)
                        {
                            if (!string.IsNullOrWhiteSpace(col))
                            {
                                if (localCount == 0)
                                {
                                    DATE = col;
                                }
                                else if ((localCount % 2) != 0) //in
                                {
                                    punchIN = col;
                                }
                                else if ((localCount % 2) == 0) //out
                                {
                                    try
                                    {
                                        using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                                        {
                                            punchOUT = col;
                                            DateTime resDate;
                                            DateTime.TryParse(DATE, out resDate);
                                            DateTime resIn;
                                            DateTime.TryParse(punchIN, out resIn);
                                            DateTime resOut;
                                            DateTime.TryParse(punchOUT, out resOut);
                                            float hourlyRate = (connection.Table<HourlyRate>().Last()).Rate;
                                            connection.Insert(new WorkInstance()
                                            {
                                                Date = resDate.Date,
                                                ClockIn = Convert.ToDateTime(String.Format("{0:HH:mm:ss}", resIn)),
                                                ClockOut = Convert.ToDateTime(String.Format("{0:HH:mm:ss}", resOut)),
                                                HourlyRate = hourlyRate,
                                                IsValid = true
                                            });
                                            connection.Commit();
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        Log.Error("EXCEPTION_INSERT_WI", exception.ToString());
                                        Toast.MakeText(ApplicationContext, "Make Sure there is an hourly rate " + exception.ToString(), ToastLength.Long).Show();
                                    }
                                }
                                localCount++;
                            }
                        }

                        //}
                        recordCount += localCount;
                    }

                }
                catch (Exception exception)
                {
                    txtImportOutput.Text = "Make Sure there is an hourly rate " + exception.ToString();
                    Toast.MakeText(ApplicationContext, "Make Sure there is an hourly rate " + exception.ToString(), ToastLength.Long).Show();
                }
                txtImportOutput.Text = $"Imported {recordCount} workinstances from {txtFileName.Text}";
            }
            else
            {
                txtImportOutput.Text = "Already Imported";
                Toast.MakeText(ApplicationContext, "Already Imported", ToastLength.Long).Show();
            }
        }

        protected void btnViewTimeframe_Click(object sender, EventArgs e)
        {
            var dpfd = datePickerFromDate.DateTime;
            DateTime fromDate = MainActivity.GetLocalTime(dpfd);
            var dptd = datePickerToDate.DateTime;
            DateTime toDate = MainActivity.GetLocalTime(dptd);

            SetTimeFrame(fromDate, toDate);
            string grossPayStr = string.Empty;
            float hrs = Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref grossPayStr);
            txtImportOutput.Text = $"FROM {TimeIntervalBegin.Date.Date.ToShortDateString()}  \n";
            txtImportOutput.Text += $"TO {TimeIntervalEnd.Date.Date.ToShortDateString()}\n";

            txtImportOutput.Text += string.Empty + String.Format("{0:0.00}", hrs) + "\n";
            txtIntervalTotalHours.Text = string.Empty + String.Format("{0:0.00}", hrs) + "\n";

            txtGrossPay.Text = grossPayStr;
            txtImportOutput.Text += $" GrossPay {grossPayStr}\n";
            Helper.SetWeekFrame(ref TimeIntervalBegin, ref TimeIntervalEnd);
            txtImportOutput.Text += $"week FROM {TimeIntervalBegin.Date.Date.ToShortDateString()}\n";
            txtImportOutput.Text += $"week TO {TimeIntervalEnd.Date.Date.ToShortDateString()}\n";

            string na = string.Empty;
            txtWeekTotalHours.Text = Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref na).ToString();
            txtImportOutput.Text += $"week hours : {txtWeekTotalHours.Text}\n";
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ViewHistory);

            btnViewHistoryExit = FindViewById<Button>(Resource.Id.btnViewHistoryExit);
            btnImport = FindViewById<Button>(Resource.Id.btnImport);
            btnExport = FindViewById<Button>(Resource.Id.btnExport);
            btnViewTimeframe = FindViewById<Button>(Resource.Id.btnViewTimeframe);
            btnClearDB = FindViewById<Button>(Resource.Id.btnClearDB);
            btnDeleteLastPunch = FindViewById<Button>(Resource.Id.btnDeleteLastPunch);
            txtImportOutput = FindViewById<TextView>(Resource.Id.txtImportOutput);
            txtFileName = FindViewById<EditText>(Resource.Id.txtFileName);
            datePickerFromDate = FindViewById<DatePicker>(Resource.Id.datePickerFromDate);
            datePickerToDate = FindViewById<DatePicker>(Resource.Id.datePickerToDate);
            txtIntervalTotalHours = FindViewById<TextView>(Resource.Id.txtIntervalTotalHours);
            txtWeekTotalHours = FindViewById<TextView>(Resource.Id.txtWeekTotalHours);
            txtGrossPay = FindViewById<TextView>(Resource.Id.txtGrossPay);
            chkClearAll = FindViewById<CheckBox>(Resource.Id.chkClearAll);
            DateTime dt = DateTime.Now.ToLocalTime();//.Month
            Java.Lang.Boolean b = Java.Lang.Boolean.True;

            btnViewHistoryExit.Click += btnExit_Click;
            btnImport.Click += btnImport_Click;
            btnExport.Click += btnExport_Click;
            btnClearDB.Click += btnClearDB_Click;
            btnDeleteLastPunch.Click += btnDeleteLastPunch_Click;
            btnViewTimeframe.Click += btnViewTimeframe_Click;
            setTimeframes();

        }

        public bool Export()
        {
            if (!string.IsNullOrWhiteSpace(txtFileName.Text))
            {
                if (!Exported)
                {
                    //prepare
                    List<string> lines = new List<string>();
                    try
                    {

                        List<WorkInstance> workinstances = new List<WorkInstance>();
                        List<HourlyRate> hourlyrates = new List<HourlyRate>();
                        List<EmailAddresses> emailaddresses = new List<EmailAddresses>();
                        List<FromPassword> frompasswords = new List<FromPassword>();
                        List<ServerOut> serverouts = new List<ServerOut>();
                        var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                        path = Path.Combine(path, SqlConnectionFactory.fileName);
                        string backupDir = "/sdcard/";

                        if (!Directory.Exists(backupDir))
                            Directory.CreateDirectory(backupDir);
                        if (!File.Exists(Path.Combine(backupDir, SqlConnectionFactory.fileName)))
                            File.Create(Path.Combine(backupDir, SqlConnectionFactory.fileName));
                        if (File.Exists(path))
                            File.Copy(path, Path.Combine(backupDir, SqlConnectionFactory.fileName), true);
                        //**

                        string fileName = Path.Combine(backupDir, txtFileName.Text);
                        string fileNamesql = Path.Combine(backupDir, txtFileName.Text + ".sql");
                        if (!Directory.Exists(backupDir))
                        {
                            Directory.CreateDirectory(backupDir);
                        }
                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                            File.Create(fileName);
                        }
                        else
                        {
                            File.Create(fileName);
                        }

                        if (File.Exists(fileNamesql))
                        {
                            File.Delete(fileNamesql);
                            File.Create(fileNamesql);
                        }
                        else
                        {
                            File.Create(fileNamesql);
                        }


                        path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), txtFileName.Text);
                        var pathsql = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), txtFileName.Text + ".sql");
                        //**

                        //****get data
                        using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                        {
                            workinstances = connection.Table<WorkInstance>().Where(x => x.IsValid).Cast<WorkInstance>().ToList();
                        }
                        using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                        {
                            hourlyrates = connection.Table<HourlyRate>().Where(x => x.IsValid).Cast<HourlyRate>().ToList();
                        }
                        using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                        {
                            emailaddresses = connection.Table<EmailAddresses>().Where(x => x.IsValid).Cast<EmailAddresses>().ToList();
                        }
                        using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                        {
                            frompasswords = connection.Table<FromPassword>().Where(x => x.IsValid).Cast<FromPassword>().ToList();
                        }
                        using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                        {
                            serverouts = connection.Table<ServerOut>().Where(x => x.IsValid).Cast<ServerOut>().ToList();
                        }
                        if (workinstances.Count() > 0)
                        {
                            //**
                            try
                            {

                                foreach (WorkInstance wi in workinstances)
                                {
                                    lines.Add(String.Format("{0:MM/dd/yyyy}", wi.Date) + "," + String.Format("{0:HH:mm:ss}", wi.ClockIn) + "," + String.Format("{0:HH:mm:ss}", wi.ClockOut));
                                    File.AppendAllText(path, String.Format("{0:MM/dd/yyyy}", wi.Date) + "," + String.Format("{0:HH:mm:ss}", wi.ClockIn) + "," + String.Format("{0:HH:mm:ss}", wi.ClockOut) + System.Environment.NewLine);
                                    File.AppendAllText(pathsql, $"INSERT INTO WorkInstance(Date,ClockIn,ClockOut,IsValid,HourlyRate)VALUES({wi.Date.Ticks},{wi.ClockIn.Ticks},{wi.ClockOut.Ticks},1,{wi.HourlyRate});{System.Environment.NewLine}");
                                }
                                //canonly read in workinstances as csv for now, res have to be sql
                                File.Copy(path, Path.Combine(backupDir, txtFileName.Text), true);
                            }
                            catch (IOException x)
                            {
                                txtImportOutput.Text = "FileIO Exception, keep tring and it will work!" + x.Message;
                                Toast.MakeText(ApplicationContext, "FileIO Exception, keep tring and it will work!" + x.Message, ToastLength.Long).Show(); ;
                            }

                        }
                        if (emailaddresses.Count() > 0)
                        {  //**
                            try
                            {

                                foreach (var ea in emailaddresses)
                                {
                                    File.AppendAllText(pathsql, $"INSERT INTO EmailAddresses(Date,Email,IsValid,EmailType) VALUES({ea.Date.Ticks},'{ea.Email}',1,{ea.EmailType});{System.Environment.NewLine}");
                                }

                            }
                            catch (IOException x)
                            {
                                txtImportOutput.Text = "FileIO Exception, keep tring and it will work!" + x.Message;
                                Toast.MakeText(ApplicationContext, "FileIO Exception, keep tring and it will work!" + x.Message, ToastLength.Long).Show(); ;
                            }
                        }

                        if (frompasswords.Count() > 0)
                        {  //**
                            try
                            {
                                var pw = frompasswords.Last();
                                File.AppendAllText(pathsql, $"INSERT INTO FromPassword(Date,Pass,IsValid) VALUES({pw.Date.Ticks},'{pw.Pass}',1);{System.Environment.NewLine}");


                            }
                            catch (IOException x)
                            {
                                txtImportOutput.Text = "FileIO Exception, keep tring and it will work!" + x.Message;
                                Toast.MakeText(ApplicationContext, "FileIO Exception, keep tring and it will work!" + x.Message, ToastLength.Long).Show(); ;
                            }
                        }
                        if (hourlyrates.Count() > 0)
                        {  //**
                            try
                            {
                                foreach (var hr in hourlyrates)
                                {
                                    File.AppendAllText(pathsql, $"INSERT INTO HourlyRate(Date,Rate,IsValid) VALUES({hr.Date.Ticks},{hr.Rate},1);{System.Environment.NewLine}");
                                }

                            }
                            catch (IOException x)
                            {
                                txtImportOutput.Text = "FileIO Exception, keep tring and it will work!" + x.Message;
                                Toast.MakeText(ApplicationContext, "FileIO Exception, keep tring and it will work!" + x.Message, ToastLength.Long).Show(); ;
                            }
                        }
                        if (serverouts.Count() > 0)
                        {  //**
                            try
                            {
                                var so = serverouts.Last();
                                File.AppendAllText(pathsql, $"INSERT INTO ServerOut(Date,Server,IsValid) VALUES({so.Date.Ticks},'{so.Server}',1);{System.Environment.NewLine}");


                            }
                            catch (IOException x)
                            {
                                txtImportOutput.Text = "FileIO Exception, keep tring and it will work!" + x.Message;
                                Toast.MakeText(ApplicationContext, "FileIO Exception, keep tring and it will work!" + x.Message, ToastLength.Long).Show(); ;
                            }
                        }

                        File.Copy(pathsql, Path.Combine(backupDir, txtFileName.Text + ".sql"), true);
                    }
                    catch (Exception exception)
                    {
                        txtImportOutput.Text = exception.ToString();
                        Toast.MakeText(ApplicationContext, "Make Sure there is an hourly rate " + exception.ToString(), ToastLength.Long).Show();
                        return false;
                    }
                    finally
                    {
                        File.Delete(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), txtFileName.Text));
                        File.Delete(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), txtFileName.Text + ".sql"));
                    }

                    txtImportOutput.Text = $"Exported {lines.Count()} records to {txtFileName.Text}";
                }
                else
                {
                    txtImportOutput.Text = $"Already exported files to {txtFileName.Text}";
                    Toast.MakeText(ApplicationContext, "Already exported!", ToastLength.Long).Show();
                    return true;
                }
            }
            btnExport.Text = "Now Exported";
            return true;
        }

        public void SetTimeFrame(DateTime fromDate, DateTime toDate)
        {
            TimeIntervalBegin = fromDate;
            TimeIntervalEnd = toDate;
        }
        public void setTimeframes()
        {
            string na = string.Empty;
            Helper.SetWeekFrame(ref TimeIntervalBegin, ref TimeIntervalEnd);
            txtWeekTotalHours.Text = string.Empty + String.Format("{0:0.00}", Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref na));
            TimeIntervalBegin = Convert.ToDateTime(MainActivity.GetLocalTime().Month + "/" + "01" + "/" + MainActivity.GetLocalTime().Year);
            TimeIntervalEnd = Convert.ToDateTime((MainActivity.GetLocalTime().Month < 12 ? MainActivity.GetLocalTime().Month + 1 : 1) + "/" + "01" + "/" + (MainActivity.GetLocalTime().Month < 12 ? MainActivity.GetLocalTime().Year : MainActivity.GetLocalTime().Year + 1));

            string grossPayStr = string.Empty;
            float hrs = Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref grossPayStr);
            txtIntervalTotalHours.Text = string.Empty + String.Format("{0:0.00}", hrs);
            txtGrossPay.Text = grossPayStr;
        }
    }
}