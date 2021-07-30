using DaluxFMService;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Module.Service
{
    public class WebService
    {
        private readonly ExternalDataAccessServiceSoapClient daluxFMSoap;
        private readonly string customerId;
        private readonly string apiKey;
        private readonly string user;
        private readonly string password;
        private readonly string xmlRoot = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n";

        public WebService(string customerId, string apiKey, string user, string password)
        {
            daluxFMSoap = new ExternalDataAccessServiceSoapClient(ExternalDataAccessServiceSoapClient.EndpointConfiguration.ExternalDataAccessServiceSoap);
            daluxFMSoap.InnerChannel.OperationTimeout = new TimeSpan(0, 15, 0);
            this.customerId = customerId;
            this.apiKey = apiKey;
            this.user = user;
            this.password = password;
        }

        public async Task<Stream> GetAssets()
        {
            var data = await daluxFMSoap.GetFullBuildingPartDataExtract2Async(customerId, apiKey, user + ":" + password);
            return new MemoryStream(Encoding.UTF8.GetBytes(xmlRoot + data));
        }

        public async Task<Stream> GetEstates()
        {
            var data = await daluxFMSoap.GetBuildingDataExtract2Async(customerId, apiKey, user + ":" + password);
            return new MemoryStream(Encoding.UTF8.GetBytes(xmlRoot + data));
        }
    }
}
