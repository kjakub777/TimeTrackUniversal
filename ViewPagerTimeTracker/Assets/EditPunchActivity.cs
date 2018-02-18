using Android.App;
using Android.OS;

using Android.Widget;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using TimeTrackerUniversal.Assets;
using TimeTrackerUniversal.Database;
using TimeTrackerUniversal.Database.Schema;
using Android.Views;
using Android.Content;
using Android.Util;

namespace TimeTrackerUniversal
{
    [Activity(Label = "Edit Punch", MainLauncher = false, Icon = "@drawable/icon")]
    public class EditPunchActivity : Activity
    {
        public int deleteButtonPresses { get; set; }
        ListView workInstanceList;
        Dialog dlgFinding;

        public EditText txtOid { get; private set; }
        public EditText txtDate { get; private set; }
        public EditText txtClockIn { get; private set; }
        public EditText txtClockOut { get; private set; }
        public EditText txtRate { get; private set; }
        public EditText txtHours { get; private set; }

        public EditPunchActivity()
        {


        }



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.EditRecord);

            workInstanceList = FindViewById<ListView>(Resource.Id.workInstanceList);
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
            {
                var v = connection.Table<WorkInstance>().OrderByDescending(x => x.Oid).ToArray();
                WorkInstanceAdapter adapter = new WorkInstanceAdapter(this, v);

                //         Android.Widget.Toast.MakeText(this, $"count { adapter.Count} class { adapter.Class}", Android.Widget.ToastLength.Short).Show();

                workInstanceList.Adapter = adapter;
            }
            deleteButtonPresses = 0;
            workInstanceList.FastScrollEnabled = true;
            workInstanceList.ItemClick += WorkInstanceList_Click;
            workInstanceList.ItemLongClick += WorkInstanceList_LongClick;

