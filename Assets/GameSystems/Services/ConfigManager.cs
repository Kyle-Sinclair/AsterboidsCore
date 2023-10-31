using Config;
using UnityEngine;

namespace GameSystems.Services {
    public class ConfigManager : GameService
    {
        private ConfigScriptable _config;

        public ConfigScriptable GetConfig() {
            return _config;
        }

        public override void ConfigureService() {
            Debug.Log("configuring config manager");
            _config = Resources.Load<ConfigScriptable>("ConfigFile");
            if (_config == null) {
                Debug.LogError("Configuration data stored incorrectly");
            }
        }
    }
}
