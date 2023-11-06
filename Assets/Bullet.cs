using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        transform.position += transform.forward * (Time.deltaTime * 5f);

        if (transform.position.z > 50f) {
            Destroy(this);
        }
    }
}
