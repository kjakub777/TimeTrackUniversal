//using Android.App;
//using Android.OS;

//using Android.Widget;
//using SQLite;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using TimeTrackerUniversal.Assets;
//using TimeTrackerUniversal.Database;
//using TimeTrackerUniversal.Database.Schema;
//using Android.Views;
//using Android.Content;
//using Android.Util;

//namespace TimeTrackerUniversal
//{
//    [Activity(Label = "Edit Punch", MainLauncher = false, Icon = "@drawable/icon")]
//    public class EditPunchActivity : Activity
//    {
//        Button btnExitPass;
//        Button btnPunch;
//        EditText dateDate;
//        ListView workInstanceList;
//        CheckBox chkForce;
//        CheckBox IsClockOut;
//        CheckBox IsClockIn;
//        TextView punchOut;
//        TimePicker timePickIn;
//        TimePicker timePickOut;
//        private List<KeyValuePair<string, string>> workInstanceKeyList;
//        Dialog dlgFinding;

//        public EditText txtOid { get; private set; }
//        public EditText txtDate { get; private set; }
//        public EditText txtClockIn { get; private set; }
//        public EditText txtClockOut { get; private set; }
//        public EditText txtRate { get; private set; }
//        public EditText txtHours { get; private set; }

//        public EditPunchActivity()
//        {


//        }

//        private void btnExitPass_Click(object sender, EventArgs e)
//        {
//            SetResult(Result.Ok);
//            this.OnBackPressed();
//        }

//        private void btnPunch_Click(object sender, EventArgs e)
//        {
//            punchOut.Text = $"Clocking...";
//            DateTime date;
//            try
//            {
//                using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
//                {
//                    if (DateTime.TryParse(dateDate.Text.ToString(), out date) && (IsClockIn.Checked || IsClockOut.Checked))
//                    {
//                        //makesure valid punch
//                        WorkInstance wi = connection.Table<WorkInstance>().Last();
//                        if (IsClockIn.Checked)
//                        {

//                            if ((wi.ClockIn != wi.ClockOut) || chkForce.Checked)
//                            {
//                                //System.Globalization.Calendar.CurrentEra;                                                                    //String.Format("{0:HH:mm:ss}", wi.ClockIn)
//                                DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, timePickIn.Hour, timePickIn.Minute, 0, 0, DateTimeKind.Local);

//                                DateTime timein = MainActivity.GetLocalTime(dateTime);
//                                dateTime = new DateTime(date.Year, date.Month, date.Day, timePickOut.Hour, timePickOut.Minute, 0, 0, DateTimeKind.Local);
//                                DateTime timeout = MainActivity.GetLocalTime(dateTime);
//                                var hourlyRate = (connection.Table<HourlyRate>().Last()).Rate;
//                                connection.Insert(new WorkInstance()
//                                {
//                                    Date = date,
//                                    IsValid = true,
//                                    ClockIn = timein,
//                                    ClockOut = IsClockOut.Checked ? timeout : timein,
//                                    HourlyRate = hourlyRate,
//                                });
//                                punchOut.Text = $"You are clocked!";
//                            }
//                            else
//                            {
//                                punchOut.Text = $"You still need to clock out from last clock in {wi.ClockIn}";
//                            }
//                        }
//                        else if (IsClockOut.Checked)
//                        {
//                            if (wi.ClockIn == wi.ClockOut || chkForce.Checked)
//                            {
//                                DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, timePickOut.Hour, timePickOut.Minute, 0, 0, DateTimeKind.Local);
//                                DateTime timeout = MainActivity.GetLocalTime(dateTime);

//                                wi.ClockOut = timeout;
//                                connection.Update(wi);
//                                punchOut.Text = $"You are clocked!";
//                            }
//                            else
//                            {
//                                punchOut.Text = $"You are not clocked in";
//                            }
//                        }

//                    }
//                }
//            }
//            catch (Exception Ex)
//            {

//                punchOut.Text = Ex.Message;
//            }
//            punchOut.Text += "  ||";
//        }

//        protected override void OnCreate(Bundle savedInstanceState)
//        {
//            base.OnCreate(savedInstanceState);
//            SetContentView(Resource.Layout.EditRecord);

