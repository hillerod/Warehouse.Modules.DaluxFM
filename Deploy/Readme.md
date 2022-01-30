# Use ARM templates to setup and maintain this module

## Prerequisites

If you don't have an Azure subscription, create a free [account](https://azure.microsoft.com/free/?ref=microsoft.com&utm_source=microsoft.com&utm_medium=docs&utm_campaign=visualstudio) before you begin.

Install the [Warehouse environment](https://github.com/Bygdrift/Warehouse/tree/master/Deploy), before installing any module.

## Videos

Short video on how to setup a Bygdrift Warehouse and install the DaluxFM Module without deeper explanations (it's in English):
<div align="left">
      <a href="https://www.youtube.com/watch?v=ahREssLMLG0">
         <img src="https://img.youtube.com/vi/ahREssLMLG0/0.jpg">
      </a>
</div>

How to setup the basic Bygdrift Warehouse (in Danish):
<div align="left">
      <a href="https://www.youtube.com/watch?v=6aR39glybhg">
         <img src="https://img.youtube.com/vi/6aR39glybhg/0.jpg">
      </a>
</div>

How to setup the Example module (in Danish). Gives a good idea of the technique:
<div align="left">
      <a href="https://www.youtube.com/watch?v=itwd2XdHIkM">
         <img src="https://img.youtube.com/vi/itwd2XdHIkM/0.jpg">
      </a>
</div>

How to setup the DaluxFM module (in Danish):
<div align="left">
      <a href="https://www.youtube.com/watch?v=xKkY_nAtV4c">
         <img src="https://img.youtube.com/vi/xKkY_nAtV4c/0.jpg">
      </a>
</div>

How to update an already installed module, once a new update has been pushed to GitHub (in Danish):
<div align="left">
      <a href="https://www.youtube.com/watch?v=XywfV_n-320">
         <img src="https://img.youtube.com/vi/XywfV_n-320/0.jpg">
      </a>
</div>

## Setup the Warehouse environment with the portal:

[![Deploy To Azure](https://raw.githubusercontent.com/Bygdrift/Warehouse/master/Docs/Images/deploytoazureButton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fhillerod%2FWarehouse.Modules.DaluxFM%2Fmaster%2FDeploy%2FWarehouse.Modules.DaluxFM_ARM.json)
[![Visualize](https://raw.githubusercontent.com/Bygdrift/Warehouse/master/Docs/Images/visualizebutton.svg)](http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2Fhillerod%2FWarehouse.Modules.DaluxFM%2Fmaster%2FDeploy%2FWarehouse.Modules.DaluxFM_ARM.json)

This will setup a Windows hosting plan and a function app that contains the software from this GitHub repository.

If you have to change some settings, you can run the setup again, and it should not affect data, but better take backup to be sure.

## Setup the environment with Azure CLI

You can also run the ARM from PowerShell.

Either run the PowerShell from computer by installing [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli), or use the [Azure Cloud Shell](https://shell.azure.com/bash) from the Azure portal. This instruction will focus on the run from a computer.

Download this [warehouse_ARM.parameters.json](https://raw.githubusercontent.com/hillerod/Warehouse.Modules.DaluxFM/master/Deploy/Warehouse.Modules.DaluxFM_ARM.parameters.json) to a folder and carefully fill in each variable.

Download [warehouse_ARM.json](https://raw.githubusercontent.com/hillerod/Warehouse.Modules.DaluxFM/master/Deploy/Warehouse.Modules.DaluxFM_ARM.json) to the same folder.

Login to azure: `az login`.

And then: `az deployment group create -g <resourceGroupName> --template-file ./Warehouse.Modules.DaluxFM_ARM.json --parameters ./Warehouse.Modules.DaluxFM_ARM.parameters.json`

Replace `<resourceGroupName>` with actual name.

I personally prefer to use CLI so I can collect all parameters in one json.