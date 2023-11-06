
using System;
using System.Collections.Generic;
using UnityEngine;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class BoidController : MonoBehaviour {



    public delegate void BoidControllerDied();
    public event BoidControllerDied BoidControllerDiedInfo;
    public Color color;
    [Header("Standard Boid Implementation")]
    public GameObject BoidPrefab;

    private int spawnCount = 10;

    public float spawnRadius = 4.0f;
    public int innerloopBatchCount = 16;

    [Range(0.1f, 20.0f)] public float ControllerVelocity = 6.0f;

    [Range(0.0f, 0.9f)] public float ControllerVelocityVariation = 0.5f;

    [Range(0.1f, 20.0f)] public float rotationCoeff = 4.0f;

    [Range(0.1f, 10.0f)] public float neighborDist = 2.0f;
    [Header("Optimised Boid Implementation")]
    
    [SerializeField] [Range(1f, 50.0f)] public float ControllerCohesionForce;   
    [SerializeField] [Range(1f, 50.0f)] public float ControllerSeparationForce;
    [SerializeField] [Range(1f, 50.0f)] public float ControllerAlignmentForce;
    private int _currentAsterboidCount;
    private int _maxAsterboidCount;
    private bool Initialized = false;

    
    private BoidDirectionParallelJob directionJob;
    private JobHandle directionJobHandle;
        
    private MoveAsteroidsJob moveAsteroidsJob;
    private JobHandle moveAsteroidsJobHandle;
        
    private UpdateAsterboidInfoJob updateAsterboidInfoJob ;
    private JobHandle UpdateInfoJobHandle;
    
    public Queue<int> _indicesToKill;
    public TransformAccessArray m_AsterboidAccessArray;
    public NativeArray<Vector3> AsterboidPositions;
    public NativeArray<Vector3> testVectors;
    public NativeArray<bool> LiveAsterboids;
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

    public void FirstUpdate() {
        KillAsterboids();
    }
    public void EarlyUpdate(float delta) {
        directionJob = CreateBoidParallelDirectionJob(delta);
        directionJobHandle = directionJob.Schedule(spawnCount,innerloopBatchCount);
    }
    public void GameUpdate(float delta) {
        MaintainAsterboids(delta);
    }

    public void LastUpdate() {
        
        moveAsteroidsJobHandle.Complete();
        UpdateInfoJobHandle.Complete();
    }
    
    private void Update() {
        if (!Initialized) return;
        KillAsterboids();
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
        
      
       
        BoidDirectionJob directionJob = CreateBoidDirectionJob(deltaTime);
         directionJobHandle = directionJob.Schedule(); 
        
    
        
        moveAsteroidsJob = CreateMoveAsteroidJob(deltaTime);
        moveAsteroidsJobHandle = moveAsteroidsJob.Schedule(m_AsterboidAccessArray,directionJobHandle);
        
        updateAsterboidInfoJob = CreateStoreAsterboidForwardsJob();
        UpdateInfoJobHandle = updateAsterboidInfoJob.Schedule(m_AsterboidAccessArray, JobHandle.CombineDependencies(directionJobHandle,moveAsteroidsJobHandle));
    
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
        LiveAsterboids = new NativeArray<bool>(spawnCount, Allocator.Persistent);
        testVectors = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        for (var i = 0; i < spawnCount; i++) {
            var spawned = Spawn().GetComponent<Transform>();
            AsterboidTransformReferences[i] = spawned.GetComponent<Transform>();
            m_AsterboidAccessArray[i] = AsterboidTransformReferences[i];
            AsterboidReferences[i] = spawned.GetComponent<Asterboid>();
            LiveAsterboids[i] = true;
            AsterboidReferences[i].Index = i;
        }
        for (var i = 0; i < spawnCount; i++) {
            AsterboidPositions[i] = AsterboidTransformReferences[i].transform.position;
        }

        _indicesToKill = new Queue<int>(spawnCount);
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
            controllerNeighbourDist = neighborDist,
             _controllerCohesionForce = ControllerCohesionForce,   
             _controllerSeparationForce = ControllerSeparationForce,
             _controllerAlignmentForce = ControllerAlignmentForce,
             _liveAsterboids = LiveAsterboids,
            //testVectors = testVectors,
            _rotationCoeff = rotationCoeff,
            _deltaTime = deltaTime,
            _speed = BoidSpeed
        };
    }
    private BoidDirectionParallelJob CreateBoidParallelDirectionJob(float deltaTime) {
        return new BoidDirectionParallelJob() {
            asterboidPositions = AsterboidPositions,
            asterboidVelocities = AsterboidVelocities,
            asterboidRotations = AsterboidRotations,
            
            controllerPosition = ControllerPosition,
            controllerNeighbourDist = neighborDist,
             _controllerCohesionForce = ControllerCohesionForce,   
             _controllerSeparationForce = ControllerSeparationForce,
             _controllerAlignmentForce = ControllerAlignmentForce,
             _liveAsterboids = LiveAsterboids,
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

    void Reactive() {
        for (var i = 0; i < _currentAsterboidCount; i++) {
            if(LiveAsterboids[i])
            AsterboidReferences[i].Reactivate();
        }

    }

    private void KillAsterboids() {
        while (_indicesToKill.Count > 0) {
            int target = _indicesToKill.Dequeue();
            LiveAsterboids[target] = false;

#if UNITY_EDITOR
            Debug.Log("killing asterboid with index " + target + ". Controller has " + _currentAsterboidCount +
                      " remaining.");
#endif

        }

        if (_currentAsterboidCount <= 0) {
            DieAndRespawn();
        }
    }
    public void RecieveDeathNotifiation(int index) {
        if (_currentAsterboidCount > 0) {

            _currentAsterboidCount--;
            _indicesToKill.Enqueue(index);
        }
    }

    private void DieAndRespawn() {
        //Destroy(this);
        int newAsterboidCount = Random.Range(1, _maxAsterboidCount);
        _currentAsterboidCount = newAsterboidCount;
#if UNITY_EDITOR
        Debug.Log("Boid Controller has lost all asterboids and will reactive soon with " + _currentAsterboidCount + " reactivated asterboids");

#endif

        for (var i = 0; i < _currentAsterboidCount; i++) {
            LiveAsterboids[i] = true;
            AsterboidReferences[i].Reactivate();
        }
    }
}



