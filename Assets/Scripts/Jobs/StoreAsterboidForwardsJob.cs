using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Jobs {
    public struct UpdateAsterboidInfoJob : IJobParallelForTransform {
        
        [WriteOnly] public NativeArray<Vector3> _asterboidPositions;
        [WriteOnly] public NativeArray<Quaternion> _asterboidRotations;
        public void Execute(int index, TransformAccess transform) {
            _asterboidRotations[index] = transform.rotation;
            _asterboidPositions[index] = transform.position;
        }
    }
}
