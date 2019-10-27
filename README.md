
[![Build status](https://ci.appveyor.com/api/projects/status/cp39he84h5ar1edk?svg=true)](https://ci.appveyor.com/project/rofr/fig) [![Join the chat at https://gitter.im/DevrexLabs/Fig](https://badges.gitter.im/DevrexLabs/Fig.svg)](https://gitter.im/DevrexLabs/Fig?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
## Fig

A .NET Standard 2.0 library to help you load application configuration settings from multiple sources and access it
in a structured, type safe manner.

Do you have code like the following spread around your code base?

```c#
   var settingString = ConfigurationManager.AppSettings["CoffeeRefillIntervalInMinutes"];
   if (settingString == null) settingString = "20";
   var coffeeInterval = TimeSpan.FromMinutes(Int32.Parse(setting));
```

Fig can help you keep this nice and clean:

```c#
  var settings = new SettingsBuilder()
    .UseAppSettingsXml()
    .Build();

  var key = "CoffeeRefillInterval";
  var refillInterval = settings.Get<TimeSpan>(key);

  // provide a default for optional settings
  var refillInterval = settings.Get(key, () => TimeSpan.FromMinutes(10));
```
## Using custom types

If you prefer a typed class there are 2 options:
* inherit from `Fig.Settings`
* Bind to a POCO (work in progress)

Here is the first approach:

```c#
   public class CoffeeShopSettings : Settings
   {
      //with default
      public TimeSpan CoffeeRefillInterval => Get(() => TimeSpan.FromMinutes(10));

      //Read and write
      public bool EspressoMachineIsEnabled
      {
         get => Get<bool>();
         set => Set<bool>(value);
      }
   }

   //Use the SettingsBuilder to build an instance
   var settings = new SettingsBuilder()
    .UseAppSettingsXml()
    .Build<CoffeeShopSettings>();

   Console.WriteLine("Next coffee due in " + settings.CoffeeRefillInterval);
```

## Binding to POCOs
If you prefer POCO setting classes, call `Settings.Bind<T>()` or `Settings.Bind<T>(T poco)`
(work in progress)

```c#
   public class CoffeeShopSettings
   {
      //with default
      public TimeSpan CoffeeRefillInterval { get; set; } = TimeSpan.FromMinutes(10);

      //Read and write
      public bool EspressoMachineIsEnabled { get; set; }
   }

   //Use the SettingsBuilder to build an instance
   var settings = new SettingsBuilder()
      .UseAppSettingsXml()
      .Build()
      .Bind<CoffeeShopSettings>();

   Console.WriteLine("Next coffee due in " + settings.CoffeeRefillInterval);
```

## Binding to multiple objects
In a larger project you don't want all the settings in the same class.
In this case save a reference to the `SettingsBuilder` and use it multiple times:

```c#
var builder = new SettingsBuilder().UseAppSettingsXml();
var dbSettings = builder.Build<DbSettings>();
var networkSettings = builder.Build<NetworkSettings>();
```

## Change notifications
The `Settings` class implements `INotifyPropertyChanged`. React to configuration changes
by subscribing to the `PropertyChanged` event.

```c#
   settings.PropertyChanged += (sender,args) 
        => Console.WriteLine(args.PropertyName + " changed!");

   settings.EspressoMachineIsEnabled = false;
```

## Configuration sources
* web.config / app.config
* appSettings.json
* ini-files
* environment variables
* command line / string array
* Bring your own by implementing `ISettingsSource`

Sources provide key value pairs `(string,string)`. 
Each source is described below with an example.

## Web.config / App.config
Given this xml configuration:
```xml
   <appSettings>
      <add key="CoffeeShopSettings.espressomachineenabled" value="true"/>
      <add key="CoffeeShopSettings.CoffeeRefillInterval" value="00:42:00"/>
   </appSettings>
   <connectionStrings>
    <add name="mydb"
      connectionString="Data Source=.; Initial Catalog=mydb;Integrated Security=true"
      providerName="System.Data.SqlClient"/>
   </connectionStrings>
```
the default behavior will yield these keys:
```
CoffeeShopSettings.espressomachineenabled
CoffeeShopSettings.CoffeeRefillInterval
ConnectionStrings.mydb.connectionString
ConnectionStrings.mydb.providerName
```

## appSettings.json
This content:
```json
{
  "EspressoMachineEnabled" : true,
  "CoffeeRefillInterval" : "00:42:00",
  "Timeout" :  42,
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=app.db"
  },
  "Servers" :  ["10.0.0.1", "10.0.0.2"],
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
will be flattened to the following keys:
```
EspressoMachineEnabled
CoffeeRefillInterval
Timeout
ConnectionStrings.DefaultConnection
Servers.0
Servers.1
Logging.LogLevel.Default
AllowedHosts
```
## Ini files
This input:
```
keya=value
keya.keyb=value
[Network]
ip=10.0.0.3
[Datasource.A]
name=value
connectionstring=value
```
yields these keys:
```
keya
keya.keyb
Network.ip
Datasource.A.name
Datasource.A.connectionstring
```

# Combining sources
Use the `SettingsBuilder` to add sources in order of precedence. 
Settings in above layers override settings with the same key in
lower layers.

```c#
   var settings = new SettingsBuilder()
    .UseCommandLine(args)
    .UseEnvironmentVariables()
    .UseAppSettingsJson("appSettings.${ENV}.json, optional:true)
    .UseAppSettingsJson("appSettings.json")
    .Build<Settings>();
```
Notice the environment specific json file. The variable `${ENV}`
will be looked up based on what has been added so far:
environment variables and command line.

## Dealing with multiple environments
Normally you use different settings in different environments such as test, production, staging, dev, etc
In the previous section we used variable expansion to include an environment specific json file.

A different approach is to qualify keys with an environment name suffix:

```
   redis.endpoint=localhost:6379
   redis.endpoint:PROD=redis.mydomain.net:6379
   redis.endpoint:STAGING=10.0.0.42:6379
   ENV=PROD
```

You can also qualify an  entire section of an ini or json file:

```json
  {
     "Network:PROD" : {
        "ip" : "10.0.0.1",
	"port" : 3001
     }

     "Network:TEST" : {
        "ip" : "127.0.0.1",
	"port" : 13001
     }
  }
```
or

```ini
[Network:PROD]
ip=10.0.0.1
port=3001

[Network:TEST]
ip=127.0.0.1
port =13001
```

set the environment on the builder:
```c#
   var settings = new SettingsBuilder()
       .UseCommandLine()
       .UseEnvironmentVariables()
       .SetEnvironment("${ENV}") //must be provided above
       .UseIniFile("settings.ini")
       .Build();             
```
Or directly on the settings instance:
```c#
   _settings.SetEnvironment("STAGING");
   endpoint = _settings.Get("redis.endpoint"); // 10.0.0.42:6379
```

## Install from nuget
```bash
#Everything in a single bundle
Install-Package Fig.All

# the core package with support for command line, json, ini and environment variables
Install-Package Fig

# web.config and app.config support is in a separate package
Install-Package Fig.AppSettingsXml

#AppSettings json support
Install-Package Fig.AppSettingsJson

```
## Creating custom sources
* Inherit from `SettingsSource` and override `GetSettings()` 
* Create an extension method for the `SettingsBuilder`

```c#
public class MySource : SettingsSource
{
   protected override IEnumerable<(string, string)> GetSettings()
   
       yield return ("key", "value");
   }
}

public static class MySettingsBuilderExtensions
{
   public static SettingsBuilder UseMySource(this SettingsBuilder builder)
   {
      var mySource = new MySource();
      var dictionary = mySource.ToSettingsDictionary();
      return builder.Use(dictionary);
   }
}
``` 
## Contributing
Contributions are welcome! Check out existing issues, or create a new one,
and then we discuss details in the comments.
