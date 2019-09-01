using System.Collections.Generic;
using Fig;
using Fig.Test;
using NUnit.Framework;

namespace Tests
{
    public class Tests
    {
        [Test]
        public void CanSubscribe()
        {
            var config = new ExampleConfig();
            PropertyChanged received = null;
            config.MyIntProperty = 169;
            config.Subscribe(nameof(config.MyIntProperty), pc => received = pc);

            config.MyIntProperty = 200;
            Assert.NotNull(received);
            Assert.AreEqual(200, received.CurrentValue);
            Assert.IsNull(received.OriginalValue);
            Assert.AreEqual(169, received.PreviousValue);
        }

        [Test]
        public void CanReadDefault()
        {
			var config = new ExampleConfig();
			Assert.AreEqual(42, config.MyIntProperty);
		}

        [Test]
        public void CanUpdateRuntimeValue()
        {
            var config = new ExampleConfig();
            config.MyIntProperty = 100;
            Assert.AreEqual(100, config.MyIntProperty);
        }

        [Test]
        public void MissingPropertyWithoutDefaultFails()
        {
            var config = new ExampleConfig();
            Assert.Throws<KeyNotFoundException>(() => {
                var _ = config.MissingDefault;
            });
        }

        [Test]
        public void MissingPropertyWithoutDefaultFailsValidation()
        {
            var config = new ExampleConfig();

            Assert.Throws<ConfigurationException>(() => {
                config.Validate();
            });
        }

        [Test]
        public void MissingValueCanBeAssignedAndRetrieved()
        {
            var config = new ExampleConfig();
            config.MissingDefault = 150;
            var actual = config.Get<int>(nameof(config.MissingDefault));
            Assert.AreEqual(150, actual);
        }
    }
}