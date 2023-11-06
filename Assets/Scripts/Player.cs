using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using GameSystems.Services;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {

    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _speed;
    
    private Vector2 _moveInput;

    private Vector3 _target;
    // Start is called before the first frame update
    void Start() {
        Initialize();

    }

    // Update is called once per frame
    void Update() {
        
    }

    private void FixedUpdate() {
        transform.position += new Vector3(_moveInput.x,_moveInput.y,0f) * (Time.fixedDeltaTime * _speed);
    }

    public void Onfire() {

        Instantiate(_bulletPrefab,transform.position,Quaternion.identity);

    }

    public void OnMove(InputAction.CallbackContext context) {
        _moveInput = context.ReadValue<Vector2>();
        

    }

    void Die() {
        
    }

    void Initialize() {
        ConfigScriptable _config = ServiceLocator.Current.Get<ConfigManager>().GetConfig();
        _speed = _config.PlayerSpeed;
        transform.position = _config.PlayerStartPosition;


    }
}
