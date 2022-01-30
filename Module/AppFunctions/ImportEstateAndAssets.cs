using Bygdrift.DataLakeTools;
using Bygdrift.Warehouse;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Module.Refines;
using Module.Services;
using System.IO;
using System.Threading.Tasks;

namespace Module.AppFunctions
{
    public class ImportEstateAndAssets
    {
        public ImportEstateAndAssets(ILogger<ImportEstateAndAssets> logger) => App = new AppBase<Settings>(logger);

        public AppBase<Settings> App { get; }

        [FunctionName(nameof(ImportEstateAndAssetsAsync))]
        public async Task ImportEstateAndAssetsAsync([TimerTrigger("%Setting--ScheduleImportEstatesAndAssets%")] TimerInfo myTimer)
        {
            App.Log.LogInformation("Start importing Estates and assets...");
            var service = new SoapService(App);
            using var estatesXmlStream = new MemoryStream();
            using var assetsXmlStream = new MemoryStream();

            App.Log.LogInformation("Loading data from Dalux");
            if (App.DataLake.FileExist("Raw", "Estates.xml", FolderStructure.DatePath))
            {
                var streamIn = App.DataLake.GetFile("Raw", "Estates.xml", FolderStructure.DatePath);
                if (streamIn.Length > 0)
                    streamIn.Stream.CopyTo(estatesXmlStream);
            }
            else
            {
                var streamIn = await service.GetEstatesAsync();
                streamIn.CopyTo(estatesXmlStream);
                await App.DataLake.SaveStreamAsync(estatesXmlStream, "Raw", "Estates.xml", FolderStructure.DatePath);
            }

            if (App.DataLake.FileExist("Raw", "Assets.xml", FolderStructure.DatePath))
            {
                var streamIn = App.DataLake.GetFile("Raw", "Assets.xml", FolderStructure.DatePath);
                if (streamIn.Length > 0)
                    streamIn.Stream.CopyTo(assetsXmlStream);
            }
            else
            {
                var streamIn = await service.GetAssetsAsync();
                streamIn.CopyTo(assetsXmlStream);
                await App.DataLake.SaveStreamAsync(assetsXmlStream, "Raw", "Assets.xml", FolderStructure.DatePath);
            }

            var drawingsAreImported = !string.IsNullOrEmpty(App.Settings.DownloadFileApiKey);  //Determines if drawings, frequently will be uploadet to datalake
            var buildingsRefineCsv = await BuildingsRefine.RefineAsync(App, estatesXmlStream, drawingsAreImported, App.HostName);
            var estatesRefineCsv = await EstatesRefine.RefineAsync(App, estatesXmlStream, buildingsRefineCsv);
            await LotsRefine.RefineAsync(App, estatesXmlStream);
            await RoomsRefine.RefineAsync(App, estatesXmlStream);
            await AssetsRefine.RefineAsync(App, assetsXmlStream, estatesRefineCsv, buildingsRefineCsv);
            App.Log.LogInformation("Importing Estates and assets completed.");
        }
    }
}
