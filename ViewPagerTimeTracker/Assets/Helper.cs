using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TimeTrackerUniversal.Database.Schema;
using TimeTrackerUniversal.Database;
using SQLite;
using Android.Util;

namespace TimeTrackerUniversal.Assets
{
    class Helper
    {

       

        public static bool SetWeekFrame(ref DateTime TimeIntervalBegin, ref DateTime TimeIntervalEnd)
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


        public static float GetTotalHoursForTimePeriod(DateTime TimeIntervalBegin,DateTime TimeIntervalEnd, ref string grossPayStr)
        {
            float gross = 0f;
            float Hours = 0f;
            object[] args = { true, TimeIntervalBegin, TimeIntervalEnd };
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
            {
                try
                {
                    List<WorkInstance> instances = connection.Table<WorkInstance>().Where(x => x.IsValid && x.Date >= TimeIntervalBegin.Date.Date && x.Date <= TimeIntervalEnd.Date.Date).Cast<WorkInstance>().ToList();
                    instances.ForEach((y) =>
                    {
                        var th = y.TotalHours;
                        Hours += th;
                        gross += th * y.HourlyRate;
                    });
                    // Hours = connection.Table<WorkInstance>().Where(x => x.IsValid && x.Date >= TimeIntervalBegin && x.Date < TimeIntervalEnd).Sum(x => x.getTotalHours());
                    // gross = connection.Table<WorkInstance>().Where(x => x.IsValid && x.Date >= TimeIntervalBegin && x.Date < TimeIntervalEnd).Sum(x => x.getTotalHours());

                    //List<WorkInstance> dt = connection.Query<WorkInstance>("SELECT * FROM WorkInstance where IsValid = ? AND Date >= ? AND Date < ?", args).ToList();
                    // txtGrossPay.Text = $"${gross}";
                    grossPayStr = string.Format("${0:f}", gross);
                }
                catch (Exception  )
                {
                  //  Toast.MakeText(ApplicationContext, "DB copied!!", ToastLength.Short).Show();
                }
            }
            return Hours;
        }
    }
}/*
                    var begin = TimeIntervalBegin.Ticks;// Math.Floor(.ToOADate());// *DateTime.;
                    var end = TimeIntervalEnd.Ticks;// Math.Floor(TimeIntervalEnd.ToOADate());
                    List<WorkInstance> instances = connection.Table<WorkInstance>().Where(x => x.IsValid && x.Date.Ticks >= begin && x.Date.Ticks <= end).Cast<WorkInstance>().ToList();
                    instances.ForEach((y) =>
                    {
                        var th = y.TotalHours;
                        Hours +=th;
                        gross += th * y.HourlyRate;
                    });
*/