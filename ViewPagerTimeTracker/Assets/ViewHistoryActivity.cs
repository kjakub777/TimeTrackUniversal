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
            txtImportOutput.Text = $"FROM {TimeIntervalBegin.ToLocalTime().ToShortDateString()} {TimeIntervalBegin.ToLocalTime().ToLongTimeString()}\n";
            txtImportOutput.Text += $"TO {TimeIntervalEnd.ToLocalTime().ToShortDateString()} {TimeIntervalEnd.ToLocalTime().ToLongTimeString()}\n";

            txtImportOutput.Text += string.Empty + String.Format("{0:0.00}", hrs) + "\n";
            txtIntervalTotalHours.Text = string.Empty + String.Format("{0:0.00}", hrs) + "\n";
      
            txtGrossPay.Text = grossPayStr;
            txtImportOutput.Text += $" GrossPay {grossPayStr}\n";
            Helper.SetWeekFrame(ref TimeIntervalBegin, ref TimeIntervalEnd);
            txtImportOutput.Text += $"week FROM {TimeIntervalBegin.ToLocalTime().ToShortDateString()} {TimeIntervalBegin.ToLocalTime().ToLongTimeString()}\n";
            txtImportOutput.Text += $"week TO {TimeIntervalEnd.ToLocalTime().ToShortDateString()} {TimeIntervalEnd.ToLocalTime().ToLongTimeString()}\n";

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
            if (!Exported)
            {
                List<string> lines = new List<string>(); 
                List<WorkInstance> instances = new List<WorkInstance>();
                try
                {
                    btnExport.Text = "Now Exported";

                    using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                    {
                        if (!string.IsNullOrWhiteSpace(txtFileName.Text))
                        {
                            instances = connection.Table<WorkInstance>().Where(x => x.IsValid).Cast<WorkInstance>().ToList();
                        }
                    }
                    if (instances.Count() > 0)
                    {
                        //**
                        try
                        {
                            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                            path = Path.Combine(path, SqlConnectionFactory.fileName);
                            string backupDir = "/sdcard/";

                            if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
                            if (!File.Exists(Path.Combine(backupDir, SqlConnectionFactory.fileName))) File.Create(Path.Combine(backupDir, SqlConnectionFactory.fileName));
                            if (File.Exists(path))
                                File.Copy(path, Path.Combine(backupDir, SqlConnectionFactory.fileName), true);
                            //**
                            string dirName = "/sdcard";
                            string fileName = Path.Combine(dirName, txtFileName.Text);
                            if (!Directory.Exists(dirName))
                            {
                                Directory.CreateDirectory(dirName);
                            }
                            if (!File.Exists(fileName))
                            {
                                File.Create(fileName);
                            }

                            path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), txtFileName.Text);

                            foreach (WorkInstance wi in instances)
                            {
                                lines.Add(String.Format("{0:MM/dd/yyyy}", wi.Date) + "," + String.Format("{0:HH:mm:ss}", wi.ClockIn) + "," + String.Format("{0:HH:mm:ss}", wi.ClockOut));
                                File.AppendAllText(path, String.Format("{0:MM/dd/yyyy}", wi.Date) + "," + String.Format("{0:HH:mm:ss}", wi.ClockIn) + "," + String.Format("{0:HH:mm:ss}", wi.ClockOut) + System.Environment.NewLine);
                            }
                            File.Copy(path, Path.Combine(backupDir, txtFileName.Text), true);

                        }
                        catch (IOException x)
                        {
                            txtImportOutput.Text = "FileIO Exception, keep tring and it will work!" + x.Message;
                            Toast.MakeText(ApplicationContext, "FileIO Exception, keep tring and it will work!" + x.Message, ToastLength.Long).Show(); ;
                        }
                    }
                }
                catch (Exception exception)
                {
                    txtImportOutput.Text = exception.ToString();
                    Toast.MakeText(ApplicationContext, "Make Sure there is an hourly rate " + exception.ToString(), ToastLength.Long).Show();
                    return false;
                }

                txtImportOutput.Text = $"Exported {lines.Count()} records to {txtFileName.Text}";
            }
            else
            {
                txtImportOutput.Text = $"Already exported files to {txtFileName.Text}";
                Toast.MakeText(ApplicationContext, "Already exported!", ToastLength.Long).Show();
                return true;
            }
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