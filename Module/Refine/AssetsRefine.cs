using Bygdrift.Warehouse.Modules;
using Bygdrift.Warehouse.DataLake.CsvTools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;

namespace Module.Refine
{
    public class AssetsRefine : RefineBase
    {
        private readonly RefineBase estates;
        private readonly RefineBase buildings;
        private readonly string uniqueHeaders;

        public XDocument Data { get; set; }
        public AssetsRefine(ImportBase importer, Stream xmlStream, RefineBase estates, RefineBase buildings, string uniqueHeaders) : base(importer, "Assets")
        {
            xmlStream.Position = 0;
            Data = XDocument.Load(xmlStream);
            this.estates = estates;
            this.buildings = buildings;
            this.uniqueHeaders = uniqueHeaders;
            CreateCsv();
            ImportRawFileToDataLake(DateTime.UtcNow, "xml", xmlStream);
            ImportCsvFileToDataLake(DateTime.UtcNow);
        }

        public  void CreateCsv()
        {
            var daluxGenericWash = new GenericHelper();

            var r = 0;
            foreach (var element in Data.Root.Elements())
            {
                daluxGenericWash.AddAttributes(r, element, CsvSet);
                daluxGenericWash.AddLayerDatas(r, element, CsvSet);
                r++;
            }
            ConvertDateTime();
            AddBuildingReferenceColumn();
            VerifyUniqueColumns();
        }

        /// <summary>
        /// DateTime is wrong
        /// </summary>
        private void ConvertDateTime()
        {
            var createdCol = CsvSet.GetRecordCol("CreatedDate");
            foreach (var item in createdCol.Records)
            {
                var val = DateTime.Parse(item.Value.ToString());
                CsvSet.UpdateRecord(createdCol.Col, item.Key, val);
            }

            var modifiedCol = CsvSet.GetRecordCol("LastModifiedDate");
            foreach (var item in modifiedCol.Records)
            {
                var val = DateTime.Parse(item.Value.ToString());
                CsvSet.UpdateRecord(modifiedCol.Col, item.Key, val);
            }
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
        public void AddBuildingReferenceColumn()
        {
            CsvSet.AddHeader("BuildingMasterId", out int assetCol_EstateBuildingId);

            if (
                !CsvSet.TryGetRecordCol("Estate", out int assetCol_Estate) || !CsvSet.TryGetRecordCol("BuildingName", out int assetCol_BuildingName) ||
                !buildings.CsvSet.TryGetRecordCol("EstateMasterID", out int buildingCol_EstateMasterID) || !buildings.CsvSet.TryGetRecordCol("Name", out int buildingCol_BuildingNumber) ||
                !buildings.CsvSet.TryGetRecordCol("MasterID", out int buildingCol_BuildingMasterID)
            )
                throw new System.Exception("Error must be handled by programmer.");


            var estateNameCol = estates.CsvSet.GetRecordCol("Name");
            var estateMasterIdCol = estates.CsvSet.GetRecordCol("MasterID");
            var buildingEstateMasterIdCol = buildings.CsvSet.GetRecordCol(buildingCol_EstateMasterID);

            var distinct = estateNameCol.Records.Select(o => o.Value).Distinct();
            var duplicates = estateNameCol.Records.Count() - distinct.Count();
            if (duplicates > 0)
            {
                estates.Errors.Add($"The column with the header: Name, must only contain unique names, but it has {duplicates} duplicates.");
                return;
            }

            for (int r = CsvSet.RowLimit.Min; r < CsvSet.RowLimit.Max; r++)
            {
                if (GetLookupValue(CsvSet, assetCol_Estate, r, estateNameCol.Records, estateMasterIdCol.Records, out object estateMasterID))
                {
                    CsvSet.Records.TryGetValue((assetCol_BuildingName, r), out object buildingNumber);
                    var (success, buildingMasterId) = GetBuildingMasterId(buildings.CsvSet, buildingEstateMasterIdCol, estateMasterID, buildingNumber, buildingCol_BuildingNumber, buildingCol_BuildingMasterID);
                    if (success)
                        CsvSet.AddRecord(assetCol_EstateBuildingId, r, buildingMasterId);
                }
            }
        }

        /// <summary>
        /// If buildingNumber is empty, then find the building with lowest building masterId
        /// If buildingNumber is set, then return current building masterId
        /// </summary>
        private (bool success, object buildingMasterId) GetBuildingMasterId(CsvSet buildings, Dictionary<int, object> buildingEstateMasterIds, object estateMasterID, object buildingNumber, int buildingCol_BuildingNumber, int buildingCol_BuildingMasterID)
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
                    buildings.Records.TryGetValue((buildingCol_BuildingNumber, row.Key), out object foundBuildingNumber);
                    if (foundBuildingNumber.Equals(buildingNumber))
                    {
                        buildings.Records.TryGetValue((buildingCol_BuildingMasterID, row.Key), out buildingMasterId);
                        break;
                    }
                    if (int.TryParse(foundBuildingNumber.ToString(), out int foundBuildingNumberAsInt) && foundBuildingNumberAsInt < lowestBuildingNumber)
                    {
                        lowestBuildingNumber = foundBuildingNumberAsInt;
                        lowestBuildingNumber_Row = row.Key;
                    }
                }

                if (buildingMasterId == null && lowestBuildingNumber_Row != int.MaxValue)
                    buildings.Records.TryGetValue((buildingCol_BuildingMasterID, lowestBuildingNumber_Row), out buildingMasterId);

            }
            return (success, buildingMasterId);
        }

        private static bool GetLookupValue(CsvSet primary, int primaryCol, int primaryRow, Dictionary<int, object> lookupArray, Dictionary<int, object> lookupResultArray, out object res)
        {
            if (primary.Records.TryGetValue((primaryCol, primaryRow), out object primaryRes) && primary != null)
            {
                var lookupResult = lookupArray.FirstOrDefault(o => o.Value.Equals(primaryRes));
                if (lookupResult.Value != null && lookupResultArray.TryGetValue(lookupResult.Key, out res))
                    return true;

                //var lookupResultRow = lookupArray[primaryRes];
                //if (lookupResultArray.TryGetValue(lookupResultRow, out res))
                //    return true;
            }
            res = null;
            return false;
        }

        private void VerifyUniqueColumns()
        {
            if (string.IsNullOrEmpty(uniqueHeaders))
                return;

            var headers = uniqueHeaders.Replace(" ", string.Empty).Split(',');

            foreach (var header in headers)
            {
                var colValues = CsvSet.GetRecordCol(header, false).Records?.Select(o => o.Value);
                var distinct = colValues.Distinct();
                var duplicates = colValues.Count() - distinct.Count();
                if (duplicates > 0)
                    AddError($"The column with the header: {header} must only contain unique content after filtering, but it has {duplicates} duplicates.");
            }
        }
    }
}
