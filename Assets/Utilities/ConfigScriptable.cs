using UnityEngine;

namespace Config {

    [CreateAssetMenu(fileName = "Config File", menuName = "Configuration/Config File", order = 2)]

    public class ConfigScriptable : ScriptableObject {

        [Header("Camera Configuration")]
        [SerializeField] public Vector3 PlayerStartPosition;
        [SerializeField] public Vector3 BoidcontrollerSpawnDistance;
        [SerializeField] public Vector3 CameraDistance;
        
        [Header("Player Configuration")]
        [SerializeField]  [Range(5f,10f)]public float PlayerSpeed;

        [Header("Boid Configuration")]

        [SerializeField]    [Range(15f, 50f)] public float _boidSpawnDistance;
        [SerializeField]    [Range(10,10000)] public int _boidAverageAsterboids;
        [SerializeField]    [Range(1,5)] public int _boidControllerAverageCount;
        [SerializeField]    [Range(0.1f, 10.0f)] public float _boidControllerAverageSpiralTightness;
        
        [Header("Prefabs")]
        [SerializeField] public GameObject BoidControllerPrefab;
    }
}
