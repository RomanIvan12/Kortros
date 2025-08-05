using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApartmentsProject.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace ApartmentsProject.AuxiliaryСlasses
{
    public class NumberingExtention
    {
        /// <summary>
        /// Функция получения центроида для помещения
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static XYZ GetRoomCentroid(Room room)
        {
            var boundaries = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
            if (boundaries == null || boundaries.Count == 0) return null;

            IList<BoundarySegment> mainBoundary = boundaries[0];

            // Получаем координаты вершин
            var points = new List<XYZ>();
            foreach (BoundarySegment seg in mainBoundary)
            {
                XYZ pt = seg.GetCurve().GetEndPoint(0);
                points.Add(pt);
            }
            // Убираем дубликаты (если последний совпадает с первым)
            if ((points[0] - points[points.Count - 1]).IsZeroLength())
                points.RemoveAt(points.Count - 1);

            // Получаю площадь полигона
            double signedArea = 0;
            double cx = 0;
            double cy = 0;

            for (int i = 0; i < points.Count; i++)
            {
                XYZ p0 = points[i];
                XYZ p1 = points[(i + 1) % points.Count];

                double a = p0.X * p1.Y - p1.X * p0.Y;
                signedArea += a;

                cx += (p0.X + p1.X) * a;
                cy += (p0.Y + p1.Y) * a;
            }

            signedArea *= 0.5;
            cx /= (6 * signedArea);
            cy /= (6 * signedArea);

            double z = room.Level.Elevation;
            var mainPoint = new XYZ(cx, cy, z);
            return mainPoint;
        }

        public static XYZ GetGeometricCenter(IList<XYZ> points)
        {
            int count = points.Count;
            if (count == 0) return null;

            double sumX = 0;
            double sumY = 0;
            double sumZ = 0;

            foreach (var pt in points)
            {
                sumX += pt.X;
                sumY += pt.Y;
                sumZ += pt.Z;
            }
            return new XYZ(sumX / count, sumY / count, sumZ / count);
        }
    }

    public class RectangleByPoints
    {
        // Векторное произведение (для построения выпуклой оболочки)
        static double Cross(XYZ a, XYZ b, XYZ c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }
        // Выпуклая оболочка (алгоритм Грэхема)
        public static List<XYZ> ConvexHull(List<XYZ> pts)
        {
            pts = pts.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            var hull = new List<XYZ>();

            // Нижняя часть оболочки
            foreach (var p in pts)
            {
                while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(p);
            }

            // Верхняя часть оболочки

            int t = hull.Count + 1;
            for (int i = pts.Count - 2; i >= 0; i--)
            {
                var p = pts[i];
                while (hull.Count >= t && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(p);
            }
            hull.RemoveAt(hull.Count - 1);
            return hull;
        }
        static XYZ Rotate(XYZ p, double angleRadians)
        {
            double cosA = Math.Cos(angleRadians), sinA = Math.Sin(angleRadians);
            return new XYZ(
                p.X * cosA - p.Y * sinA,
                p.X * sinA + p.Y * cosA,
                p.Z);
        }

        public static List<XYZ> CreateRectangle(List<XYZ> points)
        {
            double zLevel = points.First().Z;
            var inputPts = points.Select(p => new XYZ(p.X, p.Y, 0)).ToList();

            var hull = ConvexHull(inputPts);

            double minArea = double.MaxValue;
            double bestAngle = 0;
            XYZ rectMin = null, rectMax = null;

            for (int i = 0; i < hull.Count; i++)
            {
                int j = (i + 1) % hull.Count;
                // Вектор вдоль ребра
                double dx = hull[j].X - hull[i].X;
                double dy = hull[j].Y - hull[i].Y;
                double angle = Math.Atan2(dy, dx);

                double minX = double.MaxValue, minY = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue;

                // Все точки выпуклой оболочки поворачиваем так, чтобы ребро было стало горизонтальным
                foreach (var p in hull)
                {
                    var rp = Rotate(p, -angle);
                    minX = Math.Min(minX, rp.X);
                    maxX = Math.Max(maxX, rp.X);
                    minY = Math.Min(minY, rp.Y);
                    maxY = Math.Max(maxY, rp.Y);
                }
                double area = (maxX - minX) * (maxY - minY);

                if (area < minArea)
                {
                    minArea = area;
                    rectMin = new XYZ(minX, minY, 0);
                    rectMax = new XYZ(maxX, maxY, 0);
                    bestAngle = angle;
                } 
            }

            double bestAngleDeg = bestAngle * 180 / Math.PI; // перевёл в градусы                                            
            bestAngleDeg = Math.Round(bestAngleDeg / 15.0) * 15.0; // Округляем до ближайших 15 градусов
            bestAngle = bestAngleDeg * Math.PI / 180.0;

            double finalMinX = double.MaxValue, finalMinY = double.MaxValue;
            double finalMaxX = double.MinValue, finalMaxY = double.MinValue;

            foreach (var p in hull)
            {
                var rp = Rotate(p, -bestAngle);
                finalMinX = Math.Min(finalMinX, rp.X);
                finalMaxX = Math.Max(finalMaxX, rp.X);
                finalMinY = Math.Min(finalMinY, rp.Y);
                finalMaxY = Math.Max(finalMaxY, rp.Y);
            }

            // Получаем 4 угла минимального прямоугольника
            XYZ[] rect = new XYZ[4];
            rect[0] = Rotate(new XYZ(finalMinX, finalMinY, 0), bestAngle);
            rect[1] = Rotate(new XYZ(finalMaxX, finalMinY, 0), bestAngle);
            rect[2] = Rotate(new XYZ(finalMaxX, finalMaxY, 0), bestAngle);
            rect[3] = Rotate(new XYZ(finalMinX, finalMaxY, 0), bestAngle);

            for (int i = 0; i < 4; i++)
                rect[i] = new XYZ(rect[i].X, rect[i].Y, zLevel);

            return rect.ToList();
        }
    }


    public class RectangleSolver
    {
        public static bool IntersectRayWithSegment(XYZ origin, XYZ direction, XYZ segA, XYZ segB, out XYZ intersection)
        {
            // 2D-проекция: всё на плоскости XY (игнорируем Z)
            double x1 = segA.X, y1 = segA.Y;
            double x2 = segB.X, y2 = segB.Y;
            double x3 = origin.X, y3 = origin.Y;
            double x4 = origin.X + direction.X, y4 = origin.Y + direction.Y;

            double den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(den) < 1e-9)
            {
                intersection = null;
                return false;
            }
            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            double u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;

            if (t >= 0 && t <= 1 && u >= 0)
            {
                intersection = new XYZ(x1 + t * (x2 - x1), y1 + t * (y2 - y1), origin.Z);
                return true;
            }
            intersection = null;
            return false;
        }

        public static XYZ GetFinishPoint(List<XYZ> points, XYZ inner, NumberingStart rayDirection)
        {
            // Находим стороны прямоугольника (по парам соседних точек)
            int n = points.Count;
            XYZ rayDir = XYZ.Zero;
            switch (rayDirection)
            {
                case NumberingStart.Top:
                    rayDir = new XYZ(0, 1, 0);
                    break;
                case NumberingStart.Bottom:
                    rayDir = new XYZ(0, -1, 0);
                    break;
                case NumberingStart.Left:
                    rayDir = new XYZ(-1, 0, 0);
                    break;
                case NumberingStart.Right:
                    rayDir = new XYZ(1, 0, 0);
                    break;
            }

            for (int i = 0; i < n; i++)
            {
                XYZ a = points[i];
                XYZ b = points[(i + 1) % n];
                if (IntersectRayWithSegment(inner, rayDir, a, b, out XYZ intersection))
                {
                    return intersection;
                }
            }
            throw new Exception("Не удалось найти пересечение луча с прямоугольником");
        }
    }
}
