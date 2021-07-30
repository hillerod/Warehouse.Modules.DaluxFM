using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ModuleTests.Helpers;
using Bygdrift.Warehouse.Modules;
using Module.Refine;
using Bygdrift.Warehouse.DataLake.CsvTools;

namespace ModuleTests.Refine
{
    [TestClass]
    public class AssetsRefineTest : GenericTest
    {

        [TestMethod]
        public void AssetsRefine()
        {
            using var estatesStream = new FileStream(Path.Combine(BasePath, "Files", "DaluxFM", "In", "Estates.xml"), FileMode.Open);
            using var assetsStream = new FileStream(Path.Combine(BasePath, "Files", "DaluxFM" ,"In", "Assets.xml"), FileMode.Open);

            var exporter = new ImportBase(null, null, "DaluxFM", "0 * * * *", null);
            var buildingsRefine = new BuildingsRefine(exporter, estatesStream);
            var estatesRefine = new EstatesRefine(exporter, estatesStream, buildingsRefine);
            var assetsRefine = new AssetsRefine(exporter, assetsStream, estatesRefine, buildingsRefine, "Aftagernummer");

            Assert.IsFalse(assetsRefine.HasErrors);
            Assert.IsFalse(estatesRefine.HasErrors);
            Assert.IsFalse(buildingsRefine.HasErrors);

            assetsRefine.CsvSet.Write(Path.Combine(BasePath, "Files", "DaluxFM", "Out", "assets.csv"));
        }
    }
}
