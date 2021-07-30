using Bygdrift.Warehouse.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Module.Refine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Module
{
    public class Importer : ImportBase
    {
        private static readonly string moduleName = "FM.DaluxFM";
        private static readonly string[] mandatoryAppSettings = new string[] {
            "ScheduleExpression", "DataLakeAccountName", "DataLakeAccountKey", "DataLakeServiceUrl", "DataLakeBasePath",
            "DaluxFMCustomerId", "DaluxFMApiKey", "DaluxFMUser", "DaluxFMPassword", "DaluxFMUniqueAssetColumns"
        };
        private Stream estatesXmlStream;
        private Stream assetsXmlStream;

        public Importer(IConfigurationRoot config, ILogger log) : base(config, log, moduleName, config["ScheduleExpression"], mandatoryAppSettings) { }

        /// <param name="data">[0]: estatesXmlStream, [1]: assetsXmlStream</param>
        public Importer(IConfigurationRoot config, ILogger log, params object[] data) : base(config, log, moduleName, config["ScheduleExpression"], mandatoryAppSettings)
        {
            estatesXmlStream = data[0] as Stream;
            assetsXmlStream = data[1] as Stream;
        }

        public List<RefineBase> GetRefines()
        {
            var service = new Service.WebService(Config["DaluxFMCustomerId"], Config["DaluxFMApiKey"], Config["DaluxFMUser"], Config["DaluxFMPassword"]);
            if (estatesXmlStream == null)
                estatesXmlStream = service.GetEstates().Result;
            if (assetsXmlStream == null)
                assetsXmlStream = service.GetAssets().Result;

            var buildingsRefine = new BuildingsRefine(this, estatesXmlStream);
            var estatesRefine = new EstatesRefine(this, estatesXmlStream, buildingsRefine);
            var lotsRefine = new LotsRefine(this, estatesXmlStream);
            var assetsRefine = new AssetsRefine(this, assetsXmlStream, estatesRefine, buildingsRefine, Config["DaluxFMUniqueAssetColumns"]);

            return new List<RefineBase> { estatesRefine, buildingsRefine, lotsRefine, assetsRefine };
        }
    }
}