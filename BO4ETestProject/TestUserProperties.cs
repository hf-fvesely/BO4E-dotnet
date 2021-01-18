﻿using System;

using BO4E;
using BO4E.BO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace TestBO4E
{
    [TestClass]
    public class TestUserProperties
    {
        [TestMethod]
        public void TestDeserialization()
        {
            var meloJson = @"{'messlokationsId': 'DE0123456789012345678901234567890', 'sparte': 'STROM', 'myCustomInfo': 'some_value_not_covered_by_bo4e', 'myCustomValue': 123.456}";
            var melo = JsonConvert.DeserializeObject<Messlokation>(meloJson);
            Assert.IsTrue(melo.IsValid());
            Assert.IsNotNull(melo.UserProperties);
            Assert.AreEqual("some_value_not_covered_by_bo4e", melo.UserProperties["myCustomInfo"] as string);
            Assert.AreEqual(123.456M, (decimal)(double)melo.UserProperties["myCustomValue"]);
        }

        private void _AssertUserProperties(Messlokation melo)
        {
            Assert.IsTrue(melo.TryGetUserProperty("myCustomInfo", out string myCustomValue));
            Assert.AreEqual("some_value_not_covered_by_bo4e", myCustomValue);
            Assert.IsTrue(melo.UserPropertyEquals("myCustomValue", 123.456M));
            Assert.IsFalse(melo.UserPropertyEquals("myCustomValue", "foo"));

            Assert.IsFalse(melo.TryGetUserProperty("something else", out string _));
            Assert.AreEqual("default value", melo.GetUserProperty("something else", "default value"));
            Assert.IsFalse(melo.UserPropertyEquals("myCustomInfo", 888.999M)); // the cast exception is catched inside.
            Assert.IsFalse(melo.UserPropertyEquals("myCustomValue", "asd")); // the cast exception is catched inside.
            Assert.IsFalse(melo.UserPropertyEquals("some_key_that_was_not_found", "asd"));
            Assert.IsTrue(melo.EvaluateUserProperty<string, bool, Messlokation>("myCustomInfo", up => !string.IsNullOrEmpty(up)));

            melo.UserProperties = null;
            Assert.IsFalse(melo.UserPropertyEquals("there are no user properties", "asd"));
            Assert.IsFalse(melo.TryGetUserProperty("there are no user properties", out string _));
            Assert.ThrowsException<ArgumentNullException>(() => melo.EvaluateUserProperty<string, bool, Messlokation>("there are no user properties", _ => default));
            Assert.IsFalse(melo.UserPropertyEquals("myNullProp", true));
        }

        private const string meloJson =@"{""messlokationsId"": ""DE0123456789012345678901234567890"", ""sparte"": ""STROM"", ""myCustomInfo"": ""some_value_not_covered_by_bo4e"", ""myCustomValue"": 123.456, ""myNullProp"": null}";

        [TestMethod]
        public void TestTryGetUserProperties()
        { 
            var melo = JsonConvert.DeserializeObject<Messlokation>(meloJson);
            _AssertUserProperties(melo);
        }

        [TestMethod]
        public void TestTryGetUserPropertiesNet5()
        {
            var melo = System.Text.Json.JsonSerializer.Deserialize<Messlokation>(meloJson);
           _AssertUserProperties(melo);
        }

        [TestMethod]
        public void TestFlags()
        {
            var melo = new Messlokation
            {
                MesslokationsId = "DE0123456789012345678901234567890",
                Sparte = BO4E.ENUM.Sparte.STROM
            };
            Assert.IsNull(melo.UserProperties);
            Assert.IsFalse(melo.HasFlagSet("foo"));
            Assert.IsTrue(melo.SetFlag<Messlokation>("foo"));
            Assert.IsNotNull(melo.UserProperties);
            Assert.IsTrue(melo.UserProperties.TryGetValue("foo", out var upValue) && (bool)upValue);
            Assert.IsTrue(melo.HasFlagSet("foo"));
            Assert.IsFalse(melo.SetFlag<Messlokation>("foo"));
            Assert.IsTrue(melo.SetFlag<Messlokation>("foo", false));
            Assert.IsFalse(melo.HasFlagSet("foo"));
            Assert.IsTrue(melo.SetFlag<Messlokation>("foo", null));
            Assert.IsFalse(melo.UserProperties.TryGetValue("foo", out var _));
            Assert.IsFalse(melo.SetFlag<Messlokation>("foo", null));
            Assert.IsFalse(melo.HasFlagSet("foo"));
            Assert.IsTrue(melo.SetFlag<Messlokation>("foo"));

            melo.UserProperties["foo"] = null;
            Assert.IsFalse(melo.HasFlagSet("foo"));
        }
    }
}