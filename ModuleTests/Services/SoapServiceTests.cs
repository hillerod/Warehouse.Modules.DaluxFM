using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.Services;
using System.Threading.Tasks;

namespace ModuleTests.Services
{
    [TestClass]
    public class SoapServiceTests
    {
        private readonly SoapService daluxFM;

        public SoapServiceTests()
        {
            daluxFM = new SoapService(new AppBase<Module.Settings>());
        }

        [TestMethod]
        public async Task GetAssets()
        {
            var stream = await daluxFM.GetAssetsAsync();
            await daluxFM.App.DataLake.SaveStreamAsync(stream, "Raw", "Assets.xml", FolderStructure.DatePath);
        }

        [TestMethod]
        public async Task GetEstates()
        {
            var stream = await daluxFM.GetEstatesAsync();
            await daluxFM.App.DataLake.SaveStreamAsync(stream, "Raw", "Estates.xml", FolderStructure.DatePath);
        }

        [TestMethod]
        public async Task GetBuildingDrawings()
        {
            var stream2020 = await daluxFM.GetBuildingDrawingAsync(2020);
            await daluxFM.App.DataLake.SaveStreamAsync(stream2020, "Raw", "buildingDrawing 2020.pdf", FolderStructure.DatePath);
        }
    }
}
