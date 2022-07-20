using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct PartitionData
    {
        public Entity entity;
        public float3 position;
        public float3 velocity;
        public Team team;
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct IJobChunk_UpdateAvoidance : IJobChunk
    {
        [ReadOnly] public NativeParallelMultiHashMap<int, PartitionData> spatialPartitionData;
        [ReadOnly] public NativeArray<FixedList64Bytes<int>> spatialPartitionAdjacencies;

        [ReadOnly] public ComponentTypeHandle<UnitPosition> unitPositionTypeHandle;
        [ReadOnly] public ComponentTypeHandle<UnitSpatialHash> unitSpatialHashTypeHandle;
        [ReadOnly] public ComponentTypeHandle<UnitTeam> unitTeamTypeHandle;

        [ReadOnly] public float alignmentRange;
        [ReadOnly] public float cohesionRange;
        [ReadOnly] public float separationRange;
        [ReadOnly] public float alignmentStrength;
        [ReadOnly] public float cohesionStrength;
        [ReadOnly] public float separationStrength;

        [ReadOnly] public float2 worldHalfSize;

        [ReadOnly] public float2 boundarySeparationHalfSize;
        [ReadOnly] public float boundarySeparationStrength;

        [NativeDisableParallelForRestriction] public ComponentTypeHandle<UnitAvoidance> unitAvoidanceTypeHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkCount              = chunk.Count;
            var unitPositionArray       = chunk.GetNativeArray(unitPositionTypeHandle);
            var unitSpatialHashArray    = chunk.GetNativeArray(unitSpatialHashTypeHandle);
            var unitAvoidanceArray      = chunk.GetNativeArray(unitAvoidanceTypeHandle);
            var unitTeamArray           = chunk.GetNativeArray(unitTeamTypeHandle);

            PartitionData neighborData;
            float3 unitPosition;
            int unitSpatialHash;
            float3 unitAlignment;
            int unitAlignmentInfluencers;
            float3 unitCohesion;
            int unitCohesionInfluencers;
            float3 unitSeparation;
            int unitSeparationInfluencers;
            Team unitTeam;

            NativeParallelMultiHashMap<int, PartitionData>.Enumerator enumerator;
            FixedList64Bytes<int> adjacentPartitions;
            int adjacentHash;

            float3 neighborDisplacement;
            float neighborDistance;

            float3 boundarySeparation;
            int boundarySeparationInfluencers;

            float3 finalAlignment;
            float3 finalCohesion;
            float3 finalSeparation;
            float3 finalAvoidance;

            for (int i = 0; i < chunkCount; ++i)
            {
                unitPosition                = unitPositionArray[i].position;
                unitSpatialHash             = unitSpatialHashArray[i].hash;
                unitTeam                    = unitTeamArray[i].team;
                unitAlignment               = float3.zero;
                unitAlignmentInfluencers    = 0;
                unitCohesion                = float3.zero;
                unitCohesionInfluencers     = 0;
                unitSeparation              = float3.zero;
                unitSeparationInfluencers   = 0;

                adjacentPartitions          = spatialPartitionAdjacencies[unitSpatialHash];

                for (var adjacentPartitionIndex = 0; adjacentPartitionIndex < 9; ++adjacentPartitionIndex)
                {
                    adjacentHash = adjacentPartitions[adjacentPartitionIndex];
                    if (adjacentHash < 0) continue;

                    enumerator = spatialPartitionData.GetValuesForKey(adjacentPartitions[adjacentPartitionIndex]);
                    while (enumerator.MoveNext())
                    {
                        neighborData            = enumerator.Current;
                        neighborDisplacement    = unitPosition - neighborData.position;
                        neighborDistance        = math.length(neighborDisplacement);

                        if (neighborDistance > 0f && neighborDistance < alignmentRange && neighborData.team == unitTeam)
                        {
                            unitAlignment += neighborData.velocity;
                            unitAlignmentInfluencers++;
                        }

                        if (neighborDistance > 0f && neighborDistance < cohesionRange && neighborData.team == unitTeam)
                        {
                            unitCohesion += neighborData.position;
                            unitCohesionInfluencers++;
                        }

                        if (neighborDistance > 0f && neighborDistance < separationRange)
                        {
                            unitSeparation += neighborDisplacement;
                            unitSeparationInfluencers++;
                        }
                    }
                }

                boundarySeparation = float3.zero;
                boundarySeparationInfluencers = 0;
                var topDistance = 0f;
                var bottomDistance = 0f;

                if (unitPosition.x < -boundarySeparationHalfSize.x)
                {
                    boundarySeparation += new float3(1, 0, 0);
                    boundarySeparationInfluencers++;
                    topDistance = math.clamp(math.abs(unitPosition.x) - boundarySeparationHalfSize.x, 0, boundarySeparationHalfSize.x);
                }
                else if (unitPosition.x > boundarySeparationHalfSize.x)
                {
                    boundarySeparation += new float3(-1, 0, 0);
                    boundarySeparationInfluencers++;
                    topDistance = math.clamp(unitPosition.x - boundarySeparationHalfSize.x, 0, boundarySeparationHalfSize.x);
                }

                if (unitPosition.z < -boundarySeparationHalfSize.y)
                {
                    boundarySeparation += new float3(0, 0, 1);
                    boundarySeparationInfluencers++;
                    bottomDistance = math.clamp(math.abs(unitPosition.z) - boundarySeparationHalfSize.y, 0, boundarySeparationHalfSize.y);
                }
                else if (unitPosition.z > boundarySeparationHalfSize.y)
                {
                    boundarySeparation += new float3(0, 0, -1);
                    boundarySeparationInfluencers++;
                    bottomDistance = math.clamp(unitPosition.z - boundarySeparationHalfSize.y, 0, boundarySeparationHalfSize.y);
                }

             
                var normalizedDistance = math.remap(0, boundarySeparationHalfSize.x, 0f, 1f, math.max(bottomDistance, topDistance));
                var inf = math.remap(0f, 1f, 0f, 10000f, 1f * normalizedDistance * normalizedDistance);

                var boundaryAvoidance = boundarySeparationInfluencers == 0 ? float3.zero: math.normalize(boundarySeparation / boundarySeparationInfluencers) * boundarySeparationStrength * inf;
          
                finalAlignment      = unitAlignmentInfluencers  == 0 ? float3.zero : math.normalize(unitAlignment / unitAlignmentInfluencers) * alignmentStrength;
                finalCohesion       = unitCohesionInfluencers   == 0 ? float3.zero : math.normalize(unitCohesion / unitCohesionInfluencers) * cohesionStrength;
                finalSeparation     = unitSeparationInfluencers == 0 ? float3.zero : math.normalize(unitSeparation / unitSeparationInfluencers) * separationStrength;

                finalAvoidance      = math.normalize(finalAlignment + finalCohesion + finalSeparation + boundaryAvoidance);


                var unitAvoidance       = unitAvoidanceArray[i];
                unitAvoidance.avoidance = finalAvoidance;
                unitAvoidanceArray[i]   = unitAvoidance;
            }
        }

        public static JobHandle ScheduleParallel(EntityQuery updateAvoidanceQuery, NativeParallelMultiHashMap<int, PartitionData> spatialPartitionData, NativeArray<FixedList64Bytes<int>> spatialPartitionAdjacencies, JobHandle inputDeps)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var job = new IJobChunk_UpdateAvoidance
            {
                unitPositionTypeHandle          = em.GetComponentTypeHandle<UnitPosition>(true),
                unitSpatialHashTypeHandle       = em.GetComponentTypeHandle<UnitSpatialHash>(true),
                unitTeamTypeHandle              = em.GetComponentTypeHandle<UnitTeam>(true),
                unitAvoidanceTypeHandle         = em.GetComponentTypeHandle<UnitAvoidance>(false),
                spatialPartitionData            = spatialPartitionData,
                spatialPartitionAdjacencies     = spatialPartitionAdjacencies,
                alignmentRange                  = GameSettings.AlignmentRange,
                cohesionRange                   = GameSettings.CohesionRange,
                separationRange                 = GameSettings.SeparationRange,
                alignmentStrength               = GameSettings.AlignmentStrength,
                cohesionStrength                = GameSettings.CohesionStrength,
                separationStrength              = GameSettings.SeparationStrength,
                worldHalfSize                   = GameSettings.WorldSize.halfSize,
                boundarySeparationHalfSize      = GameSettings.WorldBounds.separationHalfSize,
                boundarySeparationStrength      = GameSettings.BoundarySeparationStrength
            };

            return job.ScheduleParallel(updateAvoidanceQuery, inputDeps);
        }
    }
}
