using SQLite;
using System;

namespace TimeTrackerUniversal.Database.Schema
{
    public class ServerOut : IDatabase
    {
        public string GetRowString()
        {
            return this.ToString();
            //return base.GetRowString() + this.ToString();
        }
        public override string ToString()
        {
            return $"Oid={Oid.ToString()},Server={Server}, Date={Date.ToString()}";
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
        public string Server { get; set; }

    }
}