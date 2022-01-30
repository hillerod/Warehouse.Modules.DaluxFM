using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Module.AppFunctions;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModuleTests.AppFunctions
{
    [TestClass]

    public class ApiGetFileTests
    {
        private readonly GetFile function;

        public ApiGetFileTests()
        {
            Mock<ILogger<GetFile>> loggerMock = new();
            function = new GetFile(loggerMock.Object);
        }

        [TestMethod]
        public async Task GetEstatesAndAssets()
        {
            var paramsDictionary = new Dictionary<string, StringValues> {
                { "apikey", function.App.Settings.DaluxFMSoapKey } ,
                { "raw", "1" } ,
                { "filePath", "Drawings%2FBuildings%2F2020.pdf" }
            };

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(i => i.Query).Returns(new QueryCollection(paramsDictionary));
            var res = await function.GetFilesAsync(requestMock.Object);
        }
    }
}
