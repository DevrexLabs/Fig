using NUnit.Framework;

namespace Fig.Test;

public class ArrayBindingTests
{
     [Test]
     public void CanBindToArray()
     {
          var settings = new SettingsBuilder()
               .UseAppSettingsJson("appsettings.json", required: true)
               .Build();

          var servers = settings.Get<string[]>("Servers");
          Assert.AreEqual(2,servers.Length);
          Assert.AreEqual("10.0.0.1", servers[0]);
          Assert.AreEqual("10.0.0.2", servers[1]);
          
          var mySettings = settings.Bind<MySettings>(path:"");
          Assert.AreEqual(2, mySettings.Servers.Length);
          Assert.AreEqual("10.0.0.1", mySettings.Servers[0]);
          Assert.AreEqual("10.0.0.2", mySettings.Servers[1]);
          
     }

     private class MySettings
     {
          public string[] Servers { get; set; }
     }
}