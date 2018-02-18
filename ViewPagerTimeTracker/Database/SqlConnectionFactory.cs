using SQLite;
using System.IO;

namespace TimeTrackerUniversal.Database
{
    public class SqlConnectionFactory
    {
        public static string fileName { get => "TimeDB.db3"; }

        public static string fullPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "db");
        public static string FULLDBFILEPATH = Path.Combine(fullPath, fileName);
        public static bool init { get { return File.Exists(FULLDBFILEPATH); } }

        public static SQLiteConnection GetSQLiteConnectionWithLock()
        {
            var connection = new SQLiteConnection(FULLDBFILEPATH);

            return connection;
        }
        public static SQLiteConnection GetSQLiteConnection()
        {
            var connection = new SQLiteConnection(FULLDBFILEPATH);

            return connection;
        }

        public static SQLiteConnectionWithLock GetSQLiteConnectionWithREALLock()
        {
            try
            {
                if (!init)
                {
                    System.IO.Directory.CreateDirectory(fullPath);
                    System.IO.File.Create(FULLDBFILEPATH); 
                   // var connection = new SQLiteConnectionWithLock(new SQLiteConnectionString(FULLDBFILEPATH, true), SQLiteOpenFlags.Create);

                    return new SQLiteConnectionWithLock(new SQLiteConnectionString(FULLDBFILEPATH, true), SQLiteOpenFlags.ReadWrite);
                }
                else
                {
                     return new SQLiteConnectionWithLock(new SQLiteConnectionString(FULLDBFILEPATH, true), SQLiteOpenFlags.ReadWrite);                  
                }
            }
            catch (System.Exception x)
            {
                System.Console.WriteLine(x.ToString());

            }

            return null;
        }
        //var sqliteFilename = "AAAAAAAA.db3";
        //string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
        //var path = Path.Combine(documentsPath, sqliteFilename);
        //var platform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
        //var param = new SQLiteConnectionString(path, false);
        //var connection = new SQLiteAsyncConnection(() => new SQLiteConnectionWithLock(platform, param));
        //return connection;

    }
}