//            workInstanceList = FindViewById<ListView>(Resource.Id.workInstanceList);
//            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
//            {
//                var v = connection.Table<WorkInstance>().OrderByDescending(x => x.Oid).ToArray();
//                WorkInstanceAdapter adapter = new WorkInstanceAdapter(this, v);

//                Android.Widget.Toast.MakeText(this, $"count { adapter.Count} class { adapter.Class}", Android.Widget.ToastLength.Short).Show();

//                workInstanceList.Adapter = adapter;
//            }
//            //  workInstanceList.
//            ///  var adapter = new ArrayAdapter<string>(this,
//            // Android.Resource.Layout.SimpleListItem1, planetNames); 
//            //btnExitPass = FindViewById<Button>(Resource.Id.btnExitPass);
//            //btnPunch = FindViewById<Button>(Resource.Id.btnPunch);
//            //dateDate = FindViewById<EditText>(Resource.Id.dateDate);
//            //punchOut = FindViewById<TextView>(Resource.Id.punchOut);
//            //timePickIn = FindViewById<TimePicker>(Resource.Id.timePickIn);
//            //timePickOut = FindViewById<TimePicker>(Resource.Id.timePickOut);
//            //IsClockIn = FindViewById<CheckBox>(Resource.Id.IsClockIn);
//            //chkForce = FindViewById<CheckBox>(Resource.Id.chkForce);
//            //IsClockOut = FindViewById<CheckBox>(Resource.Id.IsClockOut);

//            //DateTime dt = MainActivity.GetLocalTime();//.Month
//            //dateDate.Text = $"{dt.Month}/{dt.Day}/{dt.Year}";
//            //Java.Lang.Boolean b = Java.Lang.Boolean.True;
//            ////timePickIn.SetIs24HourView(b);
//            ////timePickOut.SetIs24HourView(b);
//            //btnPunch.Click += btnPunch_Click;
//            //btnExitPass.Click += btnExitPass_Click;

//            workInstanceList.FastScrollEnabled = true;
//            workInstanceList.ItemClick += WorkInstanceList_Click;
//            workInstanceList.ItemLongClick += WorkInstanceList_LongClick;

//            // PopulateWorkInstanceList();
//        }



//        private void WorkInstanceList_LongClick(object sender, AdapterView.ItemLongClickEventArgs e)
//        {


//            //var t = (TextView)sender;
//            //t.Text = "Iv been clicked";

//            dlgFinding = new Dialog(this);
//            dlgFinding.SetContentView(Resource.Layout.EditDialog);
//            //@+id/txtOid
//            //@+id/txtDate
//            //@+id/txtClockIn
//            //@+id/txtClockOut
//            //@+id/txtRate
//            //@+id/txtHours
//            //@+id/btnSave
//            //@+id/btnOk
//            //spinner

//            txtOid = dlgFinding.FindViewById<EditText>(Resource.Id.txtOid);
//            txtDate = dlgFinding.FindViewById<EditText>(Resource.Id.txtDate);
//            txtClockIn = dlgFinding.FindViewById<EditText>(Resource.Id.txtClockIn);
//            txtClockOut = dlgFinding.FindViewById<EditText>(Resource.Id.txtClockOut);
//            txtRate = dlgFinding.FindViewById<EditText>(Resource.Id.txtRate);
//            txtHours = dlgFinding.FindViewById<EditText>(Resource.Id.txtHours);
//            dlgFinding.SetTitle("Work Instance");
//            using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
//            {
//                var v = connection.Table<WorkInstance>().Last();// OrderByDescending(x => x.Oid).ToArray();
//                txtOid.Text = v.Oid.ToString();
//                txtDate.Text = ""+v.Date;
//                txtClockIn.Text = v.ClockIn.ToString();
//                txtClockOut.Text = ""+v.ClockOut;
//                txtRate.Text = ""+v.HourlyRate;
//                txtHours.Text = "" + v.getTotalHours();
//                dlgFinding.SetTitle("Work Instance");

//                Android.Widget.Toast.MakeText(this, $"POP wi???", Android.Widget.ToastLength.Long).Show();
                 
