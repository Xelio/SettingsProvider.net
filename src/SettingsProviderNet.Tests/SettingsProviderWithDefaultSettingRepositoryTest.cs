using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;

namespace SettingsProviderNet.Tests
{
    public class SettingsProviderWithDefaultSettingRepositoryTest
    {
        readonly SettingsProviderWithDefaultSettingRepository settingsWDSRRetreiver;
        readonly SettingsProviderWithDefaultSettingRepository settingsWDSRSaver;

        readonly SettingsProvider settingsProvider;

        readonly TestStorage store;
        readonly TestStorage store2;
        public SettingsProviderWithDefaultSettingRepositoryTest()
        {
            store = new TestStorage();
            store2 = new TestStorage();

            settingsWDSRRetreiver = new SettingsProviderWithDefaultSettingRepository(store);
            settingsWDSRSaver = new SettingsProviderWithDefaultSettingRepository(store);
            settingsProvider = new SettingsProvider(store2);
        }

        [Fact]
        public void settings_provider_wdsr_would_save_default_value_to_default_repository()
        { 
            settingsWDSRSaver.SaveSettings(new TestSettings());

            store2.Save<TestSettings>("TestSettings", store.Load<TestSettings>("TestSettings.default"));
            TestSettings defaultSettings = settingsProvider.GetSettings<TestSettings>();

            Assert.Equal(defaultSettings.TestProp1, "foo");
            Assert.Equal(defaultSettings.ProtectedStringWithDefault, "test");
            Assert.Equal(defaultSettings.FirstRun, null);
        }

        [Fact]
        public void settings_provider_wdsr_would_not_save_non_default_value_to_default_repository()
        {
            settingsWDSRSaver.SaveSettings(new TestSettings { TestProp1 = "abc" });

            TestSettings defaultSettings = store.Load<TestSettings>("TestSettings.default");

            Assert.Equal(defaultSettings.TestProp1, "foo");
        }

        [Fact]
        public void settings_provider_wdsr_would_save_non_default_value_to_non_default_repository()
        {
            settingsWDSRSaver.SaveSettings(new TestSettings { TestProp1 = "abc" });

            TestSettings overridedSettings = store.Load<TestSettings>("TestSettings");

            Assert.Equal(overridedSettings.TestProp1, "abc");
        }

        [Fact]
        public void settings_provider_wdsr_would_not_save_default_value_to_non_default_repository()
        {
            settingsWDSRSaver.SaveSettings(new TestSettings());

            TestSettings overridedSettings = store.Load<TestSettings>("TestSettings");

            Assert.Equal(overridedSettings.TestProp1, null);
        }

        [Fact]
        public void settings_provider_wdsr_would_not_save_to_default_repository_twice()
        {
            settingsProvider.SaveSettings(new TestSettings { TestProp1 = "bar", FirstRun = DateTime.Now});
            // Copy to defalut repository
            store.Save<TestSettings>("TestSettings.default", store2.Load<TestSettings>("TestSettings"));

            TestSettings settings = settingsWDSRSaver.GetSettings<TestSettings>();

            Assert.Equal(settings.TestProp1, "bar");
            Assert.Equal(1, store.WriteCount);
        }

