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
    [Activity(Label = "History", MainLauncher = false, Icon = "@drawable/icon")]
    public class ViewHistoryActivity : Activity
    {
        EditText sqlQuery;
        TextView txtOutput;
        Button btnExit;//sqlQuery btnExecuteSql 
        DateTime TimeIntervalBegin;
        DateTime TimeIntervalEnd;
        Button btnDayPicker;
        Button btnMonthPicker;
        public ViewHistoryActivity()
        {


        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ViewHistory);

            btnDayPicker = FindViewById<Button>(Resource.Id.btnDayPicker);
            btnExit = FindViewById<Button>(Resource.Id.btnExit);
            sqlQuery = FindViewById<EditText>(Resource.Id.sqlQuery);
            txtOutput = FindViewById<TextView>(Resource.Id.txtOutput);

            DateTime dt = DateTime.Now.ToLocalTime();//.Month
            Java.Lang.Boolean b = Java.Lang.Boolean.True;

            btnDayPicker.Click += btnDayPicker_Clicked;
            btnExit.Click += btnExit_Click;

        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            SetResult(Result.Ok);
            this.OnBackPressed();
        }
        protected void btnDayPicker_Clicked(object sender, EventArgs e)
        {
            EventHandler<DatePickerDialog.DateSetEventArgs> OnDialogDateSet = new EventHandler<DatePickerDialog.DateSetEventArgs>(OnDialogDatePickerSet);

            DatePickerDialog dpd = new DatePickerDialog(this, OnDialogDateSet, MainActivity.GetLocalTime().Year, MainActivity.GetLocalTime().Month - 1, MainActivity.GetLocalTime().Day);
            try
            {
                Java.Lang.Reflect.Field[] datePickerDialogFields = dpd.Class.GetDeclaredFields();
                foreach (Java.Lang.Reflect.Field datePickerDialogField in datePickerDialogFields)
                {
                    Log.WriteLine(LogPriority.Info, "INFOKJ", datePickerDialogField.Name);
                    if (datePickerDialogField.Name == "mDatePicker")
                    {
                        datePickerDialogField.Accessible = (true);
                        DatePicker datePicker = (DatePicker)datePickerDialogField.Get(dpd);
                        Java.Lang.Reflect.Field[] datePickerFields = datePickerDialogField.Type.GetDeclaredFields();
                        foreach (Java.Lang.Reflect.Field datePickerField in datePickerFields)
                        {
                            Log.WriteLine(LogPriority.Info, "test", datePickerField.Name);
                            if ("mDaySpinner" == datePickerField.Name)
                            {
                                datePickerField.Accessible = (true);
                                Object dayPicker = datePickerField.Get(datePicker);
                                ((View)dayPicker).Visibility = ViewStates.Gone;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            dpd.Show();
        }
        protected virtual void OnDialogDatePickerSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            //Log.Debug("DEEEEEELEELEEEEEEGEEEET", e.Month.ToString());
            //txtMainOut.Text = e.Date.ToLongDateString();
            //dateTxtMonth.Text = string.Empty + (e.Month + 1).ToString();
            //dateTxtYear.Text = e.Year.ToString();
            //if (SetMonthFrame())
            //{
            //    float hrs = GetTotalHoursForTimePeriod();
            //    txtMonthTotalHours.Text = string.Empty + String.Format("{0:0.00}", hrs);
            //    txtGrossPay.Text = "$" + String.Format("{0:0.00}", hrs * Convert.ToDouble(txtCurrentPayRate.Text));
            //}
            //if (SetWeekFrame())
            //{
            //    float whrs = GetTotalHoursForTimePeriod();
            //    txtWeekTotalHours.Text = string.Empty + String.Format("{0:0.00}", whrs);
            //}
        }
        public bool SetWeekFrame()
        {
            DayOfWeek dayOfWeek = MainActivity.GetLocalTime().DayOfWeek;
            int diffB = dayOfWeek - DayOfWeek.Sunday;
            int diffE = DayOfWeek.Saturday - dayOfWeek;

            TimeSpan bts = new TimeSpan(diffB * TimeSpan.TicksPerDay);
            TimeSpan ets = new TimeSpan((diffE + 1) * TimeSpan.TicksPerDay);

            TimeIntervalBegin = MainActivity.GetLocalTime().Subtract(bts).Date;
            TimeIntervalEnd = MainActivity.GetLocalTime().Add(ets).Date;

            Log.Error("BEGINWEEK", TimeIntervalBegin.ToString());

            Log.Error("ENDWEEK", TimeIntervalEnd.ToString());
            return true;
        }
        public bool SetMonthFrame()
        {
            int endMonth = 0;
            int endYear = 0;
            try
            {
                //if (Convert.ToInt32(dateTxtMonth.Text) >= 12)
                //{
                //    endMonth = 1;
                //    endYear = Convert.ToInt32(dateTxtYear.Text) + 1;
                //}
                //else
                //{
                //    endMonth = Convert.ToInt32(dateTxtMonth.Text) + 1;
                //    endYear = Convert.ToInt32(dateTxtYear.Text);
                //}

            }
            catch (Exception e)
            {
                Toast.MakeText(this, "OPPPS " + e.Message, ToastLength.Long);
                return false;
            }
            //TimeIntervalBegin = Convert.ToDateTime(string.Empty + dateTxtMonth.Text + "/1/" + dateTxtYear.Text).Date;
            //TimeIntervalEnd = Convert.ToDateTime(string.Empty + endMonth + "/1/" + endYear).Date;
            return true;
        }
        public bool SetMonthFrame(DatePicker DatePicker)
        {
            try
            {
                int endMonth = 0;
                int endYear = 0;
                if (DatePicker.Month == 12)
                {
                    endMonth = 1;
                    endYear = DatePicker.Year + 1;
                }
                else
                {
                    endMonth = DatePicker.Month + 1;
                    endYear = DatePicker.Year;
                }
                TimeIntervalBegin = Convert.ToDateTime(string.Empty + DatePicker.Month + "/1/" + DatePicker.Year);
                TimeIntervalEnd = Convert.ToDateTime(string.Empty + endMonth + "/1/" + endYear);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        private float GetTotalHoursForTimePeriod()
        {
            float Hours = 0;
            object[] args = { true, TimeIntervalBegin, TimeIntervalEnd };
            using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
            {
                Hours = connection.Table<WorkInstance>().Where(x => x.IsValid && x.Date >= TimeIntervalBegin && x.Date < TimeIntervalEnd).Sum(x => x.getTotalHours());
                //List<WorkInstance> dt = connection.Query<WorkInstance>("SELECT * FROM WorkInstance where IsValid = ? AND Date >= ? AND Date < ?", args).ToList();
            }
            return Hours;
        }

    }
}