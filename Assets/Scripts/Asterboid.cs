using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Asterboid : MonoBehaviour
{
     // Reference to the controller.
    public BoidController Controller;

    private int index;
    // Random seed.
    float noiseOffset;

    private bool IsStruck = false;

    private bool reactivating = false;

    private float reactivationLapse = 0f;
    // Caluculates the separation vector with a target.
    Vector3 GetSeparationVector(Transform target)
    {
        var diff = transform.position - target.transform.position;
        var diffLen = diff.magnitude;
        var scaler = Mathf.Clamp01(1.0f - diffLen / Controller.neighborDist);

        return diff * (scaler / diffLen);
    }

 


    private void OnTriggerEnter(Collider other) {
        //Debug.Log("Bullet struck an asterboid");
        if (IsStruck) return;
        IsStruck = true;
        RenderInactive();
    }

    private void RenderInactive() {
        Controller.RecieveDeathNotifiation();
        MeshRenderer renderers = GetComponentInChildren<MeshRenderer>();
        renderers.enabled = false;
        Collider collider = GetComponentInChildren<Collider>();
        collider.enabled = false;
    }

    public void Reactivate() {
        IsStruck = false;
        MeshRenderer renderers = GetComponentInChildren<MeshRenderer>();
        renderers.enabled = true;
        Collider collider = GetComponentInChildren<Collider>();
        collider.enabled = true;
    }
    
}
