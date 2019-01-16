https://github.com/Azure/azure-functions-core-tools#installing

See the companion posting "[Linux Ubuntu 18.04 for Azure Developers](https://github.com/gloveboxes/Linux-Ubuntu-18.04-for-Azure-Developers)" which will explain how to set up your Linux based development desktop. Along with Linux you can build for Azure from Windows and MacOs.

## Background

This solution was built on Ubuntu 18.04 using Visual Studio Code, Visual Studio Code Extensions targeting .NET Core 2. The goal was to demonstrate how to build Azure Functions with .NET Core objects.

## Solution

The are three triggers

1. The Things Network Http Trigger to Event Hub Bridge to integrate Things Network Sensor data.
2. Device data streams and Azure Event Hub processing to maintain device state with Azure table Storage.
3. OpenWeather Map Timer trigger to gather calibration data from Open Weather Map
4. Simple Node.js and charting with http://c3js.org/ client side reporting. See https://github.com/gloveboxes/Environment-Monitor-IoT-Dashboard project. 