            // PopulateWorkInstanceList();
        }



        private void WorkInstanceList_LongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            deleteButtonPresses = 0;

            //var t = (TextView)sender;
            long oid = e.Id; //t.Text.Substring(0, t.Text.IndexOf(" |"));
                             //  Android.Widget.Toast.MakeText(this, $"Oid {oid}", Android.Widget.ToastLength.Long).Show();


            dlgFinding = new Dialog(this);
            dlgFinding.SetContentView(Resource.Layout.EditDialog);
            //@+id/txtOid
            //@+id/txtDate
            //@+id/txtClockIn
            //@+id/txtClockOut
            //@+id/txtRate
            //@+id/txtHours
            //@+id/btnSave
            //@+id/btnOk
            //spinner

            txtOid = dlgFinding.FindViewById<EditText>(Resource.Id.txtOid);
            txtDate = dlgFinding.FindViewById<EditText>(Resource.Id.txtDate);
            txtClockIn = dlgFinding.FindViewById<EditText>(Resource.Id.txtClockIn);
            txtClockOut = dlgFinding.FindViewById<EditText>(Resource.Id.txtClockOut);
            txtRate = dlgFinding.FindViewById<EditText>(Resource.Id.txtRate);
            txtHours = dlgFinding.FindViewById<EditText>(Resource.Id.txtHours);
            dlgFinding.SetTitle("Work Instance");
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
            {
                try
                {
                    var v = connection.Table<WorkInstance>().FirstOrDefault(x => x.Oid == oid);// OrderByDescending(x => x.Oid).ToArray();
                    txtOid.Text = v.Oid.ToString();
                    txtDate.Text = "" + v.Date;
                    txtClockIn.Text = v.ClockIn.ToString();
                    txtClockOut.Text = "" + v.ClockOut;
                    txtRate.Text = "" + v.HourlyRate;
                    txtHours.Text = "" + v.getTotalHours();
                    dlgFinding.SetTitle("Work Instance");

                    Android.Widget.Toast.MakeText(this, $"POP wi???", Android.Widget.ToastLength.Long).Show();

                }
                catch (Exception ex)
                {
                    Android.Widget.Toast.MakeText(this, $"EX {ex}", Android.Widget.ToastLength.Long).Show();
                    return;
                    ;
                }
            }


            System.Collections.ArrayList alFindings = new System.Collections.ArrayList();

            Button btnOk = dlgFinding.FindViewById<Button>(Resource.Id.btnOk);
            Button btnSave = dlgFinding.FindViewById<Button>(Resource.Id.btnSave);
            Button btnDelete = dlgFinding.FindViewById<Button>(Resource.Id.btnDelete);
            TextView txtOut = dlgFinding.FindViewById<TextView>(Resource.Id.txtOut);

            btnOk.Click += Dialog_btnOk_Click;
            btnDelete.Click += Dialog_btnDelete_Click ;
            btnSave.Click += Dialog_btnSave_Click;
            
            //new EventHandler(this.cmdOKFinding_Click);

            dlgFinding.Show();

            //dbcon.Close();
            //var itempos = workInstanceList.SelectedItemPosition;//as WorkInstance;
            // var wi = workInstanceList.g
        }

        private void Dialog_btnSave_Click(object sender, EventArgs e)
        {
            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
            {
                try
                {
                    long oidint = Convert.ToInt32(txtOid.Text);
                    var v = connection.Table<WorkInstance>().FirstOrDefault(x => x.Oid == oidint);
                    if (v != null)
                    {
                        Android.Widget.Toast.MakeText(this, $"{v}", Android.Widget.ToastLength.Long).Show();
                        if (string.IsNullOrWhiteSpace(txtDate.Text))
                        {//check if i need to delete
                            connection.Delete(v);
                        }
                        else
                        {
                            v.Date = Convert.ToDateTime(txtDate.Text);
                            v.ClockIn = Convert.ToDateTime(txtClockIn.Text);
                            v.ClockOut = Convert.ToDateTime(txtClockOut.Text);
                            v.HourlyRate = (float)Convert.ToDecimal(txtRate.Text);
                            //   txtHours.Text = "" + v.getTotalHours();
                            dlgFinding.SetTitle("Work Instance");

                            connection.Update(v);// Commit();
                        }
                        // oLayoutChange 
                    }
                    dlgFinding.OnBackPressed();
                    ((WorkInstanceAdapter)workInstanceList.Adapter).Update();// workInstanceList.RefreshDrawableState();

                    return;
                }
                catch (Exception ex)
                {
                    Android.Widget.Toast.MakeText(this, $"EX {ex}", Android.Widget.ToastLength.Long).Show();
                    dlgFinding.OnBackPressed();
                    return;
                }
            }
        }
            

        private void Dialog_btnDelete_Click(object sender, EventArgs e)
        {
            if (deleteButtonPresses++ > 2)
            {
                using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                {
                    try
                    {
                        long oidint = Convert.ToInt32(txtOid.Text);
                        var v = connection.Table<WorkInstance>().FirstOrDefault(x => x.Oid == oidint);
                        Android.Widget.Toast.MakeText(this, $"{v}", Android.Widget.ToastLength.Long).Show();
                        if (v != null)
                            connection.Delete(v);

                        dlgFinding.OnBackPressed();
                        ((WorkInstanceAdapter)workInstanceList.Adapter).Update();  // Invalidate();

                        return;
                    }
                    catch (Exception ex)
                    {
                        Android.Widget.Toast.MakeText(this, $"EX {ex}", Android.Widget.ToastLength.Long).Show();
                        dlgFinding.OnBackPressed();
                        return;
                    }
                }
            }
            else
            {
                TextView txtOut = dlgFinding.FindViewById<TextView>(Resource.Id.txtOut);
                txtOut.Text = $"Are you sure you want to delete? Press again ...  {deleteButtonPresses}";
            }           
        }

        private void Dialog_btnOk_Click(object sender, EventArgs e)
        {
            dlgFinding.OnBackPressed(); 
        }

        //  public void Dialog_btnOk_Click
        private void WorkInstanceList_Click(object sender, AdapterView.ItemClickEventArgs e)
        {

            try
            {
                long oid = e.Id;

                using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                {
                    try
                    {
                        var v = connection.Table<WorkInstance>().FirstOrDefault(x => x.Oid == oid);// OrderByDescending(x => x.Oid).ToArray();
                        Android.Widget.Toast.MakeText(this, v.ToString(), Android.Widget.ToastLength.Long).Show();
                    }
                    catch { }
                }


            }
            catch (Exception ex)
            {

                Android.Widget.Toast.MakeText(this, ex.ToString(), Android.Widget.ToastLength.Long).Show(); ;
            }
        }




    }
}