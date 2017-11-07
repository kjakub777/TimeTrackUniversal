using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite; 
using Android.App;
using Android.Content;
using Android.OS;

namespace TimeTrackerUniversal.Database.Schema
{
	public class BaseObject
	{
		public BaseObject() { }

		[PrimaryKey, AutoIncrement]
		public int Oid { get; set; }

		[NotNull]
		public bool IsValid { get; set; }

		[Ignore]
		public string InDatabase { get; set; }
		public virtual string GetRowString()
		{
			return $"Oid = {Oid} | IsValid = {(IsValid ? "Yes" : "No")} | ";
		}
	}
}