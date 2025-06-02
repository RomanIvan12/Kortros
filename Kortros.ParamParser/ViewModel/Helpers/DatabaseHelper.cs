using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Kortros.ParamParser.ViewModel.Helpers
{

    public class DatabaseHelper
    {
        //private static readonly string _dbFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "configs.db3");
        private static readonly string _dbFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "configs.db3");


        //private static readonly string _dbFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "configs.db3");

        public static bool Insert<T>(T item)
        {
            bool result = false;

            using (SQLiteConnection conn = new SQLiteConnection(_dbFile))
            {
                conn.CreateTable<T>();
                int rows = conn.Insert(item);
                if (rows > 0)
                    result = true;
            }
            return result;
        }



        public static bool Update<T>(T item)
        {
            bool result = false;

            using (SQLiteConnection conn = new SQLiteConnection(_dbFile))
            {
                conn.CreateTable<T>();
                int rows = conn.Update(item);
                if (rows > 0)
                    result = true;
            }

            return result;
        }

        public static bool Delete<T>(T item)
        {
            bool result = false;

            using (SQLiteConnection conn = new SQLiteConnection(_dbFile))
            {
                conn.CreateTable<T>();
                int rows = conn.Delete(item);
                if (rows > 0)
                    result = true;
            }

            return result;
        }

        public static List<T> Read<T>(string defaultDbFile = null) where T : new()
        {
            defaultDbFile = defaultDbFile ?? _dbFile;

            List<T> items;

            using (SQLiteConnection conn = new SQLiteConnection(defaultDbFile))
            {
                conn.CreateTable<T>();
                items = conn.Table<T>().ToList();
            }

            return items;
        }

        public static void DeleteTable<T>()
        {
            using (SQLiteConnection conn = new SQLiteConnection(_dbFile))
            {
                conn.DropTable<T>();
            }
        }


        public static string ExtractDatabaseToTemporaryFile(string version)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"data{version}.db3");

            // Get current assembly
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Resource name
            string resourceName = $"Kortros.ParamParser.data{version}.db3";

            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    throw new Exception("Embedded resource not found." + resourceName);
                }

                using (FileStream filestream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(filestream);
                }
            }

            return tempFile;
        }
    }
}
