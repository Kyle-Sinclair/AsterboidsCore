using System;
using Config;
using GameSystems.Services;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

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

        public void Update() {
            
            float delta = Time.deltaTime;

            for (int i = 0; i < _activeBoidControllers.Length; i++) {
                _activeBoidControllers[i].FirstUpdate();
            } 
            for (int i = 0; i < _activeBoidControllers.Length; i++) {
                _activeBoidControllers[i].EarlyUpdate(delta);
            } 
            for (int i = 0; i < _activeBoidControllers.Length; i++) {
                _activeBoidControllers[i].GameUpdate(delta);
            }
            for (int i = 0; i < _activeBoidControllers.Length; i++) {
                _activeBoidControllers[i].LastUpdate();
            }
        }

        private void Initialize() {
            _activeBoidControllers = new BoidController[_boidControllerAverageCount];
            ConfigScriptable _config =  ServiceLocator.Current.Get<ConfigManager>().GetConfig();
            for (int i = 0; i < _activeBoidControllers.Length; i++) {
                _activeBoidControllers[i] = Instantiate(_boidControllerPrefab,_config.PlayerStartPosition + _config.BoidcontrollerSpawnDistance,Quaternion.identity).GetComponent<BoidController>();
                _activeBoidControllers[i].transform.position += new Vector3(Mathf.Sin(Mathf.Rad2Deg * i/_activeBoidControllers.Length * Mathf.PI) * 15f,Mathf.Cos(Mathf.Rad2Deg * i/_activeBoidControllers.Length * Mathf.PI) * 15f, 0f);
#if UNITY_EDITOR
                Debug.Log(  "placing boid controller at " + _activeBoidControllers[i].transform.position); 
#endif
                _activeBoidControllers[i].Init(_config._boidAverageAsterboids);
            }
        }
    }
}
