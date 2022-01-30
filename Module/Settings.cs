using Bygdrift.Warehouse.Helpers.Attributes;
using Newtonsoft.Json;

namespace Module
{
    public class Settings
    {
        /// <summary>ID used when loging in on Dalux. In Hillerød kommune, it is hillerod</summary>
        [ConfigSecret(NotSet = NotSet.ThrowError)]
        public string DaluxCustomerId { get; set; }

        /// <summary>An ID that you can get by contacting Dalux and say that it is to their web service. The Id would look like: f07axb-ex28-49ax-b3xd-faxa01217dd5"</summary>
        [ConfigSecret(NotSet = NotSet.ThrowError)]
        public string DaluxFMSoapKey { get; set; }

        /// <summary>The password on the user</summary>
        [ConfigSecret(NotSet = NotSet.ThrowError)]
        public string DaluxPassword { get; set; }

        /// <summary>Mail on a user that has access to Dalux</summary>
        [ConfigSecret(NotSet = NotSet.ThrowError)]
        public string DaluxUser { get; set; }

        /// <summary>A string, made by your own fantasy that is used in all links used to download files from the datalake through a appFunction service</summary>
        [ConfigSecret]
        public string DownloadFileApiKey { get; set; }

        /// <summary>If not set, drawings will not be fetched. The operation takes quite a while, so don't do this each day</summary>
        [ConfigSetting(Default = 7 )]
        public int DaysBetweenLoadingDrawings { get; set; }

        /// <summary>Like: https://rpa.azurewebsites.net</summary>
        [ConfigSetting]
        public string RpaHostName { get; set; }

        /// <summary>If True, and RpaBasePAth is set, it will try to get Dalux connectiontime each hour</summary>
        [ConfigSetting]
        public bool RpaGetDaluxConnectionTime { get; set; }

        /// <summary>
        /// You can ommit this or you can add name on headers from Assets, that you would like a check on, to see if all content is unique. 
        /// If you have multiple columns, then seperate the names with a comma
        /// </summary>
        //[ConfigSetting]
        //public string UniqueAssetColumns { get; set; }
    }
}
