# Warehouse module: DaluxFM

This module, fetches data from [DaluxFM](https://www.dalux.com/da/fm-overview/) each day, and stores it into your own data warehouse on Azure.

The module is build with [Bygdrift Warehouse](https://github.com/Bygdrift/Warehouse), that enables one to attach multiple modules within the same azure environment, that can collects and wash data from all kinds of services, in a cheap data lake and database.
By saving data to a MS SQL database, it is:
- easy to fetch data with Power BI, Excel and other systems
- easy to control who has access to what - actually, it can be controlled with AD so you don't have to handle credentials
- It's cheap

The module uses DaluxFM's SOAP web service that right now, is the best approach to fetch data from DaluxFM. There is another module here, that uses [DaluxFM API](https://github.com/Bygdrift/Warehouse.Modules.DaluxFMApi).

## Installation

All modules can be installed and facilitated with ARM templates (Azure Resource Management): [Use ARM templates to setup and maintain this module](https://github.com/hillerod/Warehouse.Modules.DaluxFM/tree/master/Deploy).

## License

[MIT License](https://github.com/Bygdrift/Warehouse.Modules.Example/blob/master/License.md)

## Database content

In DaluxFM, you can create your own fields and they will become available in the database with appropriate data type. There are also static fields from Dalux, and they are:

| TABLE_NAME | COLUMN_NAME                          | DATA_TYPE |
| :--------- | :----------------------------------- | :-------- |
| Assets     | Classification                       | varchar   |
| Assets     | ClassificationCode                   | varchar   |
| Assets     | ClassificationName                   | varchar   |
| Assets     | AssetName                            | varchar   |
| Assets     | DatabaseID                           | int       |
| Assets     | IsSubComponent                       | varchar   |
| Assets     | SubComponentOf                       | varchar   |
| Assets     | SubComponentOfMasterID               | int       |
| Assets     | Photos                               | varchar   |
| Assets     | Description                          | varchar   |
| Assets     | AdviserDisciplines                   | varchar   |
| Assets     | CreatedBy                            | varchar   |
| Assets     | CreatedDate                          | datetime  |
| Assets     | LastModifiedBy                       | varchar   |
| Assets     | LastModifiedDate                     | datetime  |
| Assets     | Location                             | varchar   |
| Assets     | Estate                               | varchar   |
| Assets     | Address                              | varchar   |
| Assets     | StreetNumber                         | varchar   |
| Assets     | ZipCode                              | int       |
| Assets     | City                                 | varchar   |
| Assets     | BuildingName                         | varchar   |
| Assets     | BbrBMunicipalityNumber               | int       |
| Assets     | BbrBEstateNumber                     | int       |
| Assets     | BbrBBuildingNumber                   | int       |
| Assets     | FloorName                            | varchar   |
| Assets     | RoomNumber                           | varchar   |
| Assets     | RoomName                             | varchar   |
| Assets     | RoomType                             | varchar   |
| Assets     | InstallDate                          | datetime  |
| Assets     | WarrantyStart                        | datetime  |
| Assets     | WarrantyEnd                          | datetime  |
| Assets     | WarrantyRest                         | int       |
| Assets     | AmountOfTasks                        | int       |
| Assets     | AmountOfReports                      | int       |
| Assets     | AmountOfTaskSeries                   | int       |
| Assets     | ProductName                          | varchar   |
| Assets     | Producer                             | varchar   |
| Assets     | ProductTypeName                      | varchar   |
| Assets     | ProductDescription                   | varchar   |
| Assets     | ProductClassification                | varchar   |
| Assets     | ProductNewPrice                      | varchar   |
| Assets     | ProductLifespan                      | int       |
| Assets     | ProductWarranty                      | int       |
| Assets     | ProductDiscipline                    | varchar   |
| Assets     | EstateMasterId                       | int       |
| Assets     | BuildingMasterId                     | int       |
| Buildings  | ID                                   | int       |
| Buildings  | MasterID                             | int       |
| Buildings  | EstateID                             | int       |
| Buildings  | EstateMasterID                       | int       |
| Buildings  | Name                                 | varchar   |
| Buildings  | Address                              | varchar   |
| Buildings  | AlternativeName                      | varchar   |
| Buildings  | Number                               | varchar   |
| Buildings  | City                                 | varchar   |
| Buildings  | ZipCode                              | int       |
| Buildings  | BasementGrossArea                    | int       |
| Buildings  | GrossArea                            | varchar   |
| Buildings  | NetArea                              | varchar   |
| Buildings  | LastModifiedDate                     | varchar   |
| Buildings  | LastDrawingChange                    | datetime  |
| Buildings  | DisplayName                          | varchar   |
| Buildings  | Drawings                             | int       |
| Buildings  | Drawing link                         | varchar   |
| Buildings  | Diagram link                         | varchar   |
| Buildings  | GISLat                               | float     |
| Buildings  | GISLon                               | float     |
| Buildings  | BbrBID                               | varchar   |
| Buildings  | KoorNord                             | int       |
| Buildings  | KoorOest                             | int       |
| Buildings  | BbrBMunicipalityNumber               | int       |
| Buildings  | BbrBMunicipalityName                 | varchar   |
| Buildings  | BbrBEstateNumber                     | int       |
| Buildings  | BbrBBuildingNumber                   | int       |
| Buildings  | BbrBStreetName                       | varchar   |
| Buildings  | BbrBHouseNumber                      | varchar   |
| Buildings  | BbrBZipCode                          | int       |
| Buildings  | BbrBCityName                         | varchar   |
| Buildings  | BbrBCountryCadastralDistrict         | varchar   |
| Buildings  | BbrBMunicipalityCadastralDistrict    | varchar   |
| Buildings  | BbrBCadastralKind                    | int       |
| Buildings  | BbrBCadastralNumber                  | varchar   |
| Buildings  | BbrBBuildingUtilization              | varchar   |
| Buildings  | BbrBFloorCount                       | int       |
| Buildings  | BbrBFloorDepart                      | varchar   |
| Buildings  | BbrBDataStatus                       | int       |
| Buildings  | BbrBCarportPrincip                   | int       |
| Buildings  | BbrBProtection                       | varchar   |
| Buildings  | BbrBTemporaryCreation                | varchar   |
| Buildings  | BbrBSecurityClassification           | int       |
| Buildings  | BbrBStormConsultNotice               | varchar   |
| Buildings  | BbrBAppartmentsWithKitchen           | int       |
| Buildings  | BbrBAppartmentsWithoutKitchen        | int       |
| Buildings  | BbrBConstructionYear                 | int       |
| Buildings  | BbrBReconstructionYear               | int       |
| Buildings  | BbrBSecurityRoomCount                | int       |
| Buildings  | BbrBTotalBasementArea                | int       |
| Buildings  | BbrBTotalBuildingArea                | int       |
| Buildings  | BbrBTotalHousingArea                 | int       |
| Buildings  | BbrBTotalProfessionArea              | int       |
| Buildings  | BbrBTotalBuiltArea                   | int       |
| Buildings  | BbrBRepportedShedArea                | int       |
| Buildings  | BbrBTotalRoofArea                    | int       |
| Buildings  | BbrBUtilizedRoofArea                 | int       |
| Buildings  | BbrBShedArea                         | int       |
| Buildings  | BbrBCoverArea                        | int       |
| Buildings  | BbrBOpenCoverArea                    | int       |
| Buildings  | BbrBAreaSource                       | varchar   |
| Buildings  | BbrBOuterWallMaterial                | varchar   |
| Buildings  | BbrBRoofMaterial                     | varchar   |
| Buildings  | BbrBMaterialSource                   | varchar   |
| Buildings  | BbrBBuildingDrain                    | varchar   |
| Buildings  | BbrBBuildingWaterSupply              | int       |
| Buildings  | BbrBBuildingHeating                  | varchar   |
| Buildings  | BbrBAdditionalHeartingSupply         | varchar   |
| Buildings  | BbrBSupplementaryHeatingInstallation | varchar   |
| Buildings  | BbrBCrudID                           | int       |
| Estates    | ID                                   | int       |
| Estates    | MasterID                             | int       |
| Estates    | LocationID                           | int       |
| Estates    | LocationMasterID                     | int       |
| Estates    | Name                                 | varchar   |
| Estates    | Description                          | varchar   |
| Estates    | DisplayName                          | varchar   |
| Estates    | GISLat                               | float     |
| Estates    | GISLon                               | float     |
| Estates    | Buildings                            | varchar   |
| Lots       | ID                                   | int       |
| Lots       | MasterID                             | int       |
| Lots       | EstateMasterID                       | int       |
| Lots       | Name                                 | varchar   |
| Lots       | Address                              | varchar   |
| Lots       | AlternativeName                      | varchar   |
| Lots       | Number                               | varchar   |
| Lots       | City                                 | varchar   |
| Lots       | ZipCode                              | int       |
| Lots       | GrossArea                            | int       |
| Lots       | DisplayName                          | varchar   |
| Lots       | GISLat                               | float     |
| Lots       | GISLon                               | float     |
| Rooms      | EstateMasterID                       | int       |
| Rooms      | BuildingMasterID                     | int       |
| Rooms      | ID                                   | int       |
| Rooms      | MasterID                             | int       |
| Rooms      | FloorMasterID                        | int       |
| Rooms      | NetArea                              | varchar   |
| Rooms      | EstimatedGrossArea                   | varchar   |
|            |                                      |           |


## Data lake content

In the data lake container with this modules name, there are three main folders `Drawings`, `Raw` and `Refined`.

`Drawings` contains all drawings that are downloaded by using the `Drawing link` and `Diagram link` from the database table `Buildings`.

The folder `Raw` contains the raw data loaded from Dalux web service and there are one folder for each day.

The folder `Refine` contains the refined data as csv - one folder for each day. The data is exactly the same as it is in the database.

 The folder structure:

+ Drawings
    - Buildings
        - {buildingMasterId}.drawio
        - {buildingMasterId}.pdf
+ Raw
    - {yyyy the year}
        - {MM the month}
            - {dd the month}
                - Assets.xml
                - Estates.xml
+ Refined
    - {yyyy the year}
        - {MM the month}
            - {dd the month}
                - Assets.csv
                - Buildings.csv
                - Estates.csv
                - Lots.csv
                - Rooms.csv

