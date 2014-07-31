using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Xunit;

namespace SettingsProviderNet.Tests
{
    public class JsonSettingsStoreBaseTest
    {
        readonly TestStorage store;

        public JsonSettingsStoreBaseTest()
        {
            store = new TestStorage();
        }

        private class TestStringClass
        {
            public string StringProp { get; set; }
        }

        private string CompressJSON(string Input)
        {
            return String.Join("", Input.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                                        .Select(l => l.TrimStart(new char[] { ' ' })));
        }

        [Fact]
        public void json_setting_store_can_serialize_string()
        {
            store.Save<TestStringClass>("test1", new TestStringClass { StringProp = "abc" });

            var json = CompressJSON(store.Files["test1.settings"]);

            Assert.Equal("{\"StringProp\": \"abc\"}", json);
        }

        [Fact]
        public void json_setting_store_can_deserialize_string()
        {
            store.Files["test1.settings"] = "{\"StringProp\": \"abc\"}";

            TestStringClass settings = store.Load<TestStringClass>("test1");

            Assert.Equal("abc", settings.StringProp);
        }

        [Fact]
        public void json_setting_store_can_serialize_unicode_string()
        {
            store.Save<TestStringClass>("test1", new TestStringClass { StringProp = "居酒屋" });

            var json = CompressJSON(store.Files["test1.settings"]);

            Assert.Equal("{\"StringProp\": \"居酒屋\"}", json);
        }

        [Fact]
        public void json_setting_store_can_deserialize_unicode_string()
        {
            store.Files["test1.settings"] = "{\"StringProp\": \"居酒屋\"}";

            TestStringClass settings = store.Load<TestStringClass>("test1");

            Assert.Equal("居酒屋", settings.StringProp);
        }

        private class TestIntClass
        {
            public int IntProp { get; set; }
        }

        [Fact]
        public void json_setting_store_can_serialize_int()
        {
            store.Save<TestIntClass>("test1", new TestIntClass { IntProp = 123 });

            var json = CompressJSON(store.Files["test1.settings"]);

            Assert.Equal("{\"IntProp\": 123}", json);
        }

        [Fact]
        public void json_setting_store_can_deserialize_int()
        {
            store.Files["test1.settings"] = "{\"IntProp\": 123}";

            TestIntClass settings = store.Load<TestIntClass>("test1");

            Assert.Equal(123, settings.IntProp);
        }

        private class TestUShortClass
        {
            public ushort UShortProp { get; set; }
        }

        [Fact]
        public void json_setting_store_can_serialize_ushort()
        {
            store.Save<TestUShortClass>("test1", new TestUShortClass { UShortProp = 123 });

            var json = CompressJSON(store.Files["test1.settings"]);

            Assert.Equal("{\"UShortProp\": 123}", json);
        }

        [Fact]
        public void json_setting_store_can_deserialize_ushort()
        {
            store.Files["test1.settings"] = "{\"UShortProp\": 123}";

            TestUShortClass settings = store.Load<TestUShortClass>("test1");

            Assert.Equal(123, settings.UShortProp);
        }

        private class TestBoolClass
        {
            public bool BoolProp { get; set; }
        }

        [Fact]
        public void json_setting_store_can_serialize_bool()
        {
            store.Save<TestBoolClass>("test1", new TestBoolClass { BoolProp = true });

            var json = CompressJSON(store.Files["test1.settings"]);

            Assert.Equal("{\"BoolProp\": true}", json);
        }

        [Fact]
        public void json_setting_store_can_deserialize_bool()
        {
            store.Files["test1.settings"] = "{\"BoolProp\": false}";

            TestBoolClass settings = store.Load<TestBoolClass>("test1");

            Assert.Equal(false, settings.BoolProp);
        }

        private class TestDateTimeClass
        {
            public DateTime? DateTimeProp { get; set; }
        }

        [Fact]
        public void json_setting_store_can_serialize_nullable_datetime()
        {
            var Current = DateTime.Now;
            store.Save<TestDateTimeClass>("test1", new TestDateTimeClass { DateTimeProp = Current });

            var json = CompressJSON(store.Files["test1.settings"]);

            Assert.Equal("{\"DateTimeProp\": \"" + Current.ToString("o") + "\"}", json);
        }

