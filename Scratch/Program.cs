using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Scratch
{
    class Program
    {
        public static string fileName { get => "TimeDB.db3"; }
        static void Main(string[] args)
        {
            string p; int daysToAdd = -1;
            string datetime = $"{DateTime.Now.AddDays(daysToAdd).ToLocalTime().Month.ToString("00")}" +
                $"{DateTime.Now.AddDays(daysToAdd).ToLocalTime().Day.ToString("00")}" +
                $"{DateTime.Now.AddDays(daysToAdd).ToLocalTime().Year}";
            if (datetime.StartsWith(p = GetDigitDate()))
                Console.WriteLine($"p{p}p starts with GetDigitDate()!");// filename =/* MainActivity.GetDigitDate() */p + filename;
            else Console.WriteLine($"p{p}p DOES NOT start with GetDigitDate()!");// filename =/* MainActivity.GetDigitDate() */p + filename;
            datetime += "\n***********************************************\n";
            var files = GetFileNames(@"C:\Users\kjaku\Dropbox\MunsysSFO\MyLISP\", "*.*");
            files.ToList().ForEach(x => Console.WriteLine(x));
            Console.WriteLine($"{datetime}!");

            Console.WriteLine("\n\n+*************\n");
            Console.WriteLine($"now ticks{DateTime.Now.ToLocalTime().Ticks}\n" +
                $"tomorrw {DateTime.Now.AddDays(1).ToLocalTime().Ticks}\n" +
                $"diff {DateTime.Now.AddDays(1).ToLocalTime().Ticks-DateTime.Now.ToLocalTime().Ticks}");
            Console.ReadKey();
        }
        public static string GetDigitDate(int daysToAdd = 0)
        {
            return $"{DateTime.Now.AddDays(daysToAdd).ToLocalTime().Month.ToString("00")}" +
                $"{DateTime.Now.AddDays(daysToAdd).ToLocalTime().Day.ToString("00")}" +
                $"{DateTime.Now.AddDays(daysToAdd).ToLocalTime().Year}";
        }
        private static string[] GetFileNames(string path, string filter)
        {
            List<string> ret = new List<string>();
            var files = Directory.GetFiles(path, filter).Where(x =>File.GetLastWriteTime(x) < DateTime.Now.AddDays(-30).ToLocalTime()).ToArray();
            for (int i = 0; i < files.Length; i++)
            {  //File.GetCreationTime(x).Ticks < DateTime.Now.AddDays(-30).ToLocalTime().Ticks
                Console.WriteLine($"\n{i} |  create {File.GetCreationTime(files[i]).ToLocalTime().Ticks}   {File.GetCreationTime(files[i]).ToLocalTime()}" +
                    $"\n GetLastWriteTime {File.GetLastWriteTime(files[i]).ToLocalTime().Ticks}   {File.GetLastWriteTime(files[i]).ToLocalTime()}" +
                    $"\n");
                files[i] = Path.GetFileName(files[i]);

            }
            Console.WriteLine("--------------------\n------------------\n*************************\n*****************************\n----------------------------------------");
            files = Directory.GetFiles(path, filter).Where(x => File.GetLastWriteTime(x).Ticks < DateTime.Now.ToLocalTime().Ticks).ToArray();
            files = Directory.GetFiles(path, filter);
            for (int i = 0; i < files.Length; i++)
            {  //File.GetCreationTime(x).Ticks < DateTime.Now.AddDays(-30).ToLocalTime().Ticks
              
                TimeSpan t = new TimeSpan(DaysInTicks(-30));
                Console.WriteLine($"t {t}");
                TimeSpan p = new TimeSpan();
                Console.WriteLine( $"writetime{File.GetLastWriteTime(files[i]).ToLocalTime().Ticks}   {File.GetCreationTime(files[i]).ToLocalTime()}\n" +
                    $"diff { DateTime.Now.ToLocalTime().Ticks- File.GetLastWriteTime(files[i]).Ticks } ...Days  { (DateTime.Now.ToLocalTime().Ticks- File.GetLastWriteTime(files[i]).Ticks)/DaysInTicks(1)}");
                Console.WriteLine($"\n{i}  | create {File.GetCreationTime(files[i]).ToLocalTime().Ticks}   {File.GetCreationTime(files[i]).ToLocalTime()}" +
            //        $"\n GetLastWriteTime {File.GetLastWriteTime(files[i]).ToLocalTime().Ticks}   {File.GetLastWriteTime(files[i]).ToLocalTime()}" +
                    $"\n");
                files[i] = Path.GetFileName(files[i]);

            }
            return files;
        }
        private static long DaysInTicks(int days = 1)
        {
            return DateTime.Now.AddDays(days).ToLocalTime().Ticks - DateTime.Now.ToLocalTime().Ticks;
        }
    }
}
