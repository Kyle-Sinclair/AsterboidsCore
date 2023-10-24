using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
[BurstCompile]

public struct BoidDirectionJob : IJob
{
      
        [ReadOnly] public Vector3 controllerPosition;
        [ReadOnly] public Vector3 controllerRotation;
        [ReadOnly] public Vector3 controllerForward;
        [ReadOnly] public float controllerVelocity;
        [ReadOnly] public float controllerVelocityVariation;
        [ReadOnly] public float controllerNeighbourDist;
        
        [ReadOnly] public NativeArray<Vector3> asterboidPositions;
        [ReadOnly] public NativeArray<Vector3> asterboidForwards;
        [WriteOnly] public NativeArray<Vector3> asterboidDirections;

        public void Execute() {
            Vector3 t = new Vector3(0f,0f,0f);
            int asterboidCount = asterboidPositions.Length;
            for (int i = 0; i < asterboidCount; i++) {
                
                var currentPosition = asterboidPositions[i];
                var velocity = controllerVelocity * (1.0f * controllerVelocityVariation);
                var separation = Vector3.zero;
                var alignment = controllerRotation;
                Vector3 cohesion = controllerPosition;
                
                for (int j = 0; j < asterboidCount; j++) {
                    if (i == j) {
                        continue;
                        
                    }
                    t = asterboidPositions[j];
                    alignment += asterboidForwards[j];
                    cohesion += t;
                    var diff = currentPosition - t;
                    var diffLen = diff.magnitude;
                    var scaler = Mathf.Clamp01(1.0f - diffLen / controllerNeighbourDist);
                    separation += diff * (scaler / diffLen);
                }
                var avg = 1.0f / asterboidCount;
                alignment *= avg;
                cohesion *= avg;
                cohesion = (cohesion - asterboidPositions[i]).normalized;
                var direction = separation + alignment + cohesion;

                asterboidDirections[i] = direction;
            }
        }
    }