        [Fact]
        public void settings_provider_wdsr_can_save_and_persist_int()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { TestProp2 = 123 });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal(123, settings.TestProp2);
        }

        [Fact]
        public void settings_provider_wdsr_can_save_and_persist_bool()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { Boolean = true });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.True(settings.Boolean);
        }

        [Fact]
        public void settings_provider_wdsr_can_save_and_retreive_string()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { TestProp1 = "bar" });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal("bar", settings.TestProp1);
        }

        [Fact]
        public void settings_provider_wdsr_can_save_and_retreive_japanese_string()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { TestProp1 = "居酒屋" });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal("居酒屋", settings.TestProp1);
        }

        [Fact]
        public void settings_provider_wdsr_can_save_and_retreive_nullable_datetime()
        {
            // arrange
            var firstRun = DateTime.Now;
            settingsWDSRSaver.SaveSettings(new TestSettings { FirstRun = firstRun });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.True(settings.FirstRun.HasValue);
            Debug.Assert(settings.FirstRun != null, "settings.FirstRun != null"); //R# annotations broken
            Assert.Equal(firstRun.ToString(CultureInfo.InvariantCulture), settings.FirstRun.Value.ToString(CultureInfo.InvariantCulture));
        }

        [Fact]
        public void settings_provider_wdsr_can_save_and_retreive_null_nullable_datetime()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { FirstRun = null });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Null(settings.FirstRun);
        }

        [Fact]
        public void settings_provider_wdsr_can_save_and_retreive_list()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { List2 = new List<int> { 123 } });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal(123, settings.List2.Single());
        }

        [Fact]
        public void settings_provider_wdsr_can_save_and_retreive_list_with_japanese_characters()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { List = new List<string> { "居酒屋" } });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal("居酒屋", settings.List.Single());
        }

        [Fact]
        public void settings_provider_wdsr_loads_default_values()
        {
            // arrange

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal("foo", settings.TestProp1);
        }

        [Fact]
        public void settings_provider_wdsr_ignores_properties_with_no_setters()
        {
            // arrange

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.NotNull(settings.NoSetter);
        }

        [Fact]
        public void settings_provider_wdsr_can_save_and_retreive_enum()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { SomeEnum = MyEnum.Value2 });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal(MyEnum.Value2, settings.SomeEnum);
        }

        [Fact]
        public void settings_provider_wdsr_can_reset_to_defaults()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { TestProp1 = "bar" });

            // act
            settingsWDSRRetreiver.ResetToDefaults<TestSettings>();
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal("foo", settings.TestProp1);
        }

        [Fact]
        public void settings_provider_wdsr_reset_to_defaults_save_to_default_repository()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { TestProp1 = "bar", TestProp2 = 123 });

            // act
            settingsWDSRRetreiver.ResetToDefaults<TestSettings>();
            var defaultSettings = store.Load<TestSettings>("TestSettings.default");
            var overridedSettings = store.Load<TestSettings>("TestSettings");

            // assert
            Assert.Equal(0, overridedSettings.TestProp2);
            Assert.Equal(0, overridedSettings.TestProp2);

            Assert.Equal("foo", defaultSettings.TestProp1);
            Assert.Equal(null, overridedSettings.TestProp1);
        }

        [Fact]
        public void settings_provider_wdsr_recycles_same_instance_on_reset()
        {
            // arrange
            var instance = settingsWDSRRetreiver.GetSettings<TestSettings>();
            settingsWDSRSaver.SaveSettings(new TestSettings { TestProp1 = "bar" });

            // act
            var settings = settingsWDSRRetreiver.ResetToDefaults<TestSettings>();
            var settings2 = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Same(instance, settings);
            Assert.Same(instance, settings2);
        }

        [Fact]
        public void settings_provider_wdsr_returns_fresh_instance_when_requested()
        {
            // arrange
            var instance = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // act
            var instance2 = settingsWDSRRetreiver.GetSettings<TestSettings>(true);

            // assert
            Assert.NotSame(instance, instance2);
        }

        [Fact]
        public void settings_provider_wdsr_defaults_to_empty_ilist()
        {
            // arrange
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // act

            // assert
            Assert.NotNull(settings.IdList);
            Assert.IsType<List<Guid>>(settings.IdList);
            Assert.Empty(settings.IdList);
        }

        [Fact]
        public void settings_provider_wdsr_defaults_to_empty_list()
        {
            // arrange
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // act

            // assert
            Assert.NotNull(settings.List2);
            Assert.IsType<List<int>>(settings.List2);
            Assert.Empty(settings.List2);
        }

        //[Fact]
        //public void settings_provider_wdsr_can_specify_key()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void settings_provider_wdsr_can_load_legacy_settings()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void settings_provider_wdsr_settings_are_not_fully_qualified()
        //{
        //    throw new NotImplementedException();
        //}

        [Fact]
        public void settings_provider_wdsr_Can_serialise_complex_types()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings
            {
                Complex = new ComplexType
                {
                    SomeProp = "Value"
                }
            });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal("Value", settings.Complex.SomeProp);
        }

        [Fact]
        public void settings_provider_wdsr_can_save_and_retreive_protected_string()
        {
            // arrange
            settingsWDSRSaver.SaveSettings(new TestSettings { ProtectedString = "crypto" });

            // act
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>();

            // assert
            Assert.Equal("crypto", settings.ProtectedString);
        }

        [Fact]
        public void settings_provider_wdsr_retreive_protected_defaultvalue_string()
        {
            // act
            settingsWDSRRetreiver.GetSettings<TestSettings>();
            var settings = settingsWDSRRetreiver.GetSettings<TestSettings>(true);

            // assert
            Assert.Equal("test", settings.ProtectedStringWithDefault);
        }
    }
}
