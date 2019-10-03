

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
    

  //get a value by key and convert to your target type
  //a KeyNotFoundException is thrown if key is missing

  var key = "CoffeeRefillInterval";
  var refillInterval = settings.Get<TimeSpan>(key);


  // if key is optional, provide a default
  // notice how the return type is derived from the default expression
  var refillInterval = settings.Get(key, () => TimeSpan.FromMinutes(10));   

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

If you prefer POCO setting classes, call `Settings.Bind<T>()` or `Settings.Bind<T>(T poco)`
(not yet implemented)

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

## Features
* Strongly typed and automatic type conversion
* Default values
* Validation - throw if key is missing
* Update runtime values (just add a setter)
* INotifyPropertyChanged support
* Case-insensitive keys
* Provider for App.Config / Web.config
* Provider for appsettings.json
* Provider for INI-files
* Provider for Environment variables
* Provider for command line args
* Named configuration support (Debug, Release, etc) with live swap (incomplete)
* Builder pattern to for flexible configuration
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
