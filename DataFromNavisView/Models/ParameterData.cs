using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DataFromNavisView.Models
{
    public class ParameterData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string LinkName { get; set; }
        public int ElementId { get; set; }
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
    }
}
