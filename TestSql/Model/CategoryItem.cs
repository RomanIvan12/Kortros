using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace TestSql.Model
{
    public class CategoryItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int ConfigId { get; set; }
        public string Name { get; set; }
    }
}
