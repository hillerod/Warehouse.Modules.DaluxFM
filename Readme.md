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