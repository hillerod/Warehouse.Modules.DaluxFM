using Bygdrift.Warehouse;
using DaluxFMService;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Module.Services
{
    public class SoapService
    {
        private readonly ExternalDataAccessServiceSoapClient daluxFMSoap;
        private readonly string xmlRoot = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n";

        public AppBase<Settings> App { get; }

        public SoapService(AppBase<Settings> app)
        {
            daluxFMSoap = new ExternalDataAccessServiceSoapClient(ExternalDataAccessServiceSoapClient.EndpointConfiguration.ExternalDataAccessServiceSoap);
            daluxFMSoap.InnerChannel.OperationTimeout = new TimeSpan(0, 15, 0);
            App = app;
        }

        public async Task<Stream> GetAssetsAsync()
        {
            var data = await daluxFMSoap.GetFullBuildingPartDataExtract2Async(App.Settings.DaluxCustomerId, App.Settings.DaluxFMSoapKey, App.Settings.DaluxUser + ":" + App.Settings.DaluxPassword);
            return new MemoryStream(Encoding.UTF8.GetBytes(xmlRoot + data));
        }

        public async Task<Stream> GetEstatesAsync()
        {
            var data = await daluxFMSoap.GetBuildingDataExtract2Async(App.Settings.DaluxCustomerId, App.Settings.DaluxFMSoapKey, App.Settings.DaluxUser + ":" + App.Settings.DaluxPassword);
            return new MemoryStream(Encoding.UTF8.GetBytes(xmlRoot + data));
        }

        public async Task<Stream> GetBuildingDrawingAsync(int buildingId)
        {
            for (int i = 0; i < 3; i++)
            {
                string link = await daluxFMSoap.GetDrawingPrintAsync(App.Settings.DaluxCustomerId, App.Settings.DaluxFMSoapKey, App.Settings.DaluxUser + ":" + App.Settings.DaluxPassword, buildingId);
                if (link != string.Empty && link != "Unknown service error")
                {
                    if (link == "Building has no drawings")
                        return null;
                    try
                    {
                        var uri = new Uri(link);
                        using var webClient = new WebClient();
                        var bytes = webClient.DownloadData(link);
                        return new MemoryStream(bytes);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        //public async Task<Stream> GetRooms()
        //{
        //    var data = await daluxFMSoap.GetRoomDataExtract2Async(customerId, apiKey, user + ":" + password);
        //    return new MemoryStream(Encoding.UTF8.GetBytes(xmlRoot + data));
        //}
    }
}
