using Bygdrift.Warehouse.Modules;
using System;
using System.IO;
using System.Xml.Linq;

namespace Module.Refine
{
    public class LotsRefine : RefineBase
    {
        public XDocument Data { get; set; }

        public LotsRefine(ImportBase importer, Stream xmlStream) : base(importer, "Lots")
        {
            xmlStream.Position = 0;
            Data = XDocument.Load(xmlStream);
            CreateCsv();
            ImportCsvFileToDataLake(DateTime.UtcNow);
        }

        public void CreateCsv()
        {
            var r = 0;
            var genericHelper = new GenericHelper();
            foreach (var item in Data.Root.Descendants("Lot"))
            {
                genericHelper.AddAttributes(r, item, CsvSet, new string[] { "GisPolygon" });
                genericHelper.AddLayerDatas(r, item, CsvSet);
                r++;
            }
        }
    }
}
