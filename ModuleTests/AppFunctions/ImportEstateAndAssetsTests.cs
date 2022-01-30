using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.AppFunctions;
using Moq;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleTests.AppFunctions
{
    [TestClass]

    public class ImportEstateAndAssetsTests
    {
        private readonly ImportEstateAndAssets function;

        public ImportEstateAndAssetsTests()
        {
            Mock<ILogger<ImportEstateAndAssets>> loggerMock = new();
            function = new ImportEstateAndAssets(loggerMock.Object);
        }

        [TestMethod]
        public async Task ImportEstateAndAssetsAsync()
        {
            await function.ImportEstateAndAssetsAsync(default(TimerInfo));
            var errors = function.App.Log.GetErrorsAndCriticals();
            Assert.IsTrue(errors.Count() == 0);
        }
    }
}
