using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs {
    [BurstCompile]

    public struct BoidDirectionJob : IJob
    {
        [ReadOnly] public NativeArray<Vector3> asterboidPositions;
        public NativeArray<Vector3> asterboidVelocities;
        public NativeArray<Quaternion> asterboidRotations;
        
        [ReadOnly] public Vector3 controllerPosition;
        [ReadOnly] public Vector3 controllerForward;
        [ReadOnly] public float controllerNeighbourDist;
        [ReadOnly] public float  _controllerCohesionForce;   
        [ReadOnly] public float _controllerSeparationForce;
        [ReadOnly] public float _controllerAlignmentForce;
     
        [ReadOnly] public float  _deltaTime;   
        [ReadOnly] public float _rotationCoeff;
        [ReadOnly] public float _speed;
  

        public void Execute() {
            for (int i = 0; i < asterboidPositions.Length; i++) {
                
                var noise = Mathf.PerlinNoise(_deltaTime + i + 0.01f, i) * 2.0f - 1.0f;
                var speed = _speed * (1.0f + noise * 0.5f);
                var currentPosition = asterboidPositions[i];
                var currentRotation = asterboidRotations[i];
                
                var separation = Vector3.zero;
                var alignment = Vector3.zero;
                var cohesion = controllerPosition;

                int neighbourCount = 0;
                for(int index = 0; index < asterboidPositions.Length; index++)
                {
                    if (index == i) {
                        neighbourCount++;

                        continue;
                    }
                    if ((asterboidPositions[i] - asterboidPositions[index]).sqrMagnitude <=
                        ((controllerNeighbourDist) * (controllerNeighbourDist))) {
                        var targetPosition = asterboidPositions[index];
                        separation += GetSeparationVector(currentPosition, targetPosition);
                        alignment += asterboidRotations[index] * Vector3.forward ;
                        cohesion += targetPosition;
                        neighbourCount++;

                    }
                }
                var avg = 1.0f / Mathf.Max(1,neighbourCount);
                alignment *= avg;
                cohesion *= avg;
                cohesion = (cohesion - currentPosition).normalized;

                var direction =  alignment + cohesion + separation;
                var accel = alignment * _controllerAlignmentForce + cohesion * _controllerCohesionForce + separation * _controllerSeparationForce;
                var vel = asterboidVelocities[i] + accel * _deltaTime;
                vel = Vector3.ClampMagnitude(vel, speed);
                asterboidVelocities[i] = vel;
                
            

                var rotation = Quaternion.FromToRotation(Vector3.forward, vel.normalized);
               if (rotation != currentRotation)
                {
                    var ip = Mathf.Exp(-_rotationCoeff * _deltaTime);
                    asterboidRotations[i] = Quaternion.Slerp(rotation, currentRotation, ip);
                }
            }
        }
        
        Vector3 GetSeparationVector(Vector3 current, Vector3 targetPos)
        {
            var diff = current - targetPos;
            var diffLen = diff.magnitude;
            var scaler = Mathf.Clamp01(1.0f - diffLen / controllerNeighbourDist);
            return diff * (scaler / diffLen);
        }
    }
}

