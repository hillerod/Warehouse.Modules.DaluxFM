using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ModuleTests.Helpers
{
    public class GenericTest
    {
        /// <summary>Get data from appSettings like Config["test"]</summary>
        public IConfigurationRoot Config;

        /// <summary>Path to project base</summary>
        public static string BasePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));

        public GenericTest()
        {
            Config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        }
    }
}