//            }

//            //txtAssetGroupILIUI.Text = strAssetGroup;
//            //txtAssetILIUI.Text = strAsset;
//            //txtInspectionDateILIUI.Text = strInspectionDate;
//            //txtFindingILIUI.Text = strFinding;
//            //txtCommentsILIUI.Text = strComments;
//            //txtWorkOrderILIUI.Text = strWorkOrder;
//            //txtDecommissionILIUI.Text = strDescommission;
//            //SQLite.SQLiteConnection dbcon;

//            //AimProvider conDBProvider;
//            //conDBProvider = new AimProvider();
//            //dbcon = conDBProvider.OpenConnection();

//            //var query = dbcon.Table<Finding>();

//            System.Collections.ArrayList alFindings = new System.Collections.ArrayList();

//            //foreach (var f in query)
//            //{
//            // //   alFindings.Add(new SpinnerItem(f.FindingID, f.FindingDescription));
//            //}

//            //ArrayAdapter myAdapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, alFindings);

//            //myAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
//            //dlgFinding.FindViewById<Spinner>(Resource.Id.cboFindingsFDUI).Adapter = myAdapter;

//            //Button cmdOKFinding = dlgFinding.FindViewById<Button>(Resource.Id.cmdOKFDUI);
//            //cmdOKFinding.Click += new EventHandler(this.cmdOKFinding_Click);

//            dlgFinding.Show();

//            //dbcon.Close();
//            //var itempos = workInstanceList.SelectedItemPosition;//as WorkInstance;
//            // var wi = workInstanceList.g
//        }

//        private void WorkInstanceList_Click(object sender, EventArgs e)
//        {

//            try
//            {
//                Android.Widget.Toast.MakeText(this, $"sender {sender}", Android.Widget.ToastLength.Long).Show();
//                var props = e.GetType().GetProperties();
//                string s = "";
//                foreach (var i in props)
//                {
//                    s += $"FIeld {i}\n";
//                    Log.Info("FIELDS****", i + "\n");
//                }
//                Android.Widget.Toast.MakeText(this, s, Android.Widget.ToastLength.Long).Show();

//                var item = workInstanceList.SelectedItem;//as WorkInstance;
//                var fields = item.GetType().GetFields();
//                long Oid = 0;
//                  s = "";
//                foreach (var i in fields)
//                {
//                    s += $"FIeld {i}\n";
//                    Log.Info("FIELDS****", i + "\n");
//                }
//                Android.Widget.Toast.MakeText(this, s, Android.Widget.ToastLength.Long).Show();

//                Log.Info("FIELDS****", item.Class.ToString() + "\n");
//                Android.Widget.Toast.MakeText(this, item.GetType().GetMembers().ToString(), Android.Widget.ToastLength.Long).Show();
//                var wi = item.Class.Cast(item);

//                Android.Widget.Toast.MakeText(this, wi.ToString(), Android.Widget.ToastLength.Long).Show();

//            }
//            catch (Exception ex)
//            {

//                Android.Widget.Toast.MakeText(this, ex.ToString(), Android.Widget.ToastLength.Long).Show(); ;
//            }
//        }

//        private void PopulateWorkInstanceList()
//        {

//            //try
//            //{
//            //    using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
//            //    {
//            //        MainActivity.initRate = connection.Table<HourlyRate>().Any() ? connection.Table<HourlyRate>().Last().Rate : 0;


//            //        txtLastPunch.Text = v != null ? v.ClockIn == v.ClockOut ? $"In {v.ClockIn.ToString()}" : $"Out {v.ClockOut.ToString()}" : "NULL";

//            //    }
//            //}
//            //catch (Exception Ex)
//            //{
//            //    txtMainOut.Text = $"ERR {Ex.Message}";
//            //}
//        }
//        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
//        {
//            Spinner spinner = (Spinner)sender;
//            string toast = string.Format("The mean temperature for planet {0} is {1}",
//                spinner.GetItemAtPosition(e.Position), workInstanceKeyList[e.Position].Value);
//            Toast.MakeText(this, toast, ToastLength.Long).Show();
//        }
//        //private class ViewHolderEditDialog : Java.Lang.Object
//        //{
//        //    Activity context;
//        //    Dialog dlgFinding;
//        //    TextView txtOid;
//        //    TextView txtDate;
//        //    TextView txtClockIn;
//        //    TextView txtClockOut;
//        //    TextView txtRate;
//        //    TextView txtHours; 
//        //    //    TextView txtDecommissionILIUI;

