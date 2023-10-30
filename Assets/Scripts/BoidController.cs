using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine;
using System.Collections;
using Jobs;
using TreeEditor;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
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
    public NativeArray<Vector3> testVectors;
    public NativeArray<Quaternion> AsterboidRotations;
    public NativeArray<Vector3> AsterboidVectorRotations;
    
    public float BoidSpeed = 5f;

    public NativeArray<Vector3> AsterboidVelocities;
    public float3 ControllerPosition;
    public Transform[] AsterboidsReferences;

    void Start() {
        Init();
    }


    private void Update() {
        ControllerPosition = transform.position;
        
        for (var i = 0; i < spawnCount; i++) {
            AsterboidPositions[i] = AsterboidsReferences[i].transform.position;
            //AsterboidVelocities[i] = AsterboidsReferences[i].transform.forward;
        }
        float delta = Time.deltaTime;
       
        BoidDirectionJob directionJob = CreateBoidDirectionJob(delta);
        JobHandle directionJobHandle = directionJob.Schedule();
        
        MoveAsteroidsJob moveAsteroidsJob = CreateMoveAsteroidJob(delta);
        JobHandle moveAsteroidsJobHandle = moveAsteroidsJob.Schedule(m_AsterboidAccessArray,directionJobHandle);

        UpdateAsterboidInfoJob updateAsterboidInfoJob = CreateStoreAsterboidForwardsJob();
        JobHandle UpdateInfoJobHandle = updateAsterboidInfoJob.Schedule(m_AsterboidAccessArray, JobHandle.CombineDependencies(directionJobHandle,moveAsteroidsJobHandle));

        
        directionJobHandle.Complete();
        moveAsteroidsJobHandle.Complete();
        UpdateInfoJobHandle.Complete();
       
 /*
        TransformJob transformJob = CreateJobs.CreateTransfromJob(AsterboidPositions, AsterboidVelocities,
            AsterboidRotations, transform.forward, ControllerPosition, rotationCoeff, Time.deltaTime, neighborDist, BoidSpeed);
        JobHandle transformJobHandle = transformJob.Schedule(m_AsterboidAccessArray);

        UpdateAsterboidInfoJob updateAsterboidInfoJob =
            CreateJobs.CreateStoreAsterboidForwardsJob(AsterboidPositions, AsterboidRotations);
        JobHandle UpdateInfoJobHandle = updateAsterboidInfoJob.Schedule(m_AsterboidAccessArray, transformJobHandle);

        transformJobHandle.Complete();
        UpdateInfoJobHandle.Complete();
         */

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
        AsterboidVelocities = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        AsterboidRotations = new NativeArray<Quaternion>(spawnCount, Allocator.Persistent);
        AsterboidVectorRotations = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        testVectors = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        for (var i = 0; i < spawnCount; i++) {
            AsterboidsReferences[i] = Spawn().GetComponent<Transform>();
            m_AsterboidAccessArray[i] = AsterboidsReferences[i];

        }
    }

    private void OnDestroy() {
        AsterboidPositions.Dispose();
        AsterboidVelocities.Dispose();
        AsterboidRotations.Dispose();
        AsterboidVectorRotations.Dispose();
        m_AsterboidAccessArray.Dispose();
        testVectors.Dispose();
    }

    private BoidDirectionJob CreateBoidDirectionJob(float deltaTime) {
        return new BoidDirectionJob {
            asterboidPositions = AsterboidPositions,
            asterboidVelocities = AsterboidVelocities,
            asterboidRotations = AsterboidRotations,
            
            controllerPosition = ControllerPosition,
            controllerForward = transform.forward,
            controllerNeighbourDist = neighborDist,
       
            //testVectors = testVectors,
            _rotationCoeff = rotationCoeff,
            _deltaTime = deltaTime,
            _speed = BoidSpeed
        };
    }
    
    private MoveAsteroidsJob CreateMoveAsteroidJob(float deltaTime) {
        return new MoveAsteroidsJob{
            _asterboidPositions = AsterboidPositions,
             _asterboidRotations = AsterboidRotations,
             _asterboidVelocities = AsterboidVelocities,
             _deltaTime = deltaTime
        };
    }
    public UpdateAsterboidInfoJob CreateStoreAsterboidForwardsJob() {
        return new UpdateAsterboidInfoJob {
            _asterboidPositions = AsterboidPositions,
            _asterboidRotations = AsterboidRotations
        };
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
     
