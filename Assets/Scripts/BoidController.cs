using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class BoidController : MonoBehaviour {

    [Header("Standard Boid Implementation")]
    public GameObject boidPrefab;

    public int spawnCount = 10;

    public float spawnRadius = 4.0f;

    [Range(0.1f, 20.0f)] public float ControllerVelocity = 6.0f;

    [Range(0.0f, 0.9f)] public float ControllerVelocityVariation = 0.5f;

    [Range(0.1f, 20.0f)] public float rotationCoeff = 4.0f;

    [Range(0.1f, 10.0f)] public float neighborDist = 2.0f;

    public LayerMask searchLayer;

    [Header("Optimised Boid Implementation")]
    public TransformAccessArray m_AsterboidAccessArray;

    public NativeArray<Vector3> AsterboidPositions;
    public NativeArray<Vector3> AsterboidForwards;
    public NativeArray<Vector3> AsterboidDirections;
    public float3 ControllerPosition;
    public Transform[] AsterboidsReferences;

    void Start() {
        Init();
    }


    private void Update() {
        ControllerPosition = transform.position;
        for (var i = 0; i < spawnCount; i++) {
            AsterboidPositions[i] = AsterboidsReferences[i].transform.position;
            AsterboidForwards[i] = AsterboidsReferences[i].transform.forward;
        }

        BoidDirectionJob CalculateAllBoidDirections = CreateBoidDirectionJob();
        JobHandle BoidDirectionJob = CalculateAllBoidDirections.Schedule();
        BoidDirectionJob.Complete();
        
        var job = new VelocityJob()
        {
            deltaTime = Time.deltaTime,
            velocity = AsterboidDirections
        };
        JobHandle jobHandle = job.Schedule(m_AsterboidAccessArray);

        jobHandle.Complete();

        //AsterboidPositions.Dispose();
        //AsterboidForwards.Dispose();
    }

    public GameObject Spawn() {
        return Spawn(transform.position + Random.insideUnitSphere * spawnRadius);
    }

    public GameObject Spawn(Vector3 position) {
        var rotation = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);
        var boid = Instantiate(boidPrefab, position, rotation) as GameObject;
        boid.GetComponent<Asterboid>().controller = this;
        return boid;
    }

    private void Init() {
        ControllerPosition = transform.position;
        AsterboidsReferences = new Transform[spawnCount];
        m_AsterboidAccessArray = new TransformAccessArray(AsterboidsReferences);
        AsterboidPositions = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        AsterboidForwards = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        AsterboidDirections = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        for (var i = 0; i < spawnCount; i++) {
            AsterboidsReferences[i] = Spawn().GetComponent<Transform>();
            m_AsterboidAccessArray[i] = AsterboidsReferences[i];

        }
    }

    private void OnDestroy() {
        AsterboidPositions.Dispose();
        AsterboidForwards.Dispose();
        AsterboidDirections.Dispose();
        m_AsterboidAccessArray.Dispose();
    }

    private BoidDirectionJob CreateBoidDirectionJob() {
        return new BoidDirectionJob {
            controllerPosition = ControllerPosition,
            controllerRotation = transform.rotation.eulerAngles,
            controllerForward = transform.forward,
            controllerVelocity = ControllerVelocity,
            controllerVelocityVariation = ControllerVelocity,
            controllerNeighbourDist = neighborDist,
            asterboidPositions = AsterboidPositions,
            asterboidForwards = AsterboidForwards,
            asterboidDirections = AsterboidDirections
        };
    }

 
        public struct VelocityJob : IJobParallelForTransform {
            // Jobs declare all data that will be accessed in the job
            // By declaring it as read only, multiple jobs are allowed to access the data in parallel
            [ReadOnly] public NativeArray<Vector3> velocity;

            // Delta time must be copied to the job since jobs generally don't have a concept of a frame.
            // The main thread waits for the job same frame or next frame, but the job should do work deterministically
            // independent on when the job happens to run on the worker threads.
            public float deltaTime;

            // The code actually running on the job
            public void Execute(int index, TransformAccess transform) {
                // Move the transforms based on delta time and velocity
                var pos = transform.position;
                var rotation = Quaternion.FromToRotation(Vector3.forward,velocity[index].normalized);
                var ip = Mathf.Exp(- 4.0f * deltaTime);
                transform.rotation = Quaternion.Slerp(rotation, transform.rotation, ip);
                
                transform.position =  pos +  transform.rotation * Vector3.forward * (5f * deltaTime);
            }
        }
    }


/*
 *var currentPosition = transform.position;
    var currentRotation = transform.rotation;

    // Current ControllerVelocity randomized with noise.
    var noise = Mathf.PerlinNoise(Time.time, noiseOffset) * 2.0f - 1.0f;
    var ControllerVelocity = controller.ControllerVelocity * (1.0f + noise * controller.ControllerVelocityVariation);

    // Initializes the vectors.
    var separation = Vector3.zero;
    var alignment = controller.transform.forward;
    var cohesion = controller.transform.position;

    // Looks up nearby boids.
    var nearbyBoids = Physics.OverlapSphere(currentPosition, controller.neighborDist, 1 << 6);

    // Accumulates the vectors.
    foreach (var boid in nearbyBoids)
    {
        if (boid.gameObject == gameObject) continue;
        var t = boid.transform;
        separation += GetSeparationVector(t);

        alignment += t.forward;
        cohesion += t.position;
    }

    var avg = 1.0f / nearbyBoids.Length;
    alignment *= avg;
    cohesion *= avg;
    cohesion = (cohesion - currentPosition).normalized;

    // Calculates a rotation from the vectors.
    var direction = separation + alignment + cohesion;

    var rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);
   // Debug.Log( separation );

    //Debug.Log(alignment);
    //Debug.Log(cohesion);
    //Debug.Log(rotation);

    // Applys the rotation with interpolation.
    if (rotation != currentRotation)
    {
        var ip = Mathf.Exp(-controller.rotationCoeff * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(rotation, currentRotation, ip);
    }

    // Moves forawrd.
    transform.position = currentPosition + transform.forward * (ControllerVelocity * Time.deltaTime);
 * */
     
