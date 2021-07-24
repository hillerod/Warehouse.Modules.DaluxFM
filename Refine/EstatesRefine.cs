using Bygdrift.Warehouse.Modules;
using Bygdrift.Warehouse.DataLake.CsvTools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Warehouse.Modules.DaluxFM.Refine
{
    public class EstatesRefine : RefineBase
    {
        public XDocument Data { get; set; }
        private readonly IRefine buildings;

        public EstatesRefine(IImporter exporter, Stream xmlStream, IRefine buildings) : base(exporter, "estates")
        {
            xmlStream.Position = 0;
            Data = XDocument.Load(xmlStream);
            this.buildings = buildings;
            Refine();
        }

        public override void Refine()
        {
            var r = 0;
            CsvSet.AddHeader("latitude", out int estLatCol);
            CsvSet.AddHeader("longitude", out int estLonCol);
            var genericHelper = new GenericHelper();
            foreach (var item in Data.Root.Descendants("Estate"))
            {
                genericHelper.AddAttributes(r, item, CsvSet);
                genericHelper.AddLayerDatas(r, item, CsvSet);
                AddEstateGps(ref estLatCol, ref estLonCol, r, item);
                r++;
            }

            AddEstatesBuildingsCountColumn(buildings);
        }

        private void AddEstateGps(ref int latCol, ref int lonCol, int r, XElement element)
        {
            var elements = element.Elements("Lot");
            if (!elements.Any())
                elements = element.Elements("Building");

            var coords = new List<(float Lat, float Lon)>();
            foreach (var item in elements)
                foreach (var coord in item.Element("GIS").Element("OuterPolygon").Elements("Coordinate"))
                    coords.Add(((float)coord.Attribute("x"), (float)coord.Attribute("y")));

            if (GenericHelper.GetGravityPoint(coords, out (float Lat, float Lon) gps))
            {
                CsvSet.AddRecord(latCol, r, gps.Lat);
                CsvSet.AddRecord(lonCol, r, gps.Lon);
            }
        }

        private void AddEstatesBuildingsCountColumn(IRefine buildings)
        {
            var estateCol_EstateBuildings = CsvSet.Headers.Count;
            CsvSet.AddHeader(estateCol_EstateBuildings, "Buildings");

            if (!CsvSet.TryGetRecordCol("MasterID", out int estateCol_EstateMasterID) || !buildings.CsvSet.TryGetRecordCol("EstateMasterID", out int buildingCol_EstateMasterID))
                throw new System.Exception("Error must be handled by programmer.");

            var estateMasterIds = CsvSet.Records.Where(o => o.Key.Col.Equals(estateCol_EstateMasterID)).ToDictionary(o => o.Key.Row, o => o.Value);
            var buildingEstateMasterIds = buildings.CsvSet.Records.Where(o => o.Key.Col.Equals(buildingCol_EstateMasterID)).ToDictionary(o => o.Key.Row, o => o.Value);

            foreach (var estateMasterRecord in estateMasterIds)
            {
                var buildingCounts = buildingEstateMasterIds.Count(o => o.Value.Equals(estateMasterRecord.Value));
                CsvSet.AddRecord(estateCol_EstateBuildings, estateMasterRecord.Key, buildingCounts.ToString());
            }
        }

        //public static (IRefine Estates, IRefine Buildings, IRefine Lots) Refine(string moduleName, Stream xmlStream)
        //{
        //    var estates = new ExportEntity(moduleName, "estates");
        //    var buildings = new ExportEntity(moduleName, "buildings");
        //    var lots = new ExportEntity(moduleName, "lots");

        //    xmlStream.Position = 0;
        //    var doc = XDocument.Load(xmlStream);
        //    var r = 0;
        //    estates.CsvSet.AddHeader("latitude", out int estLatCol);
        //    estates.CsvSet.AddHeader("longitude", out int estLonCol);
        //    var daluxGenericWash = new GenericHelper();
        //    foreach (var item in doc.Root.Descendants("Estate"))
        //    {
        //        daluxGenericWash.AddAttributes(r, item, estates.CsvSet);
        //        daluxGenericWash.AddLayerDatas(r, item, estates.CsvSet);
        //        AddEstateGps(ref estLatCol, ref estLonCol, r, item, estates.CsvSet);
        //        r++;
        //    }

        //    r = 0;
        //    buildings.CsvSet.AddHeader("latitude", out int buildLatCol);
        //    buildings.CsvSet.AddHeader("longitude", out int buildLonCol);
        //    daluxGenericWash = new GenericHelper();
        //    var buildingElements = doc.Root.Descendants("Building");
        //    foreach (var item in buildingElements)
        //    {
        //        daluxGenericWash.AddAttributes(r, item, buildings.CsvSet, new string[] { "GisPolygon" });
        //        daluxGenericWash.AddLayerDatas(r, item, buildings.CsvSet);
        //        AddBuildingGps(ref buildLatCol, ref buildLonCol, r, item, buildings.CsvSet);
        //        r++;
        //    }

        //    r = 0;
        //    daluxGenericWash = new GenericHelper();
        //    foreach (var item in doc.Root.Descendants("Lot"))
        //    {
        //        daluxGenericWash.AddAttributes(r, item, lots.CsvSet, new string[] { "GisPolygon" });
        //        daluxGenericWash.AddLayerDatas(r, item, lots.CsvSet);
        //        r++;
        //    }

        //    AddEstatesBuildingsCountColumn(ref estates, buildings);

        //    return (estates, buildings, lots);
        //}



        //private static void AddBuildingGps(ref int latCol, ref int lonCol, int r, XElement element, CsvSet csv)
        //{
        //    var coords = new List<(float Lat, float Lon)>();
        //    foreach (var coord in element.Element("GIS").Element("OuterPolygon").Elements("Coordinate"))
        //        coords.Add(((float)coord.Attribute("x"), (float)coord.Attribute("y")));

        //    if (GetGravityPoint(coords, out (float Lat, float Lon) gps))
        //    {
        //        csv.AddRecord(latCol, r, gps.Lat);
        //        csv.AddRecord(lonCol, r, gps.Lon);
        //    }
        //}




    }
}
