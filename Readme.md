# Warehouse module: DaluxFM

With this module, you can push data from [DaluxFM](https://www.dalux.com/da/fm-overview/) into your own data warehouse on Azure.

The module is build with [Bygdrift Warehouse](https://github.com/Bygdrift/Warehouse), that makes it possible to attach multiple modules within the same azure environment, that can collect data from all kinds of services, in a cheap data lake.
The data data lake, is structured as a Common Data Model (CMD), which enables an easy integration to Microsoft Power BI, through Power BI Dataflows.

The module uses DaluxFM's SOAP web service. Soon this module will be transformed to use DaluxFM OpenAPI 

# Install procedure

## Prerequisites

If you don't have an Azure subscription, create a free [account](https://azure.microsoft.com/free/?ref=microsoft.com&utm_source=microsoft.com&utm_medium=docs&utm_campaign=visualstudio) before you begin.

Azure CLI: The command-line examples in this article use the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/) and are formatted for PowerShell. You can install the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) locally, or use the [Azure Cloud Shell](https://shell.azure.com/bash).

Before you run this script, run this command to connect to Azure:

```powershell
az login
```

## Setup the environment:

If you have not already set up the fundamental Azure environment, then go through these steps, before you can install the module:

```powershell
### Variables to be set before run:
$group = "warehouse" # The name on the resource group.
$storageAccount = "warehousesstorage" # The name on the storage account. Must be rather globally unique, so the sugested name 'warehousesstorage' will fail. Names must contain 3 to 24 characters numbers and lowercase letters only.

if((az storage account check-name --name $storageAccount --query nameAvailable) -eq "false"){
	Write-Host "The storage account name: '"$storageAccount"' already exists."  -ForegroundColor Red
	exit
}
az group create -g $group -l westeurope
az storage account create -n $storageaccount -g $group -l westeurope --sku Standard_GRS --kind StorageV2 --enable-hierarchical-namespace true
az config set extension.use_dynamic_install=yes_without_prompt
az monitor app-insights component create --app appinsights -g $group -l westeurope --kind web --application-type web --retention-time 30
```

## Add this module to the environment

Now you can install the module.
You will have to fill out the variables first. You can always edit them after the function app has been installed in the [Function App configuration](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=portal).

```powershell
### Variables to be set before run:
$group = "warehouse" # The same as used when setting up the environment
$storageAccount = "warehousesstorage" # The same as used when setting up the environment
$moduleName = "moduleDaluxFM" # The name on module (the function app)

### Variables to be set before run (app settings):
$DataLakeBasePath = "warehouse" # The base name in the data lake, where data from all the modules are stored.
$ScheduleExpression = "0 0 1 * * *" # How often this module should run. This example runs each night at 1AM UTC. Read more here: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=csharp#ncrontab-expressions
$DaluxFMCustomerId = "" # The Id that for the current user. I Hiller�d Kommune it is hillerod
$DaluxFMApiKey = "" # Contact Dalux, to get a key to: https://fm-aws-api.dalux.com/sharedservices/externaldataaccessservice.asmx
$DaluxFMUser = "" # A user that you can create in DaluxFM that has access to buildings and assets
$DaluxFMPassword = "" # the users password
$DaluxFMUniqueAssetColumns = "" # A comma seperated list of headers in assets, that should be checked for to se if they contain redundant data and if so, it will raise an error. Can be left blank. 

Write-Host "Add function app:"  -ForegroundColor Green

az functionapp create -n $moduleName -g $group --consumption-plan-location westeurope --storage-account $storageAccount --app-insights appInsights --functions-version 3 --runtime dotnet-isolated --runtime-version 5.0 --deployment-source-url https://github.com/hillerod/Warehouse.Modules.DaluxFM

Write-Host "Fill out the appConfig:"  -ForegroundColor Green
$DataLakeServiceUrl = az storage account show -n $storageAccount -g $group --query "primaryEndpoints.blob"
$DataLakeAccountKey = az storage account keys list -n $storageAccount -g $group --query "[0].value"

az functionapp config appsettings set -n $moduleName -g $group --settings `
DataLakeAccountName=$group"storage" `
DataLakeAccountKey=$DataLakeAccountKey `
DataLakeServiceUrl=$DataLakeServiceUrl `
DataLakeBasePath=$DataLakeBasePath `
ScheduleExpression=$ScheduleExpression `
DaluxFMCustomerId=$DaluxFMCustomerId `
DaluxFMApiKey=$DaluxFMApiKey `
DaluxFMUser=$DaluxFMUser `
DaluxFMPassword=$DaluxFMPassword `
DaluxFMUniqueAssetColumns=$DaluxFMUniqueAssetColumns
```

The module will run immediately after it has been installed. The process with fetching and storing data, takes around 3 minutes for 'Hillerød Kommunes' portfolio.
You should should be able to se the data in the storage account after the run is finished.

## Update this modules installation

Each time a new version of this module gets released on GitHub, the module will automatic update on Azure (called ci/cd).
If you want to turn this off, then replace 'az functionapp create...' with: `az functionapp create -n $moduleName -g $group --consumption-plan-location westeurope --storage-account $storageAccount --app-insights appInsights --functions-version 3 --runtime dotnet-isolated --runtime-version 5.0`. 
And add this line: `az functionapp deployment source config -n $moduleName -g $group --branch master --manual-integration --repo-url https://github.com/hillerod/Warehouse.Modules.DaluxFM`.
Then run this command, each time you want to update the module: `az functionapp deployment source sync -n $moduleName -g $group`.

## Trace problems

If you want more detailed informations when triggering each cli, then include `--debug`.
There are even better feedback when monitoring your function apps in Visual Studio Code with the Azure extension, than there is when only using the Azure web portal and the Azure CLI. And from VS Code, it is easy to manage app settings, watching deployment logs and to trigger the function event.

## Remove this module from the environment

```powershell
$group = "warehouse" # The name on the resource group (the same as used when setting up the environment)
az functionapp delete -n moduleDaluxFM -g $group
```

## Remove the whole environment

The whole setup can be removed again by deleting the resource group:

```powershell
$group = "warehouse" # The name on the resource group (the same as used when setting up the environment)
az group delete -g $group
```

# Get access from Power BI

To create a dataflow from Azure Data Lak0 [ADLS], the ADLS account must be in the same tenant as PowerBI.

The user that sets up the data flow, has to have access to ADLS.
- In Azure web portal, go into the storage account
- Select 'Access Control (IAM)'
- Select Add, and then 'Add role assignment'
- For a role, select 'Storage Blob Data Reader', and add the user

Now the user can log into [Power BI](https://app.powerbi.com/) and:
- Select or create a workspace
- Select 'New' and then 'Dataflow'
- Select 'Attach a Common Data Model folder (preview)'
- Give it a name and attach the path to the model.json in `https://<$storageAccount>.dfs.core.windows.net/<$DataLakeBasePath>/daluxfm/current/model.json`