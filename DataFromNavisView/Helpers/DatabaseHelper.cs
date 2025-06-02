using System;
using System.Collections.Generic;
using System.IO;
using SQLite;

namespace DataFromNavisView.Helpers
{
    public class DatabaseHelper
    {
        private static readonly string _dbFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "exportData.db3");

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
    }
}
