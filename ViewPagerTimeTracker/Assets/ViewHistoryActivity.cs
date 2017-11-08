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
using TimeTrackerUniversal.Assets;
using System.Threading.Tasks;

namespace TimeTrackerUniversal
{
    [Activity(Label = "History", MainLauncher = false, Icon = "@drawable/icon")]
    public class ViewHistoryActivity : Activity
    {
        static readonly object _syncLock = new object();
        EditText txtFileName;
        TextView txtImportOutput;
        TextView txtIntervalTotalHours;
        TextView txtWeekTotalHours;
        TextView txtGrossPay;
        Button btnViewHistoryExit;//sqlQuery btnExecuteSql 
        Button btnImport;//sqlQuery btnExecuteSql 
        Button btnExport;//sqlQuery btnExecuteSql 
        Button btnClearDB;//sqlQuery btnExecuteSql 
        Button btnDeleteLastPunch;//sqlQuery btnExecuteSql 
        DateTime TimeIntervalBegin;// = Convert.ToDateTime(MainActivity.GetLocalTime().Month + "/" + "01" + "/" + MainActivity.GetLocalTime().Year);
        DateTime TimeIntervalEnd;// = Convert.ToDateTime((MainActivity.GetLocalTime().Month < 12 ? MainActivity.GetLocalTime().Month + 1 : 1) + "/" + "01" + "/" + MainActivity.GetLocalTime().Year);
        DatePicker datePickerFromDate;
        DatePicker datePickerToDate;
        CheckBox chkClearAll;
        Button btnViewTimeframe;
        Button btnMonthPicker;
        private bool Imported;
        private bool Exported;

        public ViewHistoryActivity()
        {


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
            //btnDayPicker = FindViewById<Button>(Resource.Id.btnDayPicker);
            //btnMonthPicker = FindViewById<Button>(Resource.Id.//btnMonthPicker);
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
             
            // btnDayPicker.Click += btnDayPicker_Clicked;
            btnViewHistoryExit.Click += btnExit_Click;
            btnImport.Click += btnImport_Click;
            btnExport.Click += btnExport_Click;
            btnClearDB.Click += btnClearDB_Click;
            btnDeleteLastPunch.Click += btnDeleteLastPunch_Click;
            btnViewTimeframe.Click += btnViewTimeframe_Click;
            setTimeframes();

        }
        public void setTimeframes()
        {
            string na = "";
            Helper.SetWeekFrame(ref TimeIntervalBegin, ref TimeIntervalEnd);
            txtWeekTotalHours.Text = string.Empty + String.Format("{0:0.00}", Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref na));
            TimeIntervalBegin = Convert.ToDateTime(MainActivity.GetLocalTime().Month + "/" + "01" + "/" + MainActivity.GetLocalTime().Year);
            TimeIntervalEnd = Convert.ToDateTime((MainActivity.GetLocalTime().Month < 12 ? MainActivity.GetLocalTime().Month + 1 : 1) + "/" + "01" + "/" + (MainActivity.GetLocalTime().Month < 12 ? MainActivity.GetLocalTime().Year : MainActivity.GetLocalTime().Year + 1));

