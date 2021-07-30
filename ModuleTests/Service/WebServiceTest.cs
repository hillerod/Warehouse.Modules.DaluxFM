using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.Service;
using ModuleTests.Helpers;
using System.IO;

namespace ModuleTests.Service
{
    [TestClass]
    public class WebServiceTest : GenericTest
    {
        private readonly WebService daluxFM;

        public WebServiceTest()
        {
            daluxFM = new WebService(Config["DaluxFMCustomerId"], Config["DaluxFMApiKey"], Config["DaluxFMUser"], Config["DaluxFMPassword"]);
        }

        [TestMethod]
        public void GetAssets()
        {
            using var fileStream = new FileStream(Path.Combine(BasePath, "Files", "DaluxFM", "In", "Assets.xml"), FileMode.Create, FileAccess.Write);
            daluxFM.GetAssets().Result.CopyTo(fileStream);
        }

        [TestMethod]
        public void GetEstates()
        {
            using var fileStream = new FileStream(Path.Combine(BasePath, "Files", "DaluxFM", "In", "Estates.xml"), FileMode.Create, FileAccess.Write);
            daluxFM.GetEstates().Result.CopyTo(fileStream);
        }
    }
}
