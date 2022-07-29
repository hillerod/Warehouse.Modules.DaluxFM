using Bygdrift.Tools.CsvTool;
using Bygdrift.Tools.DataLakeTool;
using Bygdrift.Warehouse;
using Module.Refines.Helpers;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Module.Refines
{
    public class RoomsRefine
    {
        private static readonly Csv csv = new();

        public static async Task<Csv> RefineAsync(AppBase app, Stream xmlStream)
        {
            app.Log.LogInformation("Loading Rooms");
            if (xmlStream == null)
                return default;

            CreateCsv(xmlStream);
            await app.DataLake.SaveCsvAsync(csv, "Refined", "Rooms.csv", FolderStructure.DatePath);
            app.Mssql.InsertCsv(csv, "Rooms", true);
            return csv;
        }

        private static void CreateCsv(Stream stream)
        {
            stream.Position = 0;
            var data = XDocument.Load(stream);
            var r = 1;
            var genericHelper = new GenericHelper();

            foreach (var building in data.Root.Descendants("Building"))
            {
                var EstateMasterId = (int)building.Attribute("EstateMasterID");
                csv.AddHeader("EstateMasterID", false, out int estateMasterIdCol);
                var buildingMasterId = (int)building.Attribute("MasterID");
                csv.AddHeader("BuildingMasterID", false, out int buildingMasterIdCol);

                foreach (var drawing in building.Elements("Drawing"))
                    foreach (var floor in drawing.Elements("Floor"))  //FloorMasterID is already added in room, so no need to extract it
                        foreach (var room in floor.Elements("Room"))
                        {
                            csv.AddRecord(r, estateMasterIdCol, EstateMasterId);
                            csv.AddRecord(r, buildingMasterIdCol, buildingMasterId);
                            genericHelper.AddAttributes(r, room, csv);
                            genericHelper.AddLayerDatas(r, room, csv);
                            r++;
                        }
            }
        }
    }
}
