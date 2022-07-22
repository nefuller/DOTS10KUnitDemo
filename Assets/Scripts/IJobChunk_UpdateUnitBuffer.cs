using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DOTS10KUnitDemo
{
    /// <summary>
    /// Updates compute buffer with unit component data so it can be uploaded to the GPU.
    /// </summary>
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct IJobChunk_UpdateUnitBuffer : IJobChunk
    {
        [ReadOnly] public ComponentTypeHandle<UnitPosition> unitPositionTypeHandle;
        [ReadOnly] public ComponentTypeHandle<UnitRotation> unitRotationTypeHandle;
        [ReadOnly] public ComponentTypeHandle<UnitIndex> unitIndexTypeHandle;

        [NativeDisableParallelForRestriction] public NativeArray<Compute_Unit> unitBufferData;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkCount          = chunk.Count;
            var unitIndexArray      = chunk.GetNativeArray(unitIndexTypeHandle);
            var unitPositionArray   = chunk.GetNativeArray(unitPositionTypeHandle);
            var unitRotationArray   = chunk.GetNativeArray(unitRotationTypeHandle);

            for (int i = 0; i < chunkCount; ++i)
            {
                var unitIndex               = unitIndexArray[i].index;
                var unitPosition            = unitPositionArray[i].position;
                var unitRotation            = unitRotationArray[i].rotation;

                var unit                    = unitBufferData[unitIndex];
                unit.position               = unitPosition;
                unit.rotation               = unitRotation.value;

                unitBufferData[unitIndex]   = unit;
            }
        }

        public static JobHandle ScheduleParallel(EntityQuery query, ref NativeArray<Compute_Unit> unitBufferData, JobHandle inputDeps)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var job = new IJobChunk_UpdateUnitBuffer
            {
                unitPositionTypeHandle  = em.GetComponentTypeHandle<UnitPosition>(true),
                unitRotationTypeHandle  = em.GetComponentTypeHandle<UnitRotation>(true),
                unitIndexTypeHandle     = em.GetComponentTypeHandle<UnitIndex>(true),
                unitBufferData          = unitBufferData
            };

            return job.ScheduleParallel(query, inputDeps);
        }
    }
}
