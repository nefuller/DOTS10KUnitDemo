using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DOTS10KUnitDemo
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct IJobChunk_UpdateSpatialPartitions : IJobChunk
    {
        [ReadOnly] public EntityTypeHandle unitEntityTypeHandle;
        [ReadOnly] public ComponentTypeHandle<UnitPosition> unitPositionTypeHandle;
        [ReadOnly] public ComponentTypeHandle<UnitVelocity> unitVelocityTypeHandle;
        [ReadOnly] public ComponentTypeHandle<UnitSpatialHash> unitSpatialHashTypeHandle;
        [ReadOnly] public ComponentTypeHandle<UnitTeam> unitTeamTypeHandle;

        [WriteOnly] public NativeParallelMultiHashMap<int, PartitionData>.ParallelWriter spatialPartitionsMultiHashMap;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkCount              = chunk.Count;
            var unitEntityArray         = chunk.GetNativeArray(unitEntityTypeHandle);
            var unitPositionArray       = chunk.GetNativeArray(unitPositionTypeHandle);
            var unitVelocityArray       = chunk.GetNativeArray(unitVelocityTypeHandle);
            var unitSpatialHashArray    = chunk.GetNativeArray(unitSpatialHashTypeHandle);
            var unitTeamArray           = chunk.GetNativeArray(unitTeamTypeHandle);

            for (int i = 0; i < chunkCount; ++i)
            {
                var unitEntity          = unitEntityArray[i];
                var unitPosition        = unitPositionArray[i].position;
                var unitVelocity        = unitVelocityArray[i].velocity;
                var unitSpatialHash     = unitSpatialHashArray[i].hash;
                var unitTeam            = unitTeamArray[i].team;

                spatialPartitionsMultiHashMap.Add(unitSpatialHash, new PartitionData() { entity = unitEntity, position = unitPosition, velocity = unitVelocity, team = unitTeam });
            }
        }

        public static JobHandle ScheduleParallel(EntityQuery updateSpatialPartitionsQuery, ref NativeParallelMultiHashMap<int, PartitionData> spatialPartitionsMultiHashMap, JobHandle inputDeps)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var job = new IJobChunk_UpdateSpatialPartitions
            {
                unitEntityTypeHandle            = em.GetEntityTypeHandle(),
                unitPositionTypeHandle          = em.GetComponentTypeHandle<UnitPosition>(true),
                unitVelocityTypeHandle          = em.GetComponentTypeHandle<UnitVelocity>(true),
                unitSpatialHashTypeHandle       = em.GetComponentTypeHandle<UnitSpatialHash>(true),
                unitTeamTypeHandle              = em.GetComponentTypeHandle<UnitTeam>(true),
                spatialPartitionsMultiHashMap   = spatialPartitionsMultiHashMap.AsParallelWriter()
            };

            return job.ScheduleParallel(updateSpatialPartitionsQuery, inputDeps);
        }
    }
}
