using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using TimeTrackerUniversal.Database;
using TimeTrackerUniversal.Database.Schema;
using SQLite;

namespace TimeTrackerUniversal
{
    [Activity(Label = "Run SQL", MainLauncher = false, Icon = "@drawable/icon")]
    public class RunSQLActivity : Activity
    {
        EditText sqlQuery;
        TextView txtOutput;
        Button btnExecuteSql;//sqlQuery btnExecuteSql 
        Button btnExit;//sqlQuery btnExecuteSql 


        public RunSQLActivity()
        {


        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RunSQL);

            btnExecuteSql = FindViewById<Button>(Resource.Id.btnExecuteSql);
            btnExit = FindViewById<Button>(Resource.Id.btnExit);
            sqlQuery = FindViewById<EditText>(Resource.Id.sqlQuery);
            txtOutput = FindViewById<TextView>(Resource.Id.txtOutput);

            DateTime dt = DateTime.Now.ToLocalTime();//.Month
            Java.Lang.Boolean b = Java.Lang.Boolean.True;

            btnExecuteSql.Click += btnExecuteSql_Click;
            btnExit.Click += btnExit_Click;

        }

        private void btnExecuteSql_Click(object sender, EventArgs e)
        {

            using (SQLiteConnection connection = SqlConnectionFactory.GetSQLiteConnectionWithLock())
            {
                int res = connection.Execute(sqlQuery.Text);
                txtOutput.Text += " || > " + res;
            }
        }



        private void btnExit_Click(object sender, EventArgs e)
        {
            SetResult(Result.Ok);
            this.OnBackPressed();
        }
    }
}