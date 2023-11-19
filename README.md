
[![Build status](https://ci.appveyor.com/api/projects/status/cp39he84h5ar1edk?svg=true)](https://ci.appveyor.com/project/rofr/fig)
 
# Fig
A .NET Standard 2.0 library to help you read application configuration settings from multiple sources. Fig is similar to the Microsoft Configuration Extensions used with AspnetCore but adds some additional behavior yet still has fewer dependencies. 

This README is the documentation.

## Breaking changes with version 2.0!
Please read the [Release notes](https://github.com/DevrexLabs/Fig/releases/tag/2.0.24) carefully before upgrading from an older version.

## Documentation
Do you have code like the following sprinkled across your code base?

```c#
   var setting = ConfigurationManager.AppSettings["CoffeeRefillIntervalInMinutes"];
   if (setting == null) setting = "20";
   var refillInterval = TimeSpan.FromMinutes(Int32.Parse(setting));
```

There is a lot of stuff going on here:
 
* Using a string literal key
* Checking for null
* Setting a default
* Converting from string to int
* Converting to TimeSpan

Fig wraps these mundane boilerplate tasks into a simple library letting you either retrieve strongly typed settings by name or bind to the properties of a custom class. The example code above is also tightly coupling to a single configuration source, the web.config or app.config file. Fig supports multiple sources such as environment variables, json files, ini files, sql databases.


## Setting up the configuration sources
Load configuration data from multiple sources
using a fluent builder API. If there are duplicate keys the last one encountered takes precedence. Keys are case insensitive.

```c#
  var settings = new SettingsBuilder()
    .UseAppSettingsXml()
    .UseEnvironmentVariables()
    .Build();
```

Do this when your application starts up and then keep a reference to the settings object.

## Binding
Next, define a simpe class with read/write properties to represent your configurable settings. Now call `settings.Bind<T>()` and Fig will create an instance of T and assign properties with matching names.

```csharp
   public class CoffeeShopSettings
   {
      public TimeSpan RefillInterval { get; set; }
         = TimeSpan.FromMinutes(10);

      public bool EnableEspressoMachine { get; set; }
   }

   var settings = new SettingsBuilder()
      .UseAppSettingsXml()
      .Build();
   
   var coffeeShopSettings = settings.Bind<CoffeeShopSettings>();

   //It's also possible to bind to an existing object
   var shopSettings = new CoffeeShopSettings();
   settings.Bind(shopSettings);
```

By default, the class name will be used as a qualifier before the property name, so the preceding example will bind to
the following keys:
```
  CoffeeShopSettings.RefillInterval
  CoffeeShopSettings.EnableEspressoMachine
```

Change this behavior by passing an alternative path, not including the ".":

```csharp
  //CoffeeShop.RefillInterval
  settings.Bind<CoffeeShopSettings>(path: "CoffeeShop");
  //or just "RefillInterval"
  settings.Bind<CoffeeShopSettings>(path: "")
```

## Binding to multiple objects
In a larger project you probably don't want all the settings in the same class.
One solution is to create separate classes to hold subsets of the configuration data.

```csharp
var settings = new SettingsBuilder()
   .UseAppSettingsXml()
   .Build();

var dbSettings = settings.Bind<DbSettings>();
var networkSettings = settings.Bind<NetworkSettings>();
```

So the configuration keys might be:

```
DbSettings.ReadTimeout
DbSettings.ConnectionString
NetworkSettings.TcpPort
NetworkSettings.Retries
```
## Binding to arrays
Append indicies to your configuration keys to define an array:

```
Servers.0 = 10.0.0.1
Servers.1 = 10.0.0.2
```

and then bind to a property of type array:

```csharp
  class MySettings
  {
     public string[] Servers { get; set;}
  }
```
Arrays in json files will work this way. See the section below on appsettings.json.

## Variable substitution

## Binding Validation
Properties are either required or optional. To make a property optional, assign it a non-null value before binding.

Note that value types have non-null defaults. So to make them required, declare using `Nullable<T>`

Fig will validate by throwing an exception if any property on the target is null after binding.

You can disable validation by passing `validation: false` to the `Binding()` methods.

So given the following class:

```csharp
   public class CoffeeShopSettings
   {
      public TimeSpan? RefillInterval { get; set; }
      public bool? EnableEspressoMachine { get; set; }
      public string Greeting { get;set; } = "Coffee time!";
   }
```

the `RefillInterval` and `EnableEspressoMachine` parameters are required while the `Greeting` property is optional.


## Retrieving values by key

For simple applications with just a few parameters defining a custom class could be considered over-engineering. In this case you can retrieve values directly by key, either as strings or converted to a desired type.

```csharp
  //Or grab directly by key
  var key = "CoffeeRefillInterval";
  var refillInterval = settings.Get<TimeSpan>(key);

  // For optional settings, provide a default using either a lambda or a direct value
  // lambda can be useful to avoid an expensive calculation
  var refillInterval = settings.Get(key, () => TimeSpan.FromMinutes(10));

  //Direct default value
  var pricePerCup = settings.Get("PricePerCup", 24);

```
Calling `Get()` without a default will throw a KeyNotFoundException if the key is missing. Keys are not case sensitive.

## Configuration sources
* web.config / app.config
* appSettings.json
* ini-files
* environment variables
* command line / string array
* Sql database
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

## Environment variables
Given these environment variables:
```bash
export ENV=TEST
export FIG_TIMEOUT=30
export FIG_LOGGING_LOGLEVEL_DEFAULT=Warning
export MYAPP_COLOR=Red
export MYAPP_ENDPOINT=10.0.0.1:3001
```
`builder.UseEnvironmentVariables(prefix: "FIG_", dropPrefix:true)` yields:
```
TIMEOUT
LOGGING.LOGLEVEL.DEFAULT
```
`builder.UseEnvironmentVariables(prefix: "MYAPP_", dropPrefix:false)` yields:
```
MYAPP.COLOR
MYAPP.ENDPOINT
```

`builder.UseEnvironmentVariables()` yields:
```
ENV
FIG.TIMEOUT
FIG.LOGGING.LOGLEVEL.DEFAULT
MYAPP.COLOR
MYAPP.ENDPOINT
```

## Command line
Fig can take key/value pairs passed on the command line. The default prefix is "--fig:" and default separator is "="
```c#
//Given
string[] args = new []{
   "--fig:ENV=Test",
   "--fig:Timeout=30",
   "retries=3"};
```

`settingsBuilder.UseCommandLine(args)` yields:

```
ENV
Timeout
```

and `settingsBuilder.UseCommandLine(args, prefix: "")` yields:
```
retries
``` 

## Sql database
Version 1.9 introduces SqlSettings. Read keys and values from any sql database
that has an ADO.NET provider. (implements `IDbConnection`)

First, `Install-Package Fig.SqlSettings`, then use one of the `UseSql` extension method
overloads to setup a connection to your database. 

```csharp
  //pass an IDbConnection with a preconfigured connection string
  IDbConnection connection = new SqlConnection("Data source=.;Integrated Security=true;Database=mydb");
  var settings = new SettingsBuilder()
   .UseSql(connection)
   .Build();
```

```csharp
  //pass a type parameter for the `IDbConnection` implementation class
  //and a connection string key to pick up from AppSettingsXml / AppSettingsJson
  var settings = new SettingsBuilder()
   .UseAppSettingsJson("appsettings.json")
   .UseSql<SqlConnection>("ConnectionStrings.SQLiteConnection")
   .Build();
```
The SQL query used is `SELECT key, value FROM FigSettings`. You can pass your own query:

```csharp
  var settings = new SettingsBuilder()
   .UseAppSettingsJson("appsettings.json")
   .UseSql<SqlConnection>("ConnectionStrings.SQLiteConnection", "SELECT a,b FROM MySettings")
   .Build();
```

# Combining sources
Use the `SettingsBuilder` to add sources in order of precedence. 
Settings in above layers override settings with the same key in
lower layers.

```c#
   var settings = new SettingsBuilder()
    .UseEnvironmentVariable("ENV")
    .UseAppSettingsJson("appSettings.json")
    .UseAppSettingsJson("appSettings.${ENV}.json", required:false)
    .UseDotEnv()
    .UseEnvironmentVariables()
    .UseCommandLine(args)    
    .Build<Settings>();
```
Notice the variable substitution in the second json file. The variable `${ENV}`
will be looked up in the settings dictionary built so far.

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

## What do I have?
`Settings.ToString()` is your friend. It will return a plain-text formatted table
with keys and values of each layer:

```
-------------------- Layer 0 ----------------------
| Network.ip             | 127.0.0.1              |
| Network.port           | 13001                  |
-------------------- Layer 1 ----------------------
| Network.ip             | 10.0.0.1               |
| Network.port           | 3001                   |
---------------------------------------------------
```


## Creating custom sources
1. Create a class that inherits from `SettingsSource` and overrides `GetSettings()` 
2. Create an extension method for the `SettingsBuilder`

```c#
public class MySource : SettingsSource
{
   protected override IEnumerable<(string, string)> GetSettings()
   {
      //todo: your code goes here
       yield return ("key", "value");
   }
}

public static class MySettingsBuilderExtensions
{
   public static SettingsBuilder UseMySource(this SettingsBuilder builder)
   {
      //do what you have to do
      var mySource = new MySource();

      //call the inherited ToSettingsDictionary() method
      //which in turn iterates over your GetSettings() implementation
      var dictionary = mySource.ToSettingsDictionary();

      // Remember to return the builder to support fluent configuration
      return builder.UseSettingsDictionary(dictionary);
   }
}
``` 
## Contributing
Contributions are welcome! Check out existing issues, or create a new one,
and then we discuss details in the comments.
