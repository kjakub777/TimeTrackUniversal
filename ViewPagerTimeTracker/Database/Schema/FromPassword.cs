
using SQLite;
using System;
using System.Linq; 

namespace TimeTrackerUniversal.Database.Schema
{

	public class FromPassword :IDatabase
	{       /**********************/
		[PrimaryKey, AutoIncrement]
		public int Oid { get; set; }

		[NotNull]
		public bool IsValid { get; set; }

		[Ignore]
		public string InDatabase { get; set; }
		/**********************/
		public DateTime Date { get; set; }

		[NotNull]
		public string Pass { get; set; }

		public override string ToString()
		{
			return $"Oid={Oid.ToString()},Date={Date.ToString()}, PW={Pass}";
		}

		public FromPassword() : base() { }
	}
}