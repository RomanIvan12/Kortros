using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ApartmentsProject.AuxiliaryСlasses
{
    public class DoorSolids
    {
        private readonly Document _doc;
        private readonly double _extrusionLength = UnitUtils.ConvertToInternalUnits(200, UnitTypeId.Millimeters);

        public Solid Solid { get; set; }
        public DoorSolids(Element doorElement, Document doc)
        {
            _doc = doc;
            Solid = CreateDoorSolid(doorElement);
        }

        private Solid CreateDoorSolid(Element element)
        {
            int id = element.Id.IntegerValue;
            var doorType = element.GetTypeId();
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
            {
                var zzz = element.Id.IntegerValue;
                double width = (_doc.GetElement(doorType).get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM).AsDouble() == 0) ? element.get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM).AsDouble() :
                    _doc.GetElement(doorType).get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM).AsDouble();
                double height = (_doc.GetElement(doorType).get_Parameter(BuiltInParameter.GENERIC_HEIGHT).AsDouble() == 0) ? element.get_Parameter(BuiltInParameter.GENERIC_HEIGHT).AsDouble() :
                    _doc.GetElement(doorType).get_Parameter(BuiltInParameter.GENERIC_HEIGHT).AsDouble();

                var vector = (element as FamilyInstance).FacingOrientation;

                // Create door contour
                List<Line> contour = new List<Line>()
                {
                    Line.CreateBound(new XYZ(-width/2, -_extrusionLength, 0), new XYZ(width/2, -_extrusionLength, 0)),
                    Line.CreateBound(new XYZ(width/2, -_extrusionLength, 0), new XYZ(width/2, -_extrusionLength, height)),
                    Line.CreateBound(new XYZ(width/2, -_extrusionLength, height), new XYZ(-width/2, -_extrusionLength, height)),
                    Line.CreateBound(new XYZ(-width/2, -_extrusionLength, height), new XYZ(-width/2, -_extrusionLength, 0))
                };

                CurveLoop curve = new CurveLoop();
                contour.ForEach(line => curve.Append(line));

                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(
                    new CurveLoop[] { curve },
                    XYZ.BasisY, 
                    _extrusionLength * 2);

                // Создание трансформации
                LocationPoint location = element.Location as LocationPoint;
                Transform translation = Transform.CreateTranslation(location.Point); // перенесли в точку
                Transform rotation = Transform.CreateRotation(
                    XYZ.BasisZ,
                    XYZ.BasisY.AngleTo(vector));

                Transform combinedTransform = translation.Multiply(rotation);

                Solid newSolid = SolidUtils.CreateTransformed(solid, combinedTransform);

                return newSolid;
            }
            else
                throw new Exception();
        }

    }
}