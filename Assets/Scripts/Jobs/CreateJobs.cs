using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace Jobs {
    public static class CreateJobs {
        public static MoveAsteroidsJob CreateMoveAsterboidsJob(float deltaTime, NativeArray<Quaternion> AsterboidRotations) {

            return new MoveAsteroidsJob() {
                _deltaTime = deltaTime,
                _asterboidRotations = AsterboidRotations
            };
        }

        public static UpdateAsterboidInfoJob CreateStoreAsterboidForwardsJob(NativeArray<Vector3> AsterboidPositions,
            NativeArray<Quaternion> AsterboidRotations) {
            return new UpdateAsterboidInfoJob {
                _asterboidPositions = AsterboidPositions,
                _asterboidRotations = AsterboidRotations
            };
        }

        public static TransformJob CreateTransfromJob(NativeArray<Vector3> AsterboidPositions,
            NativeArray<Vector3> AsterboidVelocities,
            NativeArray<Quaternion> AsterboidRotations,
            Vector3 ControllerForward,
            Vector3 ControllerPosition,
            float RotationCoeff,
            float DeltaTime,
            float ControllerNeighbourDist,
            float Speed) {

            return new TransformJob {
                boidPositions = AsterboidPositions,
                boidRotations = AsterboidRotations,
                controllerFoward = ControllerForward,
                boidVelocities = AsterboidVelocities,
                controllerPosition = ControllerPosition,
                rotationCoeff = RotationCoeff,
                deltaTime = DeltaTime,
                neighborDist = ControllerNeighbourDist,
                speed = Speed
            };
        }
    }
}
