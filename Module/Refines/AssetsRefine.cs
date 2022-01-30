using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;
using Bygdrift.CsvTools;
using Bygdrift.Warehouse;
using System.Threading.Tasks;
using Bygdrift.DataLakeTools;
using Module.Refines.Helpers;

namespace Module.Refines
{
    public class AssetsRefine
    {
        private static readonly Csv csv = new();

        public static async Task<Csv> RefineAsync(AppBase<Settings> app, Stream xmlStream, Csv estatesCsv, Csv buildingsCsv)
        {
            app.Log.LogInformation("Loading Assets");
            if (xmlStream == null)
                return default;

            CreateCsv(app, xmlStream, estatesCsv, buildingsCsv);
            await app.DataLake.SaveCsvAsync(csv, "Refined", "Assets.csv", FolderStructure.DatePath);
            app.Mssql.MergeCsv(csv, "Assets", "DatabaseID", true, true);
            return csv;
        }

        private static void CreateCsv(AppBase app, Stream stream, Csv estatesCsv, Csv buildingsCsv)
        {
            stream.Position = 0;
            var data = XDocument.Load(stream);

            var daluxGenericWash = new GenericHelper();

            var r = 0;
            foreach (var element in data.Root.Elements())
            {
                daluxGenericWash.AddAttributes(r, element, csv);
                daluxGenericWash.AddLayerDatas(r, element, csv);
                r++;
            }
            AddBuildingReferenceColumn(app, estatesCsv, buildingsCsv);
        }

        /// <summary>
        /// Loops through all assets and tries to add a reference to a buildings masterId.
        /// If the asset, only have a reference to an estate, then this method will find the building with the lowest buildingnumber and refer to that building.
        /// In details, this method does the following on each asset:
        /// - Get the field 'Estate' on asset.
        /// - Find the value in Estates and returns the estates 'MasterId'. If there are more estates with same name, it will pick a random estate between these.
        /// - Get the field 'BuildingName' on asset.
        /// - If it is set, then:
        ///     - Get all buildings with the looked up EstateMasterId.
        ///     - Loop through these buildings to find the one with the given buildingNumber
        /// - If it's not set:
        ///     - Get all buildings with the looked up EstateMasterId.
        ///     - Loop through these buildings to find the one with the lowest buildingNumber
        /// - Get the building MasterId
        /// - Save the buildings MasterId on the current asset
        /// 
        /// What must be correct in the data in Dalux:
        /// - Estate names, has to be unique or else this method will take the first estate with that name. 
        /// - Building names, also has to be unique on each estate or else it will return the first ocourence 
        /// This is a weakness that could be fixed, if Dalux adds Estate and Building masterId to assets in there web service.
        /// </summary>
        private static void AddBuildingReferenceColumn(AppBase app, Csv estatesCsv, Csv buildingsCsv)
        {
            csv.AddHeader("EstateMasterId", out int assetCol_EstateEstateId);
            csv.AddHeader("BuildingMasterId", out int assetCol_EstateBuildingId);

            if (
                !csv.TryGetColId("Estate", out int assetCol_Estate) || !csv.TryGetColId("BuildingName", out int assetCol_BuildingName) ||
                !buildingsCsv.TryGetColId("EstateMasterID", out int buildingCol_EstateMasterID) || !buildingsCsv.TryGetColId("Name", out int buildingCol_BuildingNumber) ||
                !buildingsCsv.TryGetColId("MasterID", out int buildingCol_BuildingMasterID)
            )
                throw new Exception("Error must be handled by programmer.");


            var estateNameCol = estatesCsv.GetColRecords("Name");
            var estateMasterIdCol = estatesCsv.GetColRecords("MasterID");
            var buildingEstateMasterIdCol = buildingsCsv.GetColRecords(buildingCol_EstateMasterID);

            var distinct = estateNameCol.Select(o => o.Value).Distinct();
            var duplicates = estateNameCol.Count() - distinct.Count();
            if (duplicates > 0)
            {
                app.Log.LogError($"The column with the header: Name, must only contain unique names, but it has {duplicates} duplicates.");
                return;
            }

            for (int r = csv.RowLimit.Min; r < csv.RowLimit.Max; r++)
            {
                if (GetLookupValue(csv, assetCol_Estate, r, estateNameCol, estateMasterIdCol, out object estateMasterID))
                {
                    csv.Records.TryGetValue((r, assetCol_BuildingName), out object buildingNumber);
                    var (success, buildingMasterId) = GetBuildingMasterId(buildingsCsv, buildingEstateMasterIdCol, estateMasterID, buildingNumber, buildingCol_BuildingNumber, buildingCol_BuildingMasterID);
                    if (success)
                    {
                        csv.AddRecord(r, assetCol_EstateBuildingId, buildingMasterId);
                        csv.AddRecord(r, assetCol_EstateEstateId,  estateMasterID);
                    }
                }
            }
        }

        /// <summary>
        /// If buildingNumber is empty, then find the building with lowest building masterId
        /// If buildingNumber is set, then return current building masterId
        /// </summary>
        private static (bool success, object buildingMasterId) GetBuildingMasterId(Csv buildings, Dictionary<int, object> buildingEstateMasterIds, object estateMasterID, object buildingNumber, int buildingCol_BuildingNumber, int buildingCol_BuildingMasterID)
        {
            object buildingMasterId = null;
            var success = false;
            var buildingRows = buildingEstateMasterIds.Where(o => o.Value.Equals(estateMasterID));
            if (buildingRows.Any())
            {
                success = true;
                var lowestBuildingNumber = int.MaxValue;
                var lowestBuildingNumber_Row = int.MaxValue;
                foreach (var row in buildingRows)
                {
                    buildings.Records.TryGetValue((row.Key, buildingCol_BuildingNumber), out object foundBuildingNumber);
                    if (foundBuildingNumber.Equals(buildingNumber))
                    {
                        buildings.Records.TryGetValue((row.Key, buildingCol_BuildingMasterID), out buildingMasterId);
                        break;
                    }
                    if (int.TryParse(foundBuildingNumber.ToString(), out int foundBuildingNumberAsInt) && foundBuildingNumberAsInt < lowestBuildingNumber)
                    {
                        lowestBuildingNumber = foundBuildingNumberAsInt;
                        lowestBuildingNumber_Row = row.Key;
                    }
                }

                if (buildingMasterId == null && lowestBuildingNumber_Row != int.MaxValue)
                    buildings.Records.TryGetValue((lowestBuildingNumber_Row, buildingCol_BuildingMasterID), out buildingMasterId);

            }
            return (success, buildingMasterId);
        }

        private static bool GetLookupValue(Csv primary, int primaryCol, int primaryRow, Dictionary<int, object> lookupArray, Dictionary<int, object> lookupResultArray, out object res)
        {
            if (primary.Records.TryGetValue((primaryRow, primaryCol), out object primaryRes) && primary != null)
            {
                var lookupResult = lookupArray.FirstOrDefault(o => o.Value.Equals(primaryRes));
                if (lookupResult.Value != null && lookupResultArray.TryGetValue(lookupResult.Key, out res))
                    return true;
            }
            res = null;
            return false;
        }
    }
}
