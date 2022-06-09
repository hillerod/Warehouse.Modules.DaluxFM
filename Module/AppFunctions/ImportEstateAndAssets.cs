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
        public async Task ImportEstateAndAssetsAsync([TimerTrigger("%ScheduleImportEstatesAndAssets%")] TimerInfo myTimer)
        {
            App.Log.LogInformation("Start importing Estates and assets...");
            var service = new SoapService(App);
            using var estatesXmlStream = new MemoryStream();
            using var assetsXmlStream = new MemoryStream();

            App.Log.LogInformation("Loading data from Dalux");


            if (!LoadFromDataLake("Raw", "Estates.xml", FolderStructure.DatePath, estatesXmlStream))
            {
                var streamIn = await service.GetEstatesAsync();
                if (streamIn.Length > 200)
                {
                    streamIn.CopyTo(estatesXmlStream);
                    await App.DataLake.SaveStreamAsync(estatesXmlStream, "Raw", "Estates.xml", FolderStructure.DatePath);
                }
                else
                    App.Log.LogError("The xml from Estates was very short:", estatesXmlStream.Length);
            }

            if (!LoadFromDataLake("Raw", "Assets.xml", FolderStructure.DatePath, assetsXmlStream))
            {
                var streamIn = await service.GetAssetsAsync();
                if (streamIn.Length > 200)
                {
                    streamIn.CopyTo(assetsXmlStream);
                    await App.DataLake.SaveStreamAsync(assetsXmlStream, "Raw", "Assets.xml", FolderStructure.DatePath);
                }
                else
                    App.Log.LogError("The xml from Assets was very short:", estatesXmlStream.Length);
            }

            var drawingsAreImported = !string.IsNullOrEmpty(App.Settings.DownloadFileApiKey);  //Determines if drawings, frequently will be uploadet to datalake
            var buildingsRefineCsv = await BuildingsRefine.RefineAsync(App, estatesXmlStream, drawingsAreImported, App.HostName);
            //var estatesRefineCsv = await EstatesRefine.RefineAsync(App, estatesXmlStream, buildingsRefineCsv);
            //await LotsRefine.RefineAsync(App, estatesXmlStream);
            //await RoomsRefine.RefineAsync(App, estatesXmlStream);
            //await AssetsRefine.RefineAsync(App, assetsXmlStream, estatesRefineCsv, buildingsRefineCsv);
            App.Mssql.Dispose();
            App.Log.LogInformation("Importing Estates and assets completed.");
        }

        private bool LoadFromDataLake(string basePath, string fileName, FolderStructure folderStructure, MemoryStream stream)
        {
            if (App.DataLake.FileExist(basePath, fileName, folderStructure))
            {
                var streamIn = App.DataLake.GetFile(basePath, fileName, folderStructure);
                if (streamIn.Length > 200)
                {
                    streamIn.Stream.CopyTo(stream);
                    return true;
                }
                else
                    App.Log.LogError($"The xml from {fileName} was very short:", streamIn.Length);
            }
            return false;
        }
    }
}
