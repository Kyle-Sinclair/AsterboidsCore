using System;
using Config;
using GameSystems.Services;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameSystems {
    public class Bootstrapper : MonoBehaviour {

        [Header("Prefabs")] 
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private BoidController _boidControllerPrefab;

       
        [Header("Service Prefabs")] 
        [SerializeField] private GameObject _cameraManager;
        [SerializeField] private GameObject _asterboidManager;

        
        [Header("Services")] 
        private ConfigManager _configManager;
        private CameraManager _cameraManagerRef;
        private AsterboidsManager _asterboidManagerRef;


        private void Start() {
            Init();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Application.Quit();
            }


        }
        void Init() {
            CreateServices();
            RegisterServices();
            ConfigureServices();
            Instantiate(_playerPrefab);
        }

        

        private void CreateServices() {
            _configManager = new ConfigManager();
            _cameraManagerRef =  Instantiate(_cameraManager,this.transform).GetComponent<CameraManager>();
            _asterboidManagerRef = Instantiate(_asterboidManager,this.transform).GetComponent<AsterboidsManager>();
        }

        private void RegisterServices() {
            ServiceLocator.Current.Register(_configManager);
            ServiceLocator.Current.Register(_cameraManagerRef);
            ServiceLocator.Current.Register(_asterboidManagerRef);
        }
        private void ConfigureServices() {
            ServiceLocator.Current.ConfigureServices();
        }
       
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]

        public static void InitializeServiceLocator() {
            ServiceLocator.Initialize();
            if (ServiceLocator.Current == null) {
                Debug.LogError($"Service Locator failed to initialize");
                return;
            }

        }
  

       
    }
}

