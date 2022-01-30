//<PackageReference Include="NetTopologySuite" Version="2.4.0" />
//using NetTopologySuite.Geometries;

using System.Collections.Generic;
using System.Linq;

namespace Module.Refines.Helpers
{
    public static class GIS
    {
        public static bool GetGISGravityPoint(List<(double Lat, double Lon)> coords, out (double Lat, double Lon) gps)
        {
            if (coords.Any())
            {
                var latMin = coords.Min(o => o.Lat);
                var latMax = coords.Max(o => o.Lat);
                var lonMin = coords.Min(o => o.Lon);
                var lonMax = coords.Max(o => o.Lon);
                var lat = (latMax - latMin) / 2 + latMin;
                var lon = (lonMax - lonMin) / 2 + lonMin;
                gps = (lat, lon);
                return true;
            }
            gps = default;
            return false;
        }


        //I am working on a way to get an area from some geolocations. I thought it was easy to find a package and get the data, but it's a bit more complicated, så for now, it's commented out to I get the time to crack it
        //Look at: https://gis.stackexchange.com/questions/165022/transforming-point-using-nettopologysuite
        //And this: https://groups.google.com/g/nettopologysuite/c/gl2g2O807X0


        /// <summary>
        /// NOT WORKING - only giving the geo area and not depending on where and then the sqmeters
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        //public static double GetGeoArea(List<(double Lat, double Lon)> coords)
        //{
        //    var earthRadius = 6371009;

        //    var geometryFactory = new GeometryFactory();
        //    Coordinate[] coord2 = new Coordinate[]
        //    {
        //        new Coordinate(55.84322269721682, 12.061325437502692),
        //        new Coordinate(55.84331072172417, 12.061274361796062),
        //        new Coordinate(55.84321319337128, 12.060743099930516),
        //        new Coordinate(55.84326180999534, 12.060714967659774),
        //        new Coordinate(55.843347282307775, 12.060665287611268),
        //        new Coordinate(55.843318314103094, 12.060507226130325),
        //        new Coordinate(55.84309611526841, 12.060635949543226),
        //        new Coordinate(55.84322269721682, 12.061325437502692),  //The first coordinate to close the ring
        //    };

        //    Polygon poly = geometryFactory.CreatePolygon(coord2);

        //    var geoArea = Math.Abs(poly.Area * (earthRadius * earthRadius));

        //    return geoArea;
        //}

        //public static double GetArea(List<(double Lat, double Lon)> coords)
        //{
        //    var area = 0.0;
        //    if (coords.Count > 2)
        //    {

        //        for (var i = 0; i < coords.Count - 1; i++)
        //        {
        //            var p1 = coords[i];
        //            var p2 = coords[i + 1];
        //            area += Rad(p2.Lon - p1.Lon) * (2 + Math.Sin(Rad(p1.Lat)) + Math.Sin(Rad(p2.Lat)));
        //        }
        //        area = area * 6378137.0 * 6378137.0 / 2.0;
        //    }
        //    return Math.Abs(area);
        //}

        ///// <summary>
        ///// NOT IN USE YET
        ///// </summary>
        ///// <param name="coords"></param>
        ///// <param name="area"></param>
        ///// <returns></returns>
        //public static bool GetGISArea(List<(double Lat, double Lon)> coords, out double area)
        //{
        //    area = default;
        //    if (coords.Any())
        //    {
        //        //area = ComputeSignedArea(coords);
        //        area = Tt(coords);
        //        return true;
        //    }
        //    return false;
        //}

        //public static double Rad(double deg)
        //{
        //    return deg * Math.PI / 180.0f;
        //}

        //private static double Tt(List<(double Lat, double Lon)> coords)
        //{
        //    // Add all coordinates to a list, converting them to meters:
        //    var points = new List<(double Lat, double Lon)>();
        //    foreach (var coord in coords)
        //        points.Add((coord.Lat * (Math.PI * 6378137 / 180), coord.Lon * (Math.PI * 6378137 / 180)));

        //    points.Add(points[0]);  // Add point 0 to the end again:

        //    // Calculate polygon area (in square meters):
        //    var area = Math.Abs(points.Take(points.Count - 1)
        //      .Select((p, i) => (points[i + 1].Lat - p.Lat) * (points[i + 1].Lon + p.Lon))
        //      .Sum() / 2);
        //    return area;
        //}

        //private static double ComputeSignedArea(List<(double Lat, double Lon)> path)
        //{
        //    var earthRadius = 6371009;
        //    int size = path.Count;
        //    if (size < 3) { return 0; }
        //    double total = 0;
        //    var prev = path[size - 1];
        //    double prevTanLat = Math.Tan((Math.PI / 2 - ToRadians(prev.Lat)) / 2);
        //    double prevLng = ToRadians(prev.Lon);

        //    foreach (var point in path)
        //    {
        //        double tanLat = Math.Tan((Math.PI / 2 - ToRadians(point.Lat)) / 2);
        //        double lng = ToRadians(point.Lon);
        //        total += PolarTriangleArea(tanLat, lng, prevTanLat, prevLng);
        //        prevTanLat = tanLat;
        //        prevLng = lng;
        //    }
        //    return total * (earthRadius * earthRadius);
        //}

        //private static double ToRadians(double input)
        //{
        //    return input / 180.0 * Math.PI;
        //}

        //private static double PolarTriangleArea(double tan1, double lng1, double tan2, double lng2)
        //{
        //    double deltaLng = lng1 - lng2;
        //    double t = tan1 * tan2;
        //    return 2 * Math.Atan2(t * Math.Sin(deltaLng), 1 + t * Math.Cos(deltaLng));
        //}
    }
}