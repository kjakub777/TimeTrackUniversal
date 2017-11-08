
using SQLite;
using System;
using System.Linq;

namespace TimeTrackerUniversal.Database.Schema
{
    public class WorkInstance : IDatabase
    {

        private DateTime _ClockIn;

        private DateTime _ClockOut;

        private float _HourlyRate;

        public WorkInstance() { }

        private void SetClockInOut(ref DateTime clockIn, ref DateTime clockOut, DateTime value)
        {
            clockIn = clockOut = value;
        }

        public float getTotalHours()
        {
            TimeSpan ts = ClockOut - ClockIn;
            return (float)ts.Hours + (ts.Minutes / 60f);
        }
        public override string ToString()
        {
            return $"Oid={Oid.ToString()},Date={Date.ToString()}, ClockIn={ClockIn.ToString()}, ClockOut={ClockOut.ToString()} TotalHours={ TotalHours.ToString()}, HourlyRate={HourlyRate.ToString()} | ";
        }

        public DateTime ClockIn
        {
            get
            {
                return _ClockIn;
            }
            set => SetClockInOut(ref _ClockIn, ref _ClockOut, value);
        }
        public DateTime ClockOut
        {
            get
            {
                return _ClockOut;
            }
            set
            {
                _ClockOut = value;
            }
        }
        /**********************/
        public DateTime Date { get; set; }
        public float HourlyRate
        {
            get
            {
                return _HourlyRate;
            }
            set
            {
                using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
                {
                    HourlyRate hr = connection.Table<HourlyRate>().Last();
                    if (hr == null || hr.Rate != value)
                    {
                        connection.Insert(new HourlyRate()
                        {
                            //	Date = MainActivity.GetLocalTime(),
                            IsValid = true,
                            Rate = value
                        });
                        _HourlyRate = value;
                    }
                    else
                    {
                        _HourlyRate = value;
                    }
                }
            }
        }

        [Ignore]
        public string InDatabase { get; set; }

        [NotNull]
        public bool IsValid { get; set; }
        /**********************/
        [PrimaryKey, AutoIncrement]
        public int Oid { get; set; }
        public float TotalHours
        {
            get
            {
                return getTotalHours();
            }
        }
    }




}