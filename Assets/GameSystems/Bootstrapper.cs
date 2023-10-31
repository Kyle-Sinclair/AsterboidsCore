using System;
using Config;
using GameSystems.Services;
using UnityEngine;

namespace GameSystems {
    public class Bootstrapper : MonoBehaviour {

        [Header("Prefabs")] 
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private BoidController _boidControllerPrefab;

        [Header("Services")] 
        private CameraManager _cameraManager;
        private ConfigManager _configManager;
        private AsterboidManager _asterboidManager;


        private void Start() {
            Init();
        }


        void Init() {
            CreateServices();
            RegisterServices();
            ConfigureServices();
            
        }

        private void CreateServices() {
            _cameraManager = new CameraManager();
            _configManager = new ConfigManager();
            _asterboidManager = new AsterboidManager();
        }

        private void RegisterServices() {
            ServiceLocator.Current.Register(_configManager);

            ServiceLocator.Current.Register(_cameraManager);
            ServiceLocator.Current.Register(_asterboidManager);
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
        internal class AsterboidManager : GameService {
            public override void ConfigureService() {

            }
        }

       
    }
}

