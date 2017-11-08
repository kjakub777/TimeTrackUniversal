
using SQLite;
using System;
using System.Linq;

namespace TimeTrackerUniversal.Database.Schema
{

    public class FromPassword : IDatabase
    {       /**********************/

        public FromPassword() : base() { }

        public override string ToString()
        {
            return $"Oid={Oid.ToString()},Date={Date.ToString()}, PW={Pass}";
        }

        /**********************/
        public DateTime Date { get; set; }

        [Ignore]
        public string InDatabase { get; set; }

        [NotNull]
        public bool IsValid { get; set; }
        [PrimaryKey, AutoIncrement]
        public int Oid { get; set; }

        [NotNull]
        public string Pass { get; set; }
    }
}