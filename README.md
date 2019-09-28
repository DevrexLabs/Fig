* Work in progress! Not production ready, no nuget packages available yet..

## Fig

Hey dotnet devs, here's a nice little library that helps you
access configuration data in a structured and strongly typed manner.

Do you have code like the following spread around your code base?

```csharp
   var settingString = ConfigurationManager.AppSettings["CoffeeRefillIntervalInMinutes"] ?? "100";
   if (settingString == null) settingString = "20";
   var coffeeInterval = TimeSpan.FromMinutes(Int32.Parse(setting));
```

Fig can help you keep this nice and clean:

```csharp
  var settings = new SettingsBuilder()
    .UseAppSettingsXml()
    .Build<Settings>();
    
  var key = "CoffeeRefillInterval";
  var refillInterval = settings.Get<TimeSpan>(key, default: "00:10:00");  

  //if key is missing and no default is present, a KeyNotFoundException is thrown
  refillInterval = settings.Get<TimeSpan>(key);
```

If you prefer a strongly typed class there are 2 options: either bind to a POCO
or inherit from Fig.Settings class. Here is the inheritance approach:

```csharp
   public class CoffeeShopSettings : Settings
   {
      //with default
      public TimeSpan CoffeeRefillInterval 
        => Get<TimeSpan>(@default: () => TimeSpan.FromMinutes(10)); 
      
      //no default, but writeable!
      public bool EspressoMachineIsEnabled
      {
         get => Get<bool>();
         set => Set<bool>(value);
      }
   }

   //Now pass the type to the Build method
   var settings = new SettingsBuilder()
    .AppSettingsXmlProvider()
    .Build<CoffeeShopSettings>();
   
   Console.WriteLine("Next coffee due in " + settings.CoffeeRefillInterval);
```

To use a POCO you would call `Settings.Bind<T>()` or `Settings.Bind<T>(T poco)`


That's pretty useful as it is but wait, there's more! You can modify properties
at runtime and respond to changes by subscribing to the `PropertyChanged` event.

```csharp

   settings.PropertyChanged += (sender,args) 
        => Console.WriteLine(args.PropertyName + " changed!");

   settings.EspressoMachineIsEnabled = false;
```

the config looks like this:

```xml
   <appSettings>
      <add key="CoffeeShopSettings.espressomachineenabled" value="true"/>
      <add key="CoffeeShopSettings.CoffeeRefillInterval" value="00:42:00"/>
   </appSettings>
```

For types that derive from Settings, the class name is used as prefix for the key.
This is the default and can be overridden. For untyped access using the Settings class,
the default is no prefix which can also be overriden.

But what about appsettings.json for .NET Core? No worries, we've
got you covered:

```csharp
   var settings = new SettingsBuilder()
    .UseAppSettingsJson("appsettings.json")
    .Build<Settings>();
```
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

## Features
* Strongly typed and automatic type conversion
* Default values
* Validation - throw if key is missing
* Update runtime values (just add a setter)
* INotifyPropertyChanged support
* Namespaces hierarchical keys with dot-notation db.writer.timerinterval=42
* Case-insensitive keys
* Provider for App.Config / Web.config
* Provider for appsettings.json
* Provider for INI-files
* Provider for Environment variables (incomplete)
* Provider for command line args (incomplete)
* Named configuration support (Debug, Release, etc) with live swap (incomplete)
* Builder pattern to for flexible configuration (incomplete)
* Combine providers into layers
* ConnectionStrings from xml config included

## Contributing
Contributions are welcome! Check out existing issues, or create a new one,
and then we discuss details in the comments.

## Features to do or consider, in no particular order
* Write more tests!
* Write documentation (just use markdown in the docs folder)
* Encryption - encrypt with a tool, decrypt with a key provided at runime (env var, constructor arg)
* provider bootstrapping, choose at startup using heuristics
* add more tests
* custom converters - if we have a custom type not recognized by the TypeConverter
* Custom validators - say we have a string list of IP adresses, we might want to parse them or even ensure they are resolvable
* array values, GetArray() or array type on the property
* Diagnostics: which provider did a value come from?
* Dump values (INI, text)
* TOML 
* YAML, hell no :)