using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace SettingsProviderNet
{
    public class SettingsProviderWithDefaultSettingRepository : SettingsProvider
    {
        protected ISettingsProvider defaultSettingProvider;

        public SettingsProviderWithDefaultSettingRepository(ISettingsStorage settingsRepository = null, string secretKey = null)
            : base(settingsRepository, secretKey)
        {
            defaultSettingProvider = new DefaultSettingProvider(settingsRepository, secretKey);
        }

        public override T GetSettings<T>(bool fresh = false)
        {
            if (fresh) defaultSettingProvider.GetSettings<T>(true);

            return base.GetSettings<T>(fresh);
        }

        protected override T GetDefaultSettings<T>()
        {
            var defaultSettings = defaultSettingProvider.GetSettings<T>();

            // Clone and encrypt protected settings
            var result = new T();
            var settingsMetadata = ReadSettingMetadata<T>();
            foreach (var setting in settingsMetadata)
            {
                var value = setting.ReadValue(defaultSettings);

                var str = value as string;
                if (setting.IsProtected && !String.IsNullOrEmpty(str))
                {
                    value = ProtectedDataUtils.Encrypt(str, secretKey ?? typeof(SettingDescriptor).FullName);
                }
                setting.WriteValue(result, value);
            }

            return result;
        }
        protected T EncryptProtectedSettings<T>(T settings)
        {
            var settingsMetadata = ReadSettingMetadata<T>();
            foreach (var setting in settingsMetadata)
            {
                if (setting.IsProtected)
                {
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

        public override void SaveSettings<T>(T settings)
        {
            SaveSettings(settings, false);
        }

        private class DefaultSettingProvider : SettingsProvider
        {
            public DefaultSettingProvider(ISettingsStorage settingsRepository = null, string secretKey = null)
                : base(settingsRepository, secretKey)
            { }
            protected override string GetKey<T>()
            {
                return typeof(T).Name + ".default";
            }

            public override T GetSettings<T>(bool fresh = false)
            {
                T settingsFromRepo = DecryptProtectedSettings(settingsRepository.Load<T>(GetKey<T>()));
                T settings = base.GetSettings<T>(fresh);

                var settingsMetadata = ReadSettingMetadata<T>();
                if (settingsMetadata.Any(m => !ObjectCompareUtil.IsEqual(m.ReadValue(settingsFromRepo), m.ReadValue(settings))))
                {
                    SaveSettings<T>(settings);
                }

                return settings;
            }
        }
    }
}
