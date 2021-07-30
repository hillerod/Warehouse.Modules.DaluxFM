using Bygdrift.Warehouse.DataLake.CsvTools;
using Bygdrift.Warehouse.Modules;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleTests.Helpers;
using Moq;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

/// <summary>
/// To use this test, fill out 'ModuleTests/appsettings.json'
/// You can upload data directly to Azure, by stting saveToServer = true
/// You can fetch data from Dalux, by setting useDataFromService = true.
/// It takes some time to fetch data each time, so you can instead download the two xml-files to a folder: 'ModuleTests/Files/In/' and set useDataFromService=false.
/// Get the two xml-files, by using: ModuleTests.Service.WebServiceTest.GetAssets() and GetEstates()
/// </summary>

namespace ModuleTests
{
    [TestClass]
    public class ImporterTest : GenericTest
    {
        [TestMethod]
        public void TestRunModule()
        {
            bool saveToServer = false;
            bool useDataFromService = false;
            var loggerMock = new Mock<ILogger>();
            
            ImportResult res;
            if (useDataFromService)
            {
                var importer = new Module.Importer(Config, loggerMock.Object);
                var refines = importer.GetRefines();
                res = importer.ImportToDataLake(refines, saveToServer);
            }
            else
            {
                using var assetsStream = new FileStream(Path.Combine(BasePath, "Files", "In", "Assets.xml"), FileMode.Open);
                using var estatesStream = new FileStream(Path.Combine(BasePath, "Files", "In", "Estates.xml"), FileMode.Open);
                var importer = new Module.Importer(Config, loggerMock.Object, estatesStream, assetsStream);
                var refines = importer.GetRefines();
                res = importer.ImportToDataLake(refines, saveToServer);
            }

            var errors = res.Refines.Where(o => o.HasErrors);
            Assert.IsFalse(errors.Any());
            Assert.IsTrue(res.AppSettingsOk);
            Assert.IsTrue(res.CMDModel != null);
            Assert.IsTrue(res.ImportLog != null);
            Assert.IsFalse(loggerMock.Invocations.Any(o => (LogLevel)o.Arguments[0] == LogLevel.Error));

            if (saveToServer)
                return;

            foreach (var item in res.Refines)
                item.CsvSet.Write(Path.Combine(BasePath, "Files", "Out", item.TableName + ".csv"));

            File.WriteAllText(Path.Combine(BasePath, "Files", "Out", "Model.json"), JsonConvert.SerializeObject(res.CMDModel, Formatting.Indented));

            res.ImportLog.Write(Path.Combine(BasePath, "Files", "Out", "ImportLog.csv"));
        }
    }
}