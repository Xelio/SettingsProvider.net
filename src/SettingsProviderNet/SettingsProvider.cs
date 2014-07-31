using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SettingsProviderNet
{
    // ReSharper disable InconsistentNaming

    public class SettingsProvider : ISettingsProvider
    {
        readonly ISettingsStorage settingsRepository;
        readonly Dictionary<Type, object> cache = new Dictionary<Type, object>();
        readonly string secretKey;

        public SettingsProvider(ISettingsStorage settingsRepository = null, string secretKey = null)
        {
            this.settingsRepository = settingsRepository ?? new IsolatedStorageSettingsStore();
            this.secretKey = secretKey;
        }

        static string GetKey<T>()
        {
            return typeof(T).Name;
        }

        public virtual T GetSettings<T>(bool fresh = false) where T : new()
        {
            var type = typeof (T);
            if (!fresh && cache.ContainsKey(type))
                return (T)cache[type];

            var settings = GetDefaultSettings<T>();

            settings = settingsRepository.LoadAndUpdate(GetKey<T>(), settings);

            cache[type] = settings;

            return settings;
        }

        T GetDefaultSettings<T>() where T : new()
        {
            var settings = new T();
            var settingsMetadata = ReadSettingMetadata<T>();
            foreach (var setting in settingsMetadata)
            {
                setting.Write(settings, GetDefaultValue(setting));
            }
            return settings;
        }

        object GetDefaultValue(ISettingDescriptor setting)
        {
            var value = setting.DefaultValue ?? GetDefault(setting.Property.PropertyType);

            if (setting.IsProtected && value != null)
                value = ProtectedDataUtils.Encrypt((string)value, secretKey ?? typeof(SettingDescriptor).FullName);

            return value;
        }

        static object GetDefault(Type type)
        {
            if (IsList(type))
            {
                return Activator.CreateInstance(type.IsClass ? type : typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]));
            }
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private static bool IsList(Type propertyType)
        {
            return
                typeof(IList).IsAssignableFrom(propertyType) ||
                (propertyType.IsGenericType && typeof(IList<>) == propertyType.GetGenericTypeDefinition());
        }

        public virtual void SaveSettings<T>(T settings) where T : new()
        {
            var type = typeof(T);
            T oldSettings = cache.ContainsKey(type) ? (T)cache[type] : new T();
            T defaultSettings = GetDefaultSettings<T>();

            var settingsMetadata = ReadSettingMetadata<T>();

            foreach (var setting in settingsMetadata)
            {
                var value = setting.ReadValue(settings) ?? setting.ReadValue(defaultSettings);
                setting.Write(oldSettings, value);
            }

            cache[typeof(T)] = oldSettings;

            settingsRepository.Save<T>(GetKey<T>(), (T)oldSettings);
        }

        internal static string GetLegacyKey<T>(ISettingDescriptor setting)
        {
            var settingsType = typeof(T);

            return string.Format("{0}.{1}", settingsType.FullName, setting.Property.Name);
        }

        public virtual IEnumerable<ISettingDescriptor> ReadSettingMetadata<T>()
        {
            return ReadSettingMetadata(typeof(T));
        }

        public virtual IEnumerable<ISettingDescriptor> ReadSettingMetadata(Type settingsType)
        {
            return settingsType.GetProperties()
                .Where(x => x.CanRead && x.CanWrite)
                .Select(x => new SettingDescriptor(x, secretKey))
                .ToArray();
        }

        public virtual T ResetToDefaults<T>() where T : new()
        {
            T settings;

            var type = typeof (T);
            if (cache.ContainsKey(type))
            {
                settings = (T)cache[type];
                var settingMetadata = ReadSettingMetadata<T>();

                foreach (var setting in settingMetadata)
                {
                    setting.Write(settings, GetDefaultValue(setting));
                }

            }
            else
            {
                settings = GetDefaultSettings<T>();
            }

            SaveSettings<T>(settings);

            return settings;
        }
    }

    // ReSharper restore InconsistentNaming
}