        [Fact]
        public void json_setting_store_can_deserialize_nullable_datetime()
        {
            var Current = DateTime.Now;
            store.Files["test1.settings"] = "{\"DateTimeProp\": \"" + Current.ToString("o") + "\"}";

            TestDateTimeClass settings = store.Load<TestDateTimeClass>("test1");

            Assert.Equal(Current, settings.DateTimeProp);
        }

        [Fact]
        public void json_setting_store_can_serialize_null_nullable_datetime()
        {
            var Current = DateTime.Now;
            store.Save<TestDateTimeClass>("test1", new TestDateTimeClass());

            var json = CompressJSON(store.Files["test1.settings"]);

            Assert.Equal("{\"DateTimeProp\": null}", json);
        }

        [Fact]
        public void json_setting_store_can_deserialize_null_nullable_datetime()
        {
            store.Files["test1.settings"] = "{\"DateTimeProp\": null}";

            TestDateTimeClass settings = store.Load<TestDateTimeClass>("test1");

            Assert.Equal(null, settings.DateTimeProp);
        }

        private class TestListClass
        {
            public List<string> ListProp { get; set; }
        }

        [Fact]
        public void json_setting_store_can_serialize_list()
        {
            store.Save<TestListClass>("test1", new TestListClass { ListProp = new List<string>() { "abc", "def" } });

            var json = CompressJSON(store.Files["test1.settings"]);

            Assert.Equal("{\"ListProp\": [\"abc\",\"def\"]}", json);
        }

        [Fact]
        public void json_setting_store_can_deserialize_list()
        {
            store.Files["test1.settings"] = "{\"ListProp\": [\"abc\",\"def\"]}";

            TestListClass settings = store.Load<TestListClass>("test1");

            Assert.Equal(new List<string>() { "abc", "def" }, settings.ListProp);
        }

        private class TestClass
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
            public bool BoolProp { get; set; }
            public DateTime? DateTimeProp { get; set; }
        }

        [Fact]
        public void json_setting_store_can_deserialize_object()
        {
            var Current = DateTime.Now;
            string json = "{" +
                            "\"StringProp\": \"abc\"," +
                            "\"IntProp\": 123," +
                            "\"BoolProp\": true," +
                            "\"DateTimeProp\": \"" + Current.ToString("o") + "\"" +
                            "}";
            store.Files["test1.settings"] = json;

            var settings = store.Load<TestClass>("test1");

            Assert.Equal("abc", settings.StringProp);
            Assert.Equal(123, settings.IntProp);
            Assert.Equal(true, settings.BoolProp);
            Assert.Equal(Current, settings.DateTimeProp);
        }

        [Fact]
        public void json_setting_store_LoadAndUpdate_can_update_properties()
        {
            var Current = DateTime.Now;
            string json = "{" +
                            "\"IntProp\": 123," +
                            "\"BoolProp\": true," +
                            "}";
            store.Files["test1.settings"] = json;
            
            var settings = store.LoadAndUpdate<TestClass>("test1", new TestClass {
                StringProp = "abc",
                IntProp = 456,
                BoolProp = false,
                DateTimeProp = Current
            });

            Assert.Equal(123, settings.IntProp);
            Assert.Equal(true, settings.BoolProp);
        }

        [Fact]
        public void json_setting_store_LoadAndUpdate_can_preserve_properties()
        {
            var Current = DateTime.Now;
            string json = "{" +
                            "\"IntProp\": 123," +
                            "\"BoolProp\": true," +
                            "}";
            store.Files["test1.settings"] = json;

            var settings = store.LoadAndUpdate<TestClass>("test1", new TestClass
            {
                StringProp = "abc",
                IntProp = 456,
                BoolProp = false,
                DateTimeProp = DateTime.Now
            });

            Assert.Equal("abc", settings.StringProp);
            Assert.Equal(Current, settings.DateTimeProp);
        }

        private class TestObject
        {
            public TestStringClass child { get; set; }
        }

        [Fact]
        public void json_setting_store_can_serialize_complex_object()
        {
            store.Save<TestObject>("test1", new TestObject { child = new TestStringClass { StringProp = "abc" } });

            var json = CompressJSON(store.Files["test1.settings"]);
            
            Assert.Equal("{\"child\": {\"StringProp\": \"abc\"}}", json);
        }
    }
}
