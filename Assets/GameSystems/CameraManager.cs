using Config;
using GameSystems.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystems {
    
    public class CameraManager : MonoBehaviour, IGameService {
        
        private Camera _mainCamera;

        // Start is called before the first frame update

     
        public void Initialize() {
            
            

        }

        // Update is called once per frame
        void CameraUpdate() {

        }

        public void ConfigureService() {
            
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            if (_mainCamera == null) {
                Debug.LogError("Main Camera not found");
            }
            ConfigManager _config =  ServiceLocator.Current.Get<ConfigManager>();
            _mainCamera.transform.position =  _config.GetConfig().CameraDistance;
           
         
        }
    }
}
