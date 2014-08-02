using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace SettingsProviderNet
{
    // ReSharper disable InconsistentNaming

    public class SettingsProvider : ISettingsProvider
    {
        protected readonly ISettingsStorage settingsRepository;
        protected readonly Dictionary<Type, object> cache = new Dictionary<Type, object>();
        protected readonly string secretKey;

        public SettingsProvider(ISettingsStorage settingsRepository = null, string secretKey = null)
        {
            this.settingsRepository = settingsRepository ?? new IsolatedStorageSettingsStore();
            this.secretKey = secretKey;
        }

        protected virtual string GetKey<T>()
        {
            return typeof(T).Name;
        }

        public virtual T GetSettings<T>(bool fresh = false) where T : new()
        {
            var type = typeof (T);
            if (!fresh && cache.ContainsKey(type))
                return (T)cache[type];

            var settings = GetDefaultSettings<T>();

            // Fill in settings got from repository and decrypt protected setting
            settings = DecryptProtectedSettings(
                    settingsRepository.LoadAndUpdate(GetKey<T>(), settings)
                );

            cache[type] = settings;

            return settings;
        }

        protected T DecryptProtectedSettings<T>(T settings)
        {
            var settingsMetadata = ReadSettingMetadata<T>();
            foreach (var setting in settingsMetadata)
            {
                if (setting.IsProtected)
                {
                    // Only run decrypt on string
                    var value = setting.ReadValue(settings);
                    var str = value as string;
                    var decryptedValue = String.IsNullOrEmpty(str)
                                        ? value
                                        : ProtectedDataUtils.Decrypt(str, secretKey ?? typeof(SettingDescriptor).FullName);
                    setting.WriteValue(settings, decryptedValue);
                }
            }
            return settings;
        }

        // Protected settings are encrypted
        protected virtual T GetDefaultSettings<T>() where T : new()
        {
            var settings = new T();
            var settingsMetadata = ReadSettingMetadata<T>();
            foreach (var setting in settingsMetadata)
            {
                setting.WriteValue(settings, GetDefaultValue(setting));
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
            SaveSettings(settings, true);
        }

        protected void SaveSettings<T>(T settings, bool saveDefaultValue) where T : new()
        {
            var type = typeof(T);
            // Retain orignal instance
            var oldSettings = cache.ContainsKey(type) ? (T)cache[type] : new T();
            // Decrypt for compare
            var defaultSettings = DecryptProtectedSettings(GetDefaultSettings<T>());
            // Use ExpandoObject to handle saving, which allow us to skip saving default value
            IDictionary<string, Object> settingToSave = new ExpandoObject();

            var settingsMetadata = ReadSettingMetadata<T>();
            foreach (var setting in settingsMetadata)
            {
                // Replace null with defaultValue, write result to oldSettings
                var defaultValue = setting.ReadValue(defaultSettings);
                var value = setting.ReadValue(settings) ?? defaultValue;
                setting.WriteValue(oldSettings, value);

                // Hack for comparing string
                string str = value as string;

                if (saveDefaultValue || !ObjectCompareUtil.IsEqual(value, defaultValue))
                {
                    settingToSave[setting.Key] = !setting.IsProtected || String.IsNullOrEmpty(str)
                                                ? value
                                                : ProtectedDataUtils.Encrypt(str, secretKey ?? typeof(SettingDescriptor).FullName);
                }
            }

            // Update cache
            cache[typeof(T)] = oldSettings;

            settingsRepository.Save<ExpandoObject>(GetKey<T>(), settingToSave as ExpandoObject);
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
            var type = typeof (T);
            T settings = cache.ContainsKey(type) ? (T)cache[type] : new T();

            var settingMetadata = ReadSettingMetadata<T>();
            foreach (var setting in settingMetadata)
            {
                setting.WriteValue(settings, GetDefaultValue(setting));
            }

            settings = DecryptProtectedSettings(settings);

            SaveSettings<T>(settings);

            return settings;
        }
    }

    // ReSharper restore InconsistentNaming
}