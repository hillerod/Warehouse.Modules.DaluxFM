using Bygdrift.Warehouse.Modules;
using System.IO;
using System.Xml.Linq;

namespace Warehouse.Modules.DaluxFM.Refine
{
    public class LotsRefine : RefineBase
    {
        public XDocument Data { get; set; }

        public LotsRefine(IImporter exporter, Stream xmlStream) : base(exporter, "lots")
        {
            xmlStream.Position = 0;
            Data = XDocument.Load(xmlStream);
            Refine();
        }

        public override void Refine()
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
