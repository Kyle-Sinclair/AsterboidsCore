using Config;
using GameSystems.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystems {
    
    public class CameraManager : GameService {
        
        private Camera _mainCamera;

        // Start is called before the first frame update

     
        public void Initialize() {
            
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            if (_mainCamera == null) {
                Debug.LogError("Main Camera not found");
            }
            ConfigManager _config =  ServiceLocator.Current.Get<ConfigManager>();
            if (_config == null) {
                Debug.Log("Why null");
            }
           Vector3 what =  _config.GetConfig().CameraDistance;
            //_mainCamera.transform.position =  _config.GetConfig().CameraDistance;
            //_mainCamera.transform.rotation = Quaternion.FromToRotation(  _mainCamera.transform.rotation.eulerAngles,_config.GetConfig().PlayerStartPosition - _mainCamera.transform.position);
            
        }

        // Update is called once per frame
        void CameraUpdate() {

        }

        public override void ConfigureService() {
            Initialize();
        }
    }
}