//        //    // this method now handles getting references to our subviews
//        //    public void Initialize(View view)
//        //    {
//        //        txtOid = view.FindViewById<TextView>(Resource.Id.txtOid);
//        //        txtDate = view.FindViewById<TextView>(Resource.Id.txtDate);
//        //        txtClockIn = view.FindViewById<TextView>(Resource.Id.txtClockIn);
//        //        txtClockOut = view.FindViewById<TextView>(Resource.Id.txtClockOut);
//        //        txtRate = view.FindViewById<TextView>(Resource.Id.txtRate);
//        //        txtHours = view.FindViewById<TextView>(Resource.Id.txtHours);
//        //     //   txtDecommissionILIUI = view.FindViewById<TextView>(Resource.Id.txtDecommissionILIUI);

//        //        txtClockOut.Click += new EventHandler(this.txtFindings_Click);
//        //    }

//        //    // this method now handles binding data
//        //    public void Bind(Activity myContext, string strAssetGroup, string strAsset, string strInspectionDate, string strFinding, string strComments,
//        //        string strWorkOrder, string strDescommission)
//        //    {

//        //        context = myContext;

//        //        txtOid.Text = strAssetGroup;
//        //        txtDate.Text = strAsset;
//        //        txtClockIn.Text = strInspectionDate;
//        //        txtClockOut.Text = strFinding;
//        //        txtRate.Text = strComments;
//        //        txtHours.Text = strWorkOrder;

//        //    }

//        //    void txtFindings_Click(Object sender,
//        //              EventArgs e)
//        //    {
//        //        var t = (TextView)sender;
//        //        t.Text = "Iv been clicked";

//        //        dlgFinding = new Dialog(context);
//        //        dlgFinding.SetContentView(Resource.Layout.EditDialog);
//        //        dlgFinding.SetTitle("Select Finding");

//        //        using (SQLiteConnectionWithLock connection = SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
//        //        {
//        //            var v = connection.Table<WorkInstance>().OrderByDescending(x => x.ClockIn).ToArray();
//        //            WorkInstanceAdapter adapter = new WorkInstanceAdapter(this, v);

//        //            workInstanceList.Adapter = adapter;
//        //        }
//        //        foreach (var f in query)
//        //        {
//        //            alFindings.Add(new SpinnerItem(f.FindingID, f.FindingDescription));
//        //        }

//        //        AimProvider conDBProvider;
//        //        conDBProvider = new AimProvider();
//        //        dbcon = conDBProvider.OpenConnection();

//        //        var query = dbcon.Table<Finding>();

//        //        System.Collections.ArrayList alFindings = new System.Collections.ArrayList();

//        //        foreach (var f in query)
//        //        {
//        //            alFindings.Add(new SpinnerItem(f.FindingID, f.FindingDescription));
//        //        }

//        //        ArrayAdapter myAdapter = new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerItem, alFindings);

//        //        myAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
//        //        dlgFinding.FindViewById<Spinner>(Resource.Id.cboFindingsFDUI).Adapter = myAdapter;

//        //        Button cmdOKFinding = dlgFinding.FindViewById<Button>(Resource.Id.cmdOKFDUI);
//        //        cmdOKFinding.Click += new EventHandler(this.cmdOKFinding_Click);

//        //        dlgFinding.Show();

//        //        dbcon.Close();
//        //    }

//        //    void cmdOKFinding_Click(Object sender,
//        //                EventArgs e)
//        //    {
//        //        //Get the currently selected finding                
//        //        txtFindingILIUI.Text = Convert.ToString(dlgFinding.FindViewById<ListView>(Resource.Id.cboF).SelectedItem);
//        //        dlgFinding.Dismiss();

//        //        //Update Adapter and database here TO-DO
//        //    }
//        // }


//    }
//}