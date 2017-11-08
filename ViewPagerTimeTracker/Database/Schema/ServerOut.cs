using System;
using SQLite;

namespace TimeTrackerUniversal.Database.Schema
{
	public class ServerOut : IDatabase
	{
		/**********************/
		[PrimaryKey, AutoIncrement]
		public int Oid { get; set; }

		[NotNull]
		public bool IsValid { get; set; }

		[Ignore]
		public string InDatabase { get; set; }
		/**********************/
		public string Server{ get; set; }
		public DateTime Date { get; set; }
		public   string GetRowString()
		{
			return  this.ToString();
			//return base.GetRowString() + this.ToString();
		}
		public  override string ToString()
		{
			return $"Oid={Oid.ToString()},Server={Server}, Date={Date.ToString()}";
		}
		 
	}
}