using System;
using SQLite;

namespace TimeTrackerUniversal.Database.Schema
{
	public class HourlyRate : IDatabase
	{
		/**********************/
		[PrimaryKey, AutoIncrement]
		public int Oid { get; set; }

		[NotNull]
		public bool IsValid { get; set; }

		[Ignore]
		public string InDatabase { get; set; }
		/**********************/
		public float Rate { get; set; }
		public DateTime Date { get; set; }
		public   string GetRowString()
		{
			return  this.ToString();
			//return base.GetRowString() + this.ToString();
		}
		public  override string ToString()
		{
			return $"Oid={Oid.ToString()},Rate={Rate.ToString()}, Date={Date.ToString()}";
		}
		public HourlyRate(float rate):base()
		{
			Rate = rate;
		}
		public HourlyRate() { }
	}
}