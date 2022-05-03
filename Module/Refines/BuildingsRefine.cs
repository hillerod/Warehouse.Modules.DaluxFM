﻿using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Bygdrift.CsvTools;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Bygdrift.Warehouse;
using Bygdrift.DataLakeTools;
using Module.Refines.Helpers;

namespace Module.Refines
{
    public class BuildingsRefine
    {
        private static readonly Csv csv = new();

        public static async Task<Csv> RefineAsync(AppBase<Settings> app, Stream xmlStream, bool drawingsAreImported, string moduleBasePath)
        {
            app.Log.LogInformation("Loading buildings");
            moduleBasePath = moduleBasePath.Trim('/');
            if (xmlStream == null)
                return default;

            CreateCsv(xmlStream, drawingsAreImported, moduleBasePath, app.Settings.DownloadFileApiKey);
            await app.DataLake.SaveCsvAsync(csv, "Refined", "Buildings.csv", FolderStructure.DatePath);
            app.Mssql.MergeCsv(csv, "Buildings", "MasterID", true, true);
            return csv;
        }

        private static void CreateCsv(Stream stream, bool drawingsAreImported, string moduleBasePath, string apiKey)
        {
            stream.Position = 0;
            var data = XDocument.Load(stream);

            var r = 1;
            var genericHelper = new GenericHelper();
            foreach (var item in data.Root.Descendants("Building"))
            {
                genericHelper.AddAttributes(r, item, csv, new string[] { "GisPolygon" });
                AddDrawingLink(moduleBasePath, apiKey, drawingsAreImported, r, item);
                AddBuildingGps(r, item);
                AddBBR(r, item);
                genericHelper.AddLayerDatas(r, item, csv);
                r++;
            }
        }

        private static void AddDrawingLink(string moduleBasePath, string apiKey, bool drawingsAreImported, int row, XElement element)
        {
            var drawingsCol = csv.GetOrCreateHeader("Drawings");
            var drawingsCount = element.Elements("Drawing").Count();
            csv.AddRecord(row, drawingsCol, drawingsCount);

            if (drawingsAreImported && drawingsCount > 0)
            {
                var drawingLinkCol = csv.GetOrCreateHeader("Drawing link");
                var diagramLinkCol = csv.GetOrCreateHeader("Diagram link");
                var buildingId = element.Attribute("MasterID").Value;
                var drawingUrl = $"https://{moduleBasePath}/api/getFile?filepath=Drawings%2FBuildings%2F{buildingId}.pdf&apikey={apiKey}";
                var drawioUrl = $"https://{moduleBasePath}/api/getFile?filepath=Drawings%2FBuildings%2F{buildingId}.drawio&raw=1&apikey={apiKey}";
                var diagramUrl = "https://app.diagrams.net/#U" + HttpUtility.UrlEncode(drawioUrl);
                csv.AddRecord(row, drawingLinkCol, drawingUrl);
                csv.AddRecord(row, diagramLinkCol, diagramUrl);
            }
        }

        private static void AddBuildingGps(int row, XElement element)
        {
            var coords = new List<(double Lat, double Lon)>();
            foreach (var coord in element.Element("GIS").Element("OuterPolygon").Elements("Coordinate"))
                coords.Add(((float)coord.Attribute("x"), (float)coord.Attribute("y")));

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

        private static void AddBBR(int row, XElement element)
        {
            var elem = element.Element("Bbr_bygning");
            if (elem != null)
                foreach (var item in elem.Attributes())
                {
                    var col = csv.GetOrCreateHeader(item.Name.ToString());
                    csv.AddRecord(row, col, item.Value);
                }
        }
    }
}
