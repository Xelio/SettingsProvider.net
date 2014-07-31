using System.Collections.Generic;

namespace SettingsProviderNet
{
    public interface ISettingsStorage
    {
        void Save<T>(string key, T settings);
        T Load<T>(string key) where T : new();
        T LoadAndUpdate<T>(string key, T settings);
    }
}