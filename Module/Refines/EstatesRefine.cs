using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;
using Bygdrift.CsvTools;
using Bygdrift.Warehouse;
using System.Threading.Tasks;
using Bygdrift.DataLakeTools;
using Module.Refines.Helpers;

namespace Module.Refines
{
    public class EstatesRefine
    {
        private static readonly Csv csv = new();

        public static async Task<Csv> RefineAsync(AppBase app, Stream xmlStream, Csv buildingsCsv)
        {
            app.Log.LogInformation("Loading Estates");
            if (xmlStream == null)
                return default;

            CreateCsv(xmlStream, buildingsCsv);
            await app.DataLake.SaveCsvAsync(csv, "Refined", "Estates.csv", FolderStructure.DatePath);
            app.Mssql.InserCsv(csv, "Estates", true, false);
            return csv;
        }

        private static void CreateCsv(Stream stream, Csv buildingsCsv)
        {
            stream.Position = 0;
            var data = XDocument.Load(stream);

            var r = 1;
            var genericHelper = new GenericHelper();
            foreach (var item in data.Root.Descendants("Estate"))
            {
                genericHelper.AddAttributes(r, item, csv);
                AddGIS(r, item);
                genericHelper.AddLayerDatas(r, item, csv);
                r++;
            }
            
            AddEstatesBuildingsCountColumn(buildingsCsv);
        }

        private static void AddGIS(int r, XElement element)
        {
            var elements = element.Elements("Lot");
            if (!elements.Any())
                elements = element.Elements("Building");

            var coords = new List<(double Lat, double Lon)>();
            foreach (var item in elements)
                foreach (var coord in item.Element("GIS").Element("OuterPolygon").Elements("Coordinate"))
                    coords.Add(((double)coord.Attribute("x"), (double)coord.Attribute("y")));

            if (GIS.GetGISGravityPoint(coords, out (double Lat, double Lon) gps))
            {
                csv.AddHeader("GISLat", false, out int latCol);
                csv.AddHeader("GISLon", false, out int lonCol);
                csv.AddRecord(r, latCol, gps.Lat);
                csv.AddRecord(r, lonCol, gps.Lon);
            }
        }

        private static void AddEstatesBuildingsCountColumn(Csv buildingsCsv)
        {
            var estateCol_EstateBuildings = csv.Headers.Count;
            csv.AddHeader(estateCol_EstateBuildings, "Buildings");

            if (!csv.TryGetColId("MasterID", out int estateCol_EstateMasterID) || !buildingsCsv.TryGetColId("EstateMasterID", out int buildingCol_EstateMasterID))
                throw new Exception("Error must be handled by programmer.");

            var estateMasterIds = csv.Records.Where(o => o.Key.Col.Equals(estateCol_EstateMasterID)).ToDictionary(o => o.Key.Row, o => o.Value);
            var buildingEstateMasterIds = buildingsCsv.Records.Where(o => o.Key.Col.Equals(buildingCol_EstateMasterID)).ToDictionary(o => o.Key.Row, o => o.Value);

            foreach (var estateMasterRecord in estateMasterIds)
            {
                var buildingCounts = buildingEstateMasterIds.Count(o => o.Value.Equals(estateMasterRecord.Value));
                csv.AddRecord(estateMasterRecord.Key, estateCol_EstateBuildings, buildingCounts.ToString());
            }
        }
    }
}
