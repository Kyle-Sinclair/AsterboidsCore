using Config;
using GameSystems.Services;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystems {
    public class AsterboidsManager : MonoBehaviour, IGameService {

        private GameObject _boidControllerPrefab;
        private float _boidSpawnDistance;
        private int _asterboidAverageCount;

        [Header("Boids")]
        private BoidController[] _activeBoidControllers;
        private BoidController[] _inactiveBoidControllers;
        private int _boidControllerAverageCount;
        private int _boidControllerCount;
        

        public void ConfigureService() {
            ConfigScriptable _config =  ServiceLocator.Current.Get<ConfigManager>().GetConfig();
            _boidControllerPrefab = _config.BoidControllerPrefab;
            _boidSpawnDistance = _config._boidSpawnDistance;
            _boidControllerAverageCount = _config._boidControllerAverageCount;
            _asterboidAverageCount = _config._boidAverageAsterboids;
            Initialize();
        }

        private void Initialize() {
            _activeBoidControllers = new BoidController[_boidControllerAverageCount + 5];
            ConfigScriptable _config =  ServiceLocator.Current.Get<ConfigManager>().GetConfig();

            for (int i = 0; i < _activeBoidControllers.Length; i++) {
                _activeBoidControllers[i] = Instantiate(_boidControllerPrefab,_config.PlayerStartPosition + _config.BoidcontrollerSpawnDistance,Quaternion.identity).GetComponent<BoidController>();
                _activeBoidControllers[i].Init(_config._boidAverageAsterboids);
            }
        }
    }
}
