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

Fig can help you keep this nice and clean. Create a class that derives
from `Fig.Config`, add properties with getters and optional setters:

```csharp
   public class CoffeeShopSettings : Config
   {
      [Config(Default = TimeSpan.FromMinutes(100))]
      public TimeSpan CoffeeRefillInterval
      {
        get => Get<TimeSpan>(); 
      }

      // No default so this is a required property
      // also it has a setter which means the current
      // runtime value can be changed. Changes will fire events
      // that can be subscribed to
      [Config(Name="EspressoMachineEnabled")]
      public bool EspressoMachineIsEnabled
      {
         get => Get<bool>();
         set => Set<bool>(value);
      }
   }

   var provider = new AppSettingsXmlProvider(prefix="fig:");
   var fig = new CoffeeShopSettings(provider);

   // Ensure required properties exist
   // and can be converted to their correct types
   // will throw a ConfigurationException otherwise
   fig.Validate();

   Console.WriteLine("Next coffee due in " + fig.CoffeeRefillInterval);
```

That's pretty useful as it is but wait, there's more! You can change properties
at runtime and subscribe to changes elsewhere in the application.

```csharp

   fig.Subscribe(nameof(fig.EspressoMachineIsEnabled), pc => {
      Console.WriteLine("Setting changed from " + pc.PreviousValue + " to " + pc.CurrentValue);
   });

   fig.EspressoMachineIsEnabled = false;
```

the config looks like this:

```xml
   <appSettings>
      <add key="fig:espressomachineenabled" value="true"/>
      <add key="fig:CoffeeRefillInterval" value="00:42:00"/>
      <add key="whatever" value="Ignored , no fig: prefix"/>
   </appSettings>
```

But what about appsettings.json for .NET Core? No worries, we've
got you covered:

```csharp
   var provider = new AppSettingsJson();
   var fig = new CoffeeShopSettings(provider);
```
```javascript
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
  "AllowedHosts": "*",
}
```

## Features
* Strongly typed and automatic type conversion
* Default values
* Update runtime values (Add a setter)
* Change notifications using pub/sub model
* Thread safe
* Namespace prefixes, ie db.writer.timerinterval=42
* case insensitive keys
* Providers
* Provider for App.Config / Web.config
* Provider for appsettings.json

## Contributing
Contributions are welcome! Create an issue for the
feature (see below!) you want to work on,
then we discuss details in the comments.

## Features to do or consider, in no particular order

* Environment support, example PROD, DEV, TEST
* Encryption - encrypt with a tool, decrypt with a key provided at runime (env var, constructor arg)
* Validation - required properties, undefined properties (not mapped)
* provider bootstrapping, choose at startup using heuristics
* builder pattern to configure providers
* Add providers
 - ini files (memstate)
 - env vars
* Cascading providers - example: use env var if exists, otherwise take from appsettings
* Expose the TypeConverter for extension or replacement
* custom converters - if we have a custom type not recognized by the TypeConverter
* Custom validators - say we have a string list of IP adresses, we might want to parse them or even ensure they are resolvable
* array values, GetArray() or array type on the property
* Diagnostics: which provider did a value come from?
* Dump values (INI, text)
* Reload from provider (all or some, on demand, on schedule)
* OriginalValue vs CurrentValue will behave oddly if both refer to same object
* Write values back to underlying (could be useful for mobile applications)
* ConnectionStrings in AppSettingsXml provider