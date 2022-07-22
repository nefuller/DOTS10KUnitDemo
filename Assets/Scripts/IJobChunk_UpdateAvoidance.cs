using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    /// <summary>
    /// Calculates an avoidance vector for each unit. The avoidance vector incorporates
    /// alignment, cohesion and separation forces common to flocking algorithms. It also
    /// includes a boundary separation force to prevent units from moving outside the
    /// play area.
    /// </summary>
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
            float boundaryDistX;
            float boundaryDistZ;

            float3 finalAlignment;
            float3 finalCohesion;
            float3 finalSeparation;
            float3 finalAvoidance;

            float boundaryDistMax;
            float boundarySeparationRamp;
            float3 finalBoundarySeparation;

            UnitAvoidance unitAvoidance;

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

                // Get the adjacent partitions based on the current units own spatial hash.
                adjacentPartitions          = spatialPartitionAdjacencies[unitSpatialHash];

                for (var adjacentPartitionIndex = 0; adjacentPartitionIndex < 9; ++adjacentPartitionIndex)
                {
                    // If the adjacent partition has a value of -1 it is invalid (outside
                    // the play area) and should be skipped.
                    adjacentHash = adjacentPartitions[adjacentPartitionIndex];
                    if (adjacentHash < 0) continue;

                    // Enumerate on all units in the adjacent partition.
                    enumerator = spatialPartitionData.GetValuesForKey(adjacentPartitions[adjacentPartitionIndex]);
                    while (enumerator.MoveNext())
                    {
                        neighborData            = enumerator.Current;
                        neighborDisplacement    = unitPosition - neighborData.position;
                        neighborDistance        = math.length(neighborDisplacement);

                        // Calculate alignment, cohesion and separation contribution from the
                        // current unit. Alignment and cohesion are only given by units on
                        // the same team while separation comes from units on both teams.

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

                // Calculate boundary separation forces.
                boundarySeparation = float3.zero;
                boundarySeparationInfluencers = 0;
                boundaryDistX = 0f;
                boundaryDistZ = 0f;

                if (unitPosition.x < -boundarySeparationHalfSize.x)
                {
                    boundarySeparation += new float3(1, 0, 0);
                    boundarySeparationInfluencers++;
                    boundaryDistX = math.clamp(math.abs(unitPosition.x) - boundarySeparationHalfSize.x, 0, boundarySeparationHalfSize.x);
                }
                else if (unitPosition.x > boundarySeparationHalfSize.x)
                {
                    boundarySeparation += new float3(-1, 0, 0);
                    boundarySeparationInfluencers++;
                    boundaryDistX = math.clamp(unitPosition.x - boundarySeparationHalfSize.x, 0, boundarySeparationHalfSize.x);
                }

                if (unitPosition.z < -boundarySeparationHalfSize.y)
                {
                    boundarySeparation += new float3(0, 0, 1);
                    boundarySeparationInfluencers++;
                    boundaryDistZ = math.clamp(math.abs(unitPosition.z) - boundarySeparationHalfSize.y, 0, boundarySeparationHalfSize.y);
                }
                else if (unitPosition.z > boundarySeparationHalfSize.y)
                {
                    boundarySeparation += new float3(0, 0, -1);
                    boundarySeparationInfluencers++;
                    boundaryDistZ = math.clamp(unitPosition.z - boundarySeparationHalfSize.y, 0, boundarySeparationHalfSize.y);
                }

                boundaryDistMax         = math.max(boundaryDistZ, boundaryDistX);
                boundarySeparationRamp  = math.remap(0f, boundarySeparationHalfSize.x, 0f, 10000f, boundaryDistMax * boundaryDistMax);

                // Integrate forces into a final avoidance vector.
                finalAlignment          = unitAlignmentInfluencers  == 0 ? float3.zero : math.normalize(unitAlignment / unitAlignmentInfluencers) * alignmentStrength;
                finalCohesion           = unitCohesionInfluencers   == 0 ? float3.zero : math.normalize(unitCohesion / unitCohesionInfluencers) * cohesionStrength;
                finalSeparation         = unitSeparationInfluencers == 0 ? float3.zero : math.normalize(unitSeparation / unitSeparationInfluencers) * separationStrength;
                finalBoundarySeparation = boundarySeparationInfluencers == 0 ? float3.zero : math.normalize(boundarySeparation / boundarySeparationInfluencers) * boundarySeparationStrength * boundarySeparationRamp;

                unitAvoidance           = unitAvoidanceArray[i];
                unitAvoidance.avoidance = math.normalize(finalAlignment + finalCohesion + finalSeparation + finalBoundarySeparation);
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
