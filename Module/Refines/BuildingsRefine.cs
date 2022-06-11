using System.Collections.Generic;
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
        private readonly Csv csv = new();

        public async Task<Csv> RefineAsync(AppBase<Settings> app, Stream xmlStream, bool drawingsAreImported, string moduleBasePath)
        {
            app.Log.LogInformation("Loading buildings");
            moduleBasePath = moduleBasePath.Trim('/');
            if (xmlStream == null)
                return default;

            CreateCsv(xmlStream, drawingsAreImported, moduleBasePath, app.Settings.DownloadFileApiKey);
            await app.DataLake.SaveCsvAsync(csv, "Refined", "Buildings.csv", FolderStructure.DatePath);
            app.Mssql.InserCsv(csv, "Buildings", true, false);
            return csv;
        }

        private void CreateCsv(Stream stream, bool drawingsAreImported, string moduleBasePath, string apiKey)
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

        private void AddDrawingLink(string moduleBasePath, string apiKey, bool drawingsAreImported, int row, XElement element)
        {
            csv.AddHeader("Drawings", false, out int drawingsCol);
            var drawingsCount = element.Elements("Drawing").Count();
            csv.AddRecord(row, drawingsCol, drawingsCount);

            if (drawingsAreImported && drawingsCount > 0)
            {
                csv.AddHeader("Drawing link", false, out int drawingLinkCol);
                csv.AddHeader("Drawio link", false, out int diagramLinkCol);
                var buildingId = element.Attribute("MasterID").Value;
                var drawingUrl = $"https://{moduleBasePath}/api/getFile?filepath=Drawings%2FBuildings%2F{buildingId}.pdf&apikey={apiKey}";
                var drawioUrl = $"https://{moduleBasePath}/api/getFile?filepath=Drawings%2FBuildings%2F{buildingId}.drawio&raw=1&apikey={apiKey}";
                //var diagramUrl = "https://app.diagrams.net/#U" + HttpUtility.UrlEncode(drawioUrl);  //It takes to long time to download the file, so drawio times out.
                csv.AddRecord(row, drawingLinkCol, drawingUrl);
                csv.AddRecord(row, diagramLinkCol, drawioUrl);
            }
        }

        private void AddBuildingGps(int row, XElement element)
        {
            var coords = new List<(double Lat, double Lon)>();
            foreach (var coord in element.Element("GIS").Element("OuterPolygon").Elements("Coordinate"))
                coords.Add(((float)coord.Attribute("x"), (float)coord.Attribute("y")));

            if (GIS.GetGISGravityPoint(coords, out (double Lat, double Lon) gps))
            {
                csv.AddHeader("GISLat", false, out int latCol);
                csv.AddHeader("GISLon", false, out int lonCol);
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

        private void AddBBR(int row, XElement element)
        {
            var elem = element.Element("Bbr_bygning");
            if (elem != null)
                foreach (var item in elem.Attributes())
                {
                    csv.AddHeader(item.Name.ToString(), false, out int col);
                    csv.AddRecord(row, col, item.Value);
                }
        }
    }
}
