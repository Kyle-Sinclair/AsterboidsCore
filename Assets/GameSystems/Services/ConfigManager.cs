using Config;
using UnityEngine;

namespace GameSystems.Services {
    public class ConfigManager : IGameService
    {
        private ConfigScriptable _config;

        public ConfigScriptable GetConfig() {
            return _config;
        }

        public void ConfigureService() {
            _config = Resources.Load<ConfigScriptable>("ConfigFile");
            if (_config == null) {
                Debug.LogError("Configuration data stored incorrectly");
            }
        }
    }
}
