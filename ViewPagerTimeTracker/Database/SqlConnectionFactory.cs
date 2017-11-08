using SQLite;
using System.IO;

namespace TimeTrackerUniversal.Database
{
    public class SqlConnectionFactory
	{
		public static string fileName = "TimeTrackerUniversal.db3";

        public static SQLiteConnection GetSQLiteConnectionWithLock()
        {
            //  var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            //string documentsPath = "/sdcard/MyAppData/Database/";
            //string path = documentsPath + fileName;
            ////Path.Combine(documentsPath, fileName); 
            //if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);

            //if (!File.Exists(path))
            //{
            //    File.Create(path);
            //}
          var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            path = Path.Combine(path, fileName);
            var connection = new SQLiteConnection(path);

            return connection;
        }
        public static SQLiteConnection GetSQLiteConnection()
        {
            //  var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            //string documentsPath = "/sdcard/MyAppData/Database/";
            //string path = documentsPath + fileName;
            ////Path.Combine(documentsPath, fileName); 
            //if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);

            //if (!File.Exists(path))
            //{
            //    File.Create(path);
            //}
            var   path =  System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            path = Path.Combine(path, fileName);
            var connection = new SQLiteConnection(path);

            return connection;
        }
        /*
                public static SQLiteConnection GetSQLiteConnectionWithLock()
                {
                    //var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    // var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                   string documentsPath = "/sdcard/MyAppData/Database/";
                    string path = documentsPath + fileName;
                    Path.Combine(documentsPath, fileName); 
                    //if (!Directory.Exists(documentsPath)) Directory.CreateDirectory(documentsPath);

                    //if (!File.Exists(path))
                    //{
                    //    File.Create(path);
                    //}
                    //var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    //path = Path.Combine(path, fileName);
                    var param = new SQLiteConnectionString(path, true);
                    var connection = new SQLiteConnectionWithLock(param, SQLiteOpenFlags.ProtectionNone);

                    return connection;
                }*/
        //var sqliteFilename = "AAAAAAAA.db3";
        //string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
        //var path = Path.Combine(documentsPath, sqliteFilename);
        //var platform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
        //var param = new SQLiteConnectionString(path, false);
        //var connection = new SQLiteAsyncConnection(() => new SQLiteConnectionWithLock(platform, param));
        //return connection;

    }
	public interface IFileHelper
	{
		string GetLocalFilePath(string filename);
	}
	//static TodoItemDatabase database;

	//public static TodoItemDatabase Database
	//{
	//	get
	//	{
	//		if (database == null)
	//		{
	//			database = new TodoItemDatabase(DependencyService.Get<IFileHelper>().GetLocalFilePath("TodoSQLite.db3"));
	//		}
	//		return database;
	//	}
	//}
	//public TodoItemDatabase(string dbPath)
	//{
	//	database = new SQLiteAsyncConnection(dbPath);
	//	database.CreateTableAsync<TodoItem>().Wait();
	//}
}

