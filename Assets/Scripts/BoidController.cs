
using System;
using System.Collections.Generic;
using UnityEngine;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public abstract class BoidController : MonoBehaviour {



    public abstract Delegate OnDeath();
    [Header("Standard Boid Implementation")]
    public GameObject BoidPrefab;

    public int spawnCount = 10;

    public float spawnRadius = 4.0f;

    [Range(0.1f, 20.0f)] public float ControllerVelocity = 6.0f;

    [Range(0.0f, 0.9f)] public float ControllerVelocityVariation = 0.5f;

    [Range(0.1f, 20.0f)] public float rotationCoeff = 4.0f;

    [Range(0.1f, 10.0f)] public float neighborDist = 2.0f;
    [Header("Optimised Boid Implementation")]
    private int _currentAsterboidCount;
    private int _maxAsterboidCount;
    private bool Initialized = false;

    public TransformAccessArray m_AsterboidAccessArray;
    public NativeArray<Vector3> AsterboidPositions;
    public NativeArray<Vector3> testVectors;
    public NativeArray<Quaternion> AsterboidRotations;
    public NativeArray<Vector3> AsterboidVectorRotations;
    public NativeArray<Vector3> AsterboidVelocities;
    public Transform[] AsterboidTransformReferences;

    public float BoidSpeed = 5f;

    public float3 ControllerPosition;
    public Asterboid[] AsterboidReferences;

    
    [Header("Boid Movement")]
    private Vector3 _target = new Vector3();
    private Vector3 _velocity = new Vector3();

   


    private void Update() {
        if (!Initialized) return;
        float delta = Time.deltaTime;
        CalculateDirectionVector();
        Move(delta);


        MaintainAsterboids(delta);
    }

    private void Move(float deltaTime) {
        transform.position += _velocity * deltaTime;

    }

    private void CalculateDirectionVector() {
        Vector2 random = Random.insideUnitCircle;
        _target = new Vector3(random.x, random.y,
            0).normalized;
        _velocity += _target;
        _velocity = Vector3.ClampMagnitude(_velocity, 3f);
    }


    void MaintainAsterboids(float deltaTime) {
        ControllerPosition = transform.position;
        
        for (var i = 0; i < spawnCount; i++) {
            AsterboidPositions[i] = AsterboidTransformReferences[i].transform.position;
        }
       
        BoidDirectionJob directionJob = CreateBoidDirectionJob(deltaTime);
        JobHandle directionJobHandle = directionJob.Schedule();
        
        MoveAsteroidsJob moveAsteroidsJob = CreateMoveAsteroidJob(deltaTime);
        JobHandle moveAsteroidsJobHandle = moveAsteroidsJob.Schedule(m_AsterboidAccessArray,directionJobHandle);
        
        UpdateAsterboidInfoJob updateAsterboidInfoJob = CreateStoreAsterboidForwardsJob();
        JobHandle UpdateInfoJobHandle = updateAsterboidInfoJob.Schedule(m_AsterboidAccessArray, JobHandle.CombineDependencies(directionJobHandle,moveAsteroidsJobHandle));
        directionJobHandle.Complete();
        moveAsteroidsJobHandle.Complete();
        UpdateInfoJobHandle.Complete();
    }

    void CleanOutDeadAsterboids() {
        //m_AsterboidAccessArray.RemoveAtSwapBack(index);        
    }

    public GameObject Spawn() {
        return Spawn(transform.position + Random.insideUnitSphere * spawnRadius);
    }

    public GameObject Spawn(Vector3 position) {
        var rotation = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);
        var boid = Instantiate(BoidPrefab, position, rotation) as GameObject;
        boid.GetComponent<Asterboid>().Controller = this;
        return boid;
    }

    public void Init(int SpawnCount) {
        ControllerPosition = transform.position;
        spawnCount = SpawnCount;
        _currentAsterboidCount = spawnCount;
        _maxAsterboidCount = spawnCount;
        AsterboidTransformReferences = new Transform[spawnCount];
        AsterboidReferences = new Asterboid[spawnCount];
        m_AsterboidAccessArray = new TransformAccessArray(AsterboidTransformReferences);
        AsterboidPositions = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        AsterboidVelocities = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        AsterboidRotations = new NativeArray<Quaternion>(spawnCount, Allocator.Persistent);
        AsterboidVectorRotations = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        testVectors = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        for (var i = 0; i < spawnCount; i++) {
            var spawned = Spawn().GetComponent<Transform>();
            AsterboidTransformReferences[i] = spawned.GetComponent<Transform>();
            m_AsterboidAccessArray[i] = AsterboidTransformReferences[i];
            AsterboidReferences[i] = spawned.GetComponent<Asterboid>();
        }

        Initialized = true;
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

    public void RecieveDeathNotifiation() {
        _currentAsterboidCount--;
        if (_currentAsterboidCount <= 0) {
            DieAndRespawn();
        }
    }

    private void DieAndRespawn() {
        int newAsterboidCount = Random.Range(1, _maxAsterboidCount);
        _currentAsterboidCount = newAsterboidCount;
        for (var i = 0; i < spawnCount; i++) {
        
            AsterboidReferences[i].Reactivate();
        }
        Debug.Log("Boid Controller has lost all asterboids and will reactive soon");
    }
}



