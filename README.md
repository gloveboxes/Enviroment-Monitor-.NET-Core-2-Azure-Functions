https://github.com/Azure/azure-functions-core-tools#installing

See the companion posting "[Linux Ubuntu 18.04 for Azure Developers](https://github.com/gloveboxes/Linux-Ubuntu-18.04-for-Azure-Developers)" which will explain how to set up your Linux based development desktop. Along with Linux you can build for Azure from Windows and MacOs.

## Background

This solution was built on Ubuntu 18.04 using Visual Studio Code, Visual Studio Code Extensions targetting .NET Core 2. The goal was to demonstrate how to build Azure Functions with .NET Core objects.

## Solution

The are three triggers

1. The Things Network Http Trigger to Event Hub Bridge to integrate Things Networ Sensor data.
2. Device data streams and Azure Event Hub processing to maintain device state with Azure table Storage.
3. OpenWeather Map Timer trigger to gather calibration data from Open Weather Map
4. Simple Node.js and charting with http://c3js.org/ client side reporting. See https://github.com/gloveboxes/Environment-Monitor-IoT-Dashboard project. 


## Solution Dependencies

This solution has the following dependencies.

* https://www.nuget.org/packages/Microsoft.Azure.ServiceBus/
* https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.EventHubs
* https://www.nuget.org/packages/Microsoft.NET.Sdk.Functions/
* https://www.nuget.org/packages/Microsoft.Azure.Amqp/ 


## Tips and Tricks

.NET Core 2 Azure Functions is still in beta so you need to keep a close eye on the version numbers of assemblies and functionality is moving assemblies.

### Event Hub Triggers has moved assemblies

* Adding Azure Event Hub Trigger support 
    * https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.EventHubs
        * dotnet add package Microsoft.Azure.WebJobs.Extensions.EventHubs --version 3.0.0-beta5
* Manually add in the AMQP assembles as Eventhub nuget package did not pull in the depency automatically.
    * dotnet add package Microsoft.Azure.Amqp --version 2.3.0





