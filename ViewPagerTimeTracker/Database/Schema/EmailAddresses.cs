using SQLite;
using System;
using System.Linq;

namespace TimeTrackerUniversal.Database.Schema
{

    public class EmailAddresses : IDatabase
    {       /**********************/

        public EmailAddresses() : base() { }

        public override string ToString()
        {
            return $"Oid={Oid.ToString()},Date={Date.ToString()}, Email={Email}";
        }

        /**********************/
        public DateTime Date { get; set; }

        [NotNull]
        public string Email { get; set; }
        //1=to,2=from,3=BCC
        public int EmailType { get; set; }

        [Ignore]
        public string InDatabase { get; set; }

        [NotNull]
        public bool IsValid { get; set; }
        [PrimaryKey, AutoIncrement]
        public int Oid { get; set; }
    }

}