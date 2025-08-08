# API Plugin

This is a plugin for the [Broadcast Application](http://google.com). 

This plugin will provide a REST API accessing the internal cache of the Broadcast application, making it possible to read and write data.

Additionally, the API will provide some specific calls to the windows services, allowing the calling application to carry out tasks like starting the Microsoft Flight Simulator Application.

## Build
The application is based upon the [Broadcast Plugin SDK] using C# .NET 8. 

To build:

```
dotnet build -t release
```

To Publish:

```
dotnet publish -t release
```

