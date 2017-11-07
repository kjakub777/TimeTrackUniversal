using System;
using System.Linq;
using SQLite;

namespace TimeTrackerUniversal.Database.Schema
{

    public class EmailAddresses : IDatabase
    {       /**********************/
        [PrimaryKey, AutoIncrement]
        public int Oid { get; set; }

        [NotNull]
        public bool IsValid { get; set; }
        //1=to,2=from,3=BCC
        public int EmailType { get; set; }

        [Ignore]
        public string InDatabase { get; set; }
        /**********************/
        public DateTime Date { get; set; }

        [NotNull]
        public string Email { get; set; }

        public override string ToString()
        {
            return $"Oid={Oid.ToString()},Date={Date.ToString()}, Email={Email}";
        }

        public EmailAddresses() : base() { }
    }

}