using Android.App;
using Android.OS;

using Android.Widget;
using SQLite;
using System;
using System.IO;
using System.Linq;
using TimeTrackerUniversal.Database;
using TimeTrackerUniversal.Database.Schema;

namespace TimeTrackerUniversal
{
    [Activity(Label = "Run SQL", MainLauncher = false, Icon = "@drawable/icon")]
    public class RunSQLActivity : Activity
    {
        Button btnExecuteSql;//sqlQuery btnExecuteSql 
        Button btnExit;//sqlQuery btnExecuteSql 
        TextView txtOutput;
        EditText txtSqlQuery;
        EditText txtFile;


        public RunSQLActivity()
        {


        }

        private void btnExecuteSql_Click(object sender, EventArgs e)
        {
            string query = "";
            if (!string.IsNullOrWhiteSpace(txtSqlQuery.Text))
            {
                query = txtSqlQuery.Text;
                using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                {
                    try
                    {
                        int res = connection.Execute(query);
                        txtOutput.Text += " || > " + res;
                    }
                    catch (Exception x)
                    {

                        Toast.MakeText(ApplicationContext, "SQL Exception, don't try to selct anything!" + x.Message, ToastLength.Long).Show(); ;
                    }
                }

            }
            if (!string.IsNullOrWhiteSpace(txtFile.Text))
            {
                string[] inserts = null;
                try
                {
                     inserts = File.ReadAllLines(Path.Combine("/sdcard/", txtFile.Text));
                }
                catch (Exception x)
                {
                    Toast.MakeText(ApplicationContext, "Exception reading in inserts!"+x.ToString(), ToastLength.Long).Show();
                    return;
                }
                using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                {

                    try
                    {
                        int c;
                        c = connection.CreateTable<HourlyRate>(CreateFlags.AutoIncPK);
                        c = connection.CreateTable<WorkInstance>(SQLite.CreateFlags.AutoIncPK);
                        c = connection.CreateTable<FromPassword>(SQLite.CreateFlags.AutoIncPK);
                        c = connection.CreateTable<ServerOut>(SQLite.CreateFlags.AutoIncPK);
                        c = connection.CreateTable<EmailAddresses>(SQLite.CreateFlags.AutoIncPK);
                        if (connection.Table<HourlyRate>().Count() > 0)
                        {
                            c = connection.DropTable<HourlyRate>();
                            c = connection.CreateTable<HourlyRate>(CreateFlags.AutoIncPK);
                        }
                        if (connection.Table<WorkInstance>().Count() > 0)
                        {
                            c = connection.DropTable<WorkInstance>();
                            c = connection.CreateTable<WorkInstance>(CreateFlags.AutoIncPK);
                        }
                        if (connection.Table<FromPassword>().Count() > 0)
                        {
                            c = connection.DropTable<FromPassword>();
                            c = connection.CreateTable<FromPassword>(CreateFlags.AutoIncPK);
                        }
                        if (connection.Table<ServerOut>().Count() > 0)
                        {
                            c = connection.DropTable<ServerOut>();
                            c = connection.CreateTable<ServerOut>(CreateFlags.AutoIncPK);
                        }
                        if (connection.Table<EmailAddresses>().Count() > 0)
                        {
                            c = connection.DropTable<EmailAddresses>();
                            c = connection.CreateTable<EmailAddresses>(CreateFlags.AutoIncPK);
                        }


                        connection.BeginTransaction();
                        foreach (var item in inserts)
                        {
                            if (!string.IsNullOrWhiteSpace(item))
                            {
                                int res = connection.Execute(item);
                                txtOutput.Text += " || > " + res;
                            }
                        }
                        connection.Commit();
                    }
                    catch (Exception x)
                    {

                        Toast.MakeText(ApplicationContext, "SQL Exception, don't try to selct anything!" + x.Message, ToastLength.Long).Show(); ;
                    }
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
            txtFile = FindViewById<EditText>(Resource.Id.txtFile);
            txtOutput = FindViewById<TextView>(Resource.Id.txtOutput);

            DateTime dt = DateTime.Now.ToLocalTime();//.Month
            Java.Lang.Boolean b = Java.Lang.Boolean.True;

            btnExecuteSql.Click += btnExecuteSql_Click;
            btnExit.Click += btnExit_Click;

        }
    }
}