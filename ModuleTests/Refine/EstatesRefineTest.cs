using Bygdrift.Warehouse.DataLake.CsvTools;
using Bygdrift.Warehouse.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.Refine;
using ModuleTests.Helpers;
using System.IO;

namespace ModuleTests.Refine
{
    [TestClass]
    public class EstatesRefineTest : GenericTest
    {

        [TestMethod]
        public void EstatesRefine()
        {
            using var estatesStream = new FileStream(Path.Combine(BasePath, "Files", "DaluxFM", "In", "Estates.xml"), FileMode.Open);
            
            var exporter = new ImportBase(null, null, "DaluxFM", "0 * * * *", null);
            var buildingsRefine = new BuildingsRefine(exporter, estatesStream);
            var estatesRefine = new EstatesRefine(exporter, estatesStream, buildingsRefine);
            var lotsRefine = new LotsRefine(exporter, estatesStream);

            Assert.IsFalse(estatesRefine.HasErrors);
            Assert.IsFalse(buildingsRefine.HasErrors);
            Assert.IsFalse(lotsRefine.HasErrors);

            buildingsRefine.CsvSet.Write(Path.Combine(BasePath, "Files", "DaluxFM", "Out", "buildings.csv"));
            estatesRefine.CsvSet.Write(Path.Combine(BasePath, "Files", "DaluxFM", "Out", "estates.csv"));
            lotsRefine.CsvSet.Write(Path.Combine(BasePath, "Files", "DaluxFM", "Out", "lots.csv"));
        }
    }
}
