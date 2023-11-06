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
    }
}
