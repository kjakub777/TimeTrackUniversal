using SQLite;
using System;

namespace TimeTrackerUniversal.Database.Schema
{
    public class HourlyRate : IDatabase
    {
        public HourlyRate() { }
        public HourlyRate(float rate) : base()
        {
            Rate = rate;
        }

        public string GetRowString()
        {
            return this.ToString();
            //return base.GetRowString() + this.ToString();
        }
        public override string ToString()
        {
            return $"Oid={Oid.ToString()},Rate={Rate.ToString()}, Date={Date.ToString()}";
        }

        public DateTime Date { get; set; }

        [Ignore]
        public string InDatabase { get; set; }

        [NotNull]
        public bool IsValid { get; set; }
        /**********************/
        [PrimaryKey, AutoIncrement]
        public int Oid { get; set; }
        /**********************/
        public float Rate { get; set; }
    }
}