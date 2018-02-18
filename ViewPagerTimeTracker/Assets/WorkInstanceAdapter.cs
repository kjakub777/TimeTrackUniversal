using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TimeTrackerUniversal.Database.Schema;

namespace TimeTrackerUniversal.Assets
{
    //public HomeScreenAdapter(Activity context, string[] items) : base() {
    //    this.context = context;
    //    this.items = items;
    //}


    //public override long GetItemId(int position)
    //{
    //    return position;
    //}
    //public override string this[int position]
    //{
    //    get { return items[position]; }
    //}
    //public override int Count
    //{
    //    get { return items.Length; }
    //}
    //public override View GetView(int position, View convertView, ViewGroup parent)
    //{
    //    View view = convertView; // re-use an existing view, if one is available
    //    if (view == null) // otherwise create a new one
    //        view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
    //    view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = items[position];
    //    return view;
    //}
    public class WorkInstanceAdapter : BaseAdapter<string>
    {
        WorkInstance[] items;
        Activity context;
        public WorkInstanceAdapter(Activity context, WorkInstance[] items) : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return items[position].Oid;
        }
        public override string this[int position]
        {
            get { return items[position].ToString(); }
        }
        public override int Count
        {
            get { return items.Length; }
        }
        public void Update( )
        {
            try
            {
                using (SQLite.SQLiteConnectionWithLock connection = Database.SqlConnectionFactory.GetSQLiteConnectionWithREALLock())
                {
                    this.items = connection.Table<WorkInstance>().OrderByDescending(x => x.Oid).ToArray();
                }
               NotifyDataSetChanged();
            }
            catch (Exception ex)
            {
                Android.Widget.Toast.MakeText(Application.Context, $"EX on update {ex}", Android.Widget.ToastLength.Short).Show();
            }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView; // re-use an existing view, if one is available
            if (view == null) // otherwise create a new one
                view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem2, null);
            view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = items[position].ToString();
            return view;
        }

    }
    //public class ViewHolder : Java.Lang.Object
    //{
    //    Activity context;
    //    Dialog dlgFinding;
    //    //TextView txtAssetGroupILIUI;
    //    //TextView txtAssetILIUI;
    //    //TextView txtInspectionDateILIUI;
    //    //TextView txtFindingILIUI;
    //    //TextView txtCommentsILIUI;
    //    //TextView txtWorkOrderILIUI;
    //    //TextView txtDecommissionILIUI;

    //    // this method now handles getting references to our subviews
    //    public void Initialize(View view)
    //    {
    //        //txtAssetGroupILIUI = view.FindViewById<TextView>(Resource.Id.txtAssetGroupILIUI);
    //        //txtAssetILIUI = view.FindViewById<TextView>(Resource.Id.txtAssetILIUI);
    //        //txtInspectionDateILIUI = view.FindViewById<TextView>(Resource.Id.txtInspectionDateILIUI);
    //        //txtFindingILIUI = view.FindViewById<TextView>(Resource.Id.txtFindingILIUI);
    //        //txtCommentsILIUI = view.FindViewById<TextView>(Resource.Id.txtCommentsILIUI);
    //        //txtWorkOrderILIUI = view.FindViewById<TextView>(Resource.Id.txtWorkOrderILIUI);
    //        //txtDecommissionILIUI = view.FindViewById<TextView>(Resource.Id.txtDecommissionILIUI);

    //        //txtFindingILIUI.Click += new EventHandler(this.txtFindings_Click);
    //    }

    //    //// this method now handles binding data
    //    //public void Bind(Activity myContext, string strAssetGroup, string strAsset, string strInspectionDate, string strFinding, string strComments,
    //    //    string strWorkOrder, string strDescommission)
    //    //{

    //    //    context = myContext; 

    //    //} 
    //}
}