            string grossPayStr = "";
            float hrs = Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref grossPayStr);
            txtIntervalTotalHours.Text = string.Empty + String.Format("{0:0.00}", hrs);
            txtGrossPay.Text = grossPayStr;
        }
        private void btnDeleteLastPunch_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
            {
                var id = connection.Table<WorkInstance>().Last();
                txtImportOutput.Text = $"Deleted {id.ToString()}";
                connection.Delete<WorkInstance>(id.Oid);
            }
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
        public bool Export()
        {
            if (!Exported)
            {
                List<string> lines = new List<string>();// File.ReadAllLines("/sdcard/import.csv");
                List<WorkInstance> instances = new List<WorkInstance>();
                try
                {
                    btnExport.Text = "Now Exported";

                    using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
                    {
                        if (!string.IsNullOrWhiteSpace(txtFileName.Text))
                        {
                            instances = connection.Table<WorkInstance>().Where(x => x.IsValid).Cast<WorkInstance>().ToList();
                        }
                    }
                    if (instances.Count() > 0)
                    {
                        //**
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

                        // File.OpenWrite(fileName);
                        path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), txtFileName.Text);
                        // File.OpenWrite(path);
                        foreach (WorkInstance wi in instances)
                        {
                            lines.Add(String.Format("{0:MM/dd/yyyy}", wi.Date) + "," + String.Format("{0:HH:mm:ss}", wi.ClockIn) + "," + String.Format("{0:HH:mm:ss}", wi.ClockOut));
                            File.AppendAllText(path, String.Format("{0:MM/dd/yyyy}", wi.Date) + "," + String.Format("{0:HH:mm:ss}", wi.ClockIn) + "," + String.Format("{0:HH:mm:ss}", wi.ClockOut) + System.Environment.NewLine);
                        }
                        File.Copy(path, Path.Combine(backupDir, txtFileName.Text), true);
                        // File.SetCreationTime(fileName, MainActivity.GetLocalTime());
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
                                        using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
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
        private void btnExit_Click(object sender, EventArgs e)
        {
            SetResult(Result.Ok);
            this.OnBackPressed();
        }

        public void SetTimeFrame(DateTime fromDate, DateTime toDate)
        {
            TimeIntervalBegin = fromDate;
            TimeIntervalEnd = toDate;
        }
        private void btnClearDB_Click(object sender, EventArgs e)
        {
            txtFileName.Text = "deletedWorkinstances.csv";
            btnExport_Click(sender, e);
            using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
            {
                //foreach (WorkInstance wi in instances)
                //{
                //    lines.Add(String.Format("{0:MM/dd/yyyy}", wi.Date) + "," + String.Format("{0:MM/dd/yyyy HH:mm:ss}", wi.ClockIn) + "," + String.Format("{0:MM/dd/yyyy HH:mm:ss}", wi.ClockOut));
                //}
                //File.WriteAllLines(fileName, lines.ToArray());
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
        protected void btnViewTimeframe_Click(object sender, EventArgs e)
        {
            var dpfd = datePickerFromDate.DateTime;
            DateTime fromDate = MainActivity.GetLocalTime(dpfd);
            var dptd = datePickerToDate.DateTime;
            DateTime toDate = MainActivity.GetLocalTime(dptd); 

            //txtImportOutput.Text = $"FROM {fromDate.ToLocalTime().ToShortDateString()} {fromDate.ToLocalTime().ToLongTimeString()}\n";
            //txtImportOutput.Text += $"TO {toDate.ToLocalTime().ToShortDateString()} {toDate.ToLocalTime().ToLongTimeString()}\n";
            SetTimeFrame(fromDate, toDate);
            string grossPayStr = "";
            float hrs = Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref grossPayStr);
            txtImportOutput.Text = $"FROM {TimeIntervalBegin.ToLocalTime().ToShortDateString()} {TimeIntervalBegin.ToLocalTime().ToLongTimeString()}\n";
            txtImportOutput.Text += $"TO {TimeIntervalEnd.ToLocalTime().ToShortDateString()} {TimeIntervalEnd.ToLocalTime().ToLongTimeString()}\n";

            txtImportOutput.Text += string.Empty + String.Format("{0:0.00}", hrs)+"\n";
            txtIntervalTotalHours.Text = string.Empty + String.Format("{0:0.00}", hrs) + "\n";
            //using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
            //{
            //    //  connection.Table<HourlyRate>().Last()
            //    txtGrossPay.Text = "$" + String.Format("{0:0.00}", hrs * Convert.ToDouble(connection.Table<HourlyRate>().Last().Rate));
            //}

            txtGrossPay.Text = grossPayStr;
            txtImportOutput.Text += $" GrossPay {grossPayStr}\n";
            Helper.SetWeekFrame(ref TimeIntervalBegin, ref TimeIntervalEnd);
            txtImportOutput.Text += $"week FROM {TimeIntervalBegin.ToLocalTime().ToShortDateString()} {TimeIntervalBegin.ToLocalTime().ToLongTimeString()}\n";
            txtImportOutput.Text += $"week TO {TimeIntervalEnd.ToLocalTime().ToShortDateString()} {TimeIntervalEnd.ToLocalTime().ToLongTimeString()}\n";

            string na = "";
            txtWeekTotalHours.Text = Helper.GetTotalHoursForTimePeriod(TimeIntervalBegin, TimeIntervalEnd, ref na).ToString();
            txtImportOutput.Text += $"week hours : {txtWeekTotalHours.Text}\n";
        }
    }
}