using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystems.Services {

    public class ServiceLocator {
        
        private ServiceLocator() { }
        
        //Static method used for any object to get a service if needed
        public static ServiceLocator Current { get; private set; }

        [SerializeField]
        private readonly Dictionary<string, GameService> _services = new Dictionary<string, GameService>();

        public T Get<T>() where T : GameService
        {
            string key = typeof(T).Name;
            if (!_services.ContainsKey(key))
            {
                Debug.LogError($"{key} not registered with {GetType().Name}");
                throw new InvalidOperationException();
            }

            return (T)_services[key];
        }
        
        public void Register<T>(T service) where T : GameService
        {
            string key = typeof(T).Name;
            if (_services.ContainsKey(key))
            {
                Debug.LogError($"Attempted to register service of type {key} which is already registered with the {GetType().Name}.");
                return;
            }
#if UNITY_EDITOR
            Debug.Log($"Added a service of type {key}");
#endif
            _services.Add(key, service);
        }

        /// <summary>
        /// Unregisters the service from the current service locator.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        public void Unregister<T>() where T : GameService
        {
            string key = typeof(T).Name;
            if (!_services.ContainsKey(key))
            {
                Debug.LogError($"Attempted to unregister service of type {key} which is not registered with the {GetType().Name}.");
                return;
            }

            _services.Remove(key);
        }

        public void ConfigureServices() {
            foreach (var VARIABLE in _services) {
                Debug.Log("configuring" + VARIABLE);
                VARIABLE.Value.ConfigureService();
            }
        }
        public static void Initialize()
        {
            Current = new ServiceLocator();
        }
    }

}
