﻿using Bygdrift.CsvTools;
using Bygdrift.DataLakeTools;
using Bygdrift.Warehouse;
using Module.Refines.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Module.Refines
{
    public class LotsRefine
    {
        private static readonly Csv csv = new();

        public static async Task<Csv> RefineAsync(AppBase app, Stream xmlStream)
        {
            app.Log.LogInformation("Loading Lots");
            if (xmlStream == null)
                return default;

            CreateCsv(xmlStream);
            await app.DataLake.SaveCsvAsync(csv, "Refined", "Lots.csv", FolderStructure.DatePath);
            app.Mssql.MergeCsv(csv, "Lots", "MasterID", true, true);
            return csv;
        }

        private static void CreateCsv(Stream stream)
        {
            stream.Position = 0;
            var data = XDocument.Load(stream);
            var r = 1;
            var genericHelper = new GenericHelper();
            foreach (var item in data.Root.Descendants("Lot"))
            {
                genericHelper.AddAttributes(r, item, csv, new string[] { "GisPolygon" });
                AddGIS(r, item);
                genericHelper.AddLayerDatas(r, item, csv);
                r++;
            }
        }

        private static void AddGIS(int row, XElement element)
        {
            var elements = element.Elements("GIS");
            if (!elements.Any())
                return;

            var coords = new List<(double Lat, double Lon)>();
            foreach (var item in elements)
                foreach (var coord in item.Element("OuterPolygon").Elements("Coordinate"))
                    coords.Add(((double)coord.Attribute("x"), (double)coord.Attribute("y")));

            if (GIS.GetGISGravityPoint(coords, out (double Lat, double Lon) gps))
            {
                var latCol = csv.GetOrCreateHeader("GISLat");
                var lonCol = csv.GetOrCreateHeader("GISLon");
                csv.AddRecord(row, latCol, gps.Lat);
                csv.AddRecord(row, lonCol, gps.Lon);
            }

            //I am working on a way to get an area from some geolocations. I thought it was easy to find a package and get the data, but it's a bit more complicated, så for now, it's commented out to I get the time to crack it
            //if (GISArea.GetGISArea(coords, out double area))
            //{
            //    var gisAreaCol = csv.GetOrCreateHeader("GISArea");
            //    csv.AddRecord(row, gisAreaCol, area);
            //}
        }
    }
}
