using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Jobs {
    public struct MoveAsteroidsJob : IJobParallelForTransform {

        [ReadOnly] public NativeArray<Vector3> _asterboidPositions;

        [ReadOnly] public NativeArray<Quaternion> _asterboidRotations;
        [ReadOnly] public NativeArray<Vector3> _asterboidVelocities;
        [ReadOnly] public float _deltaTime;

        // The code actually running on the job
        public void Execute(int index, TransformAccess transform) {
            //Dictionary<int, Transform> colliderdictionary;
            transform.rotation =  _asterboidRotations[index];
            transform.position = _asterboidPositions[index] + _asterboidVelocities[index] *  _deltaTime;
        }
    }
}


/*BeginOfFrame
 struct (indexedCollider)
 Vector3 position;
 bucketIndex;
 NativeArray<indexedCollider> MappedColliderPositions;
  NativeArray<indexedCollider> 
 Foreachbucket(int index){
 ColliderPositions.Add(BucketContents,index)
 
 IJobParallelFor(MappedColliderPositions){
    
 }
*/