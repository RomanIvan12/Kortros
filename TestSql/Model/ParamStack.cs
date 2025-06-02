using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSql.Model
{
    public class ParamStack
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int CategoryItemId { get; set; }
        public string TESTname { get; set; }
    }
}
