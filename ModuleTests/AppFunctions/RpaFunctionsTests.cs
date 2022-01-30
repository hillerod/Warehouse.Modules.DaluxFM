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

    public class RpaFunctionsTests
    {
        //private readonly Mock<ILogger<RpaFunctions>> loggerMock = new();
        //private readonly RpaFunctions function;

        //public RpaFunctionsTests()
        //{
        //    function = new RpaFunctions(loggerMock.Object);
        //}

        //[TestMethod]
        //public async Task RpaGetDaluxConnectionTimeAsync()
        //{
        //    //var a = function.App;
        //    //await function.RpaGetDaluxConnectionTimeAsync(default(TimerInfo));
        //    //var errors = function.App.Log.GetErrors();
        //    //Assert.IsTrue(errors.Count() == 0);
        //}
    }
}
