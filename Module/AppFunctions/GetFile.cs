using Bygdrift.PdfToDrawio;
using Bygdrift.Warehouse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Module.Services;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Module.AppFunctions
{
    public class GetFile
    {
        public GetFile(ILogger<GetFile> logger) => App = new AppBase<Settings>(logger);
        
        public AppBase<Settings> App { get; }

        /// <summary>
        /// Gets files from dataLake
        /// Example on a call: http://localhost:7071/api/getfile/Drawings%2Fbuildings%2F2020.pdf?raw=1&apikey=6AC63FF1-F9C0-44A0-B023-8C7769D5C846
        /// </summary>
        /// <param name="raw">If true or 1, then file comes back as string. If omitted or 0 or false, file will come for download</param>
        /// <param name="filePath">Url encoded datalake path like: Drawings%2Fbuildings%2F2020.pdf</param>
        /// <returns></returns>
        [FunctionName(nameof(GetFilesAsync))]
        public async Task<IActionResult> GetFilesAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getFile")] HttpRequest req)
        {
            string apiKey = req.Query["apikey"];
            string raw = req.Query["raw"];
            string filePath = req.Query["filePath"];
            string url = string.Join("/", req.Scheme,"", req.Host, req.Path.ToString().Trim('/'), req.QueryString);

            if (!App.Settings.DownloadFileApiKey.Equals(apiKey, StringComparison.OrdinalIgnoreCase))
                return new BadRequestObjectResult(string.IsNullOrEmpty(apiKey) ? "Apikey is missing." : "Apikey is not correct.");

            App.Log.LogInformation($"'{url}' was called. ApiKey was OK.");

            if (string.IsNullOrEmpty(filePath))
                return new NotFoundObjectResult("You have to set filePath and it has to be url encoded. It must contain the path to the file on datalake like 'basepath'/'filename'. Could be: 'Refined/2021/10/21/data.csv' and remember to url encode. The search is case sensitive.");

            filePath = HttpUtility.UrlDecode(filePath).TrimStart('/');
            var (stream, basePath, fileName, fileLength, lastModified) = App.DataLake.GetFile(filePath);
            if (basePath != null && basePath == "Drawings/Buildings")  //Case sensitve comparison
                stream = await VerifyDrawingAsync(App, stream, fileName, lastModified);

            if (stream == null || stream.Length == 0)
            {
                var basePathExistsOnDataLake = App.DataLake.BasePathExists(basePath);
                if (basePathExistsOnDataLake)
                    return new NotFoundObjectResult($"You have parsed this basepath that exists on the datalake: '{basePath}' and this filename: '{fileName}' that does not exist. The search is case sensitive.");
                else
                    return new NotFoundObjectResult($"You have parsed this basepath: '{basePath}' and this filename: '{fileName}'. The basepath does not exist on the datalake. The search is case sensitive.");
            }

            if (!string.IsNullOrEmpty(raw) && (raw.ToLower() == "true" || raw == "1"))
                return new FileContentResult(StreamToByte(stream), "application/octet-stream");
            else
                return new FileContentResult(StreamToByte(stream), "application/octet-stream") { FileDownloadName = fileName };
        }

        /// <summary>
        /// Drawings are loaded dynamically when somone asks for them. If they should be downloaded each week, it would take over an hour and a functionApp can max be run for 10 minutes. Therefor this method
        /// </summary>
        public async Task<Stream> VerifyDrawingAsync(AppBase<Settings> app, Stream stream, string fileName, DateTime? lastModified)
        {
            if (stream != null && ((DateTime)lastModified).AddDays(app.Settings.DaysBetweenLoadingDrawings) >= DateTime.Now)
                return stream;

            var extension = Path.GetExtension(fileName).ToLower();
            if (extension != ".pdf" && extension != ".drawio")
                return null;

            if (!int.TryParse(Path.GetFileNameWithoutExtension(fileName), out int buildingId))
                return null;

            var service = new SoapService(app);
            var pdfStream = await service.GetBuildingDrawingAsync(buildingId);

            if (pdfStream != null)
            {
                using var memoryStream = new MemoryStream();  //Saving to memorystream so stream is not being closed
                pdfStream.CopyTo(memoryStream);

                var fileNamePdf = buildingId + ".pdf";
                await app.DataLake.SaveStreamAsync(pdfStream, "Drawings/Buildings", fileNamePdf, Bygdrift.Tools.DataLakeTool.FolderStructure.DatePath);

                var pdfToDrawio = new Bygdrift.PdfToDrawio.Convert(memoryStream, Format.PDF);
                var fileNameDrawio = buildingId + ".drawio";
                var drawioStream = pdfToDrawio.ToDrawIo();
                await app.DataLake.SaveStreamAsync(drawioStream, "Drawings/Buildings", fileNameDrawio, Bygdrift.Tools.DataLakeTool.FolderStructure.DatePath);

                if (extension == ".drawio")
                {
                    pdfStream.Close();
                    drawioStream.Position = 0;
                    return drawioStream;
                }
                else
                {
                    pdfStream.Position = 0;
                    drawioStream.Close();
                    return pdfStream;
                }
            }
            return null;
        }

        private static byte[] StreamToByte(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
