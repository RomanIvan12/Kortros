using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kortros.Architecture.Apartments.Model
{
    public class TableInstance
    {
        public List<Room> Rooms { get; set; }
        public string Level { get; set; }
        public bool IsVersionsAvailable { get; set; }
        public string Version { get; set; }
        public double AreaOfLivingSpace { get; set; }
        public double AreaOfSpace { get; set; }
        public double AreaVNS { get; set; }
        public bool IsAreaVNSCorrect { get; set; }
        public double AreaGNS1 { get; set; }
        public bool IsAreaGNS1Correct { get; set; }
        public double AreaGNS2 { get; set; }
        public bool IsAreaGNS2Correct { get; set; }
        public double AreaSectionGNS1 { get; set; }
        public double AreaSectionGNS2 { get; set; }
    }
}
