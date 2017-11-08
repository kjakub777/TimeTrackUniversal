using Android.App;
using Android.OS;

using Android.Widget;
using SQLite;
using System;
using System.Linq;
using TimeTrackerUniversal.Database;

namespace TimeTrackerUniversal
{
    [Activity(Label = "Run SQL", MainLauncher = false, Icon = "@drawable/icon")]
    public class RunSQLActivity : Activity
    {
        Button btnExecuteSql;//sqlQuery btnExecuteSql 
        Button btnExit;//sqlQuery btnExecuteSql 
        TextView txtOutput;
        EditText txtSqlQuery;


        public RunSQLActivity()
        {


        }

        private void btnExecuteSql_Click(object sender, EventArgs e)
        {

            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
            {
                try
                {
                    int res = connection.Execute(txtSqlQuery.Text);
                    txtOutput.Text += " || > " + res;
                }
                catch (Exception x)
                {

                    Toast.MakeText(ApplicationContext, "SQL Exception, don't try to selct anything!" + x.Message, ToastLength.Long).Show(); ;
                }
            }
        }



        private void btnExit_Click(object sender, EventArgs e)
        {
            SetResult(Result.Ok);
            this.OnBackPressed();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.RunSQL);

            btnExecuteSql = FindViewById<Button>(Resource.Id.btnExecuteSql);
            btnExit = FindViewById<Button>(Resource.Id.btnExit);
            txtSqlQuery = FindViewById<EditText>(Resource.Id.txtSqlQuery);
            txtOutput = FindViewById<TextView>(Resource.Id.txtOutput);

            DateTime dt = DateTime.Now.ToLocalTime();//.Month
            Java.Lang.Boolean b = Java.Lang.Boolean.True;

            btnExecuteSql.Click += btnExecuteSql_Click;
            btnExit.Click += btnExit_Click;

        }
    }
}