using UnityEngine;

namespace Config {

    [CreateAssetMenu(fileName = "Config File", menuName = "Configuration/Config File", order = 2)]

    public class ConfigScriptable : ScriptableObject {


        [SerializeField] public Vector3 PlayerStartPosition;
        [SerializeField] public Vector3 CameraDistance;
    }
}
