using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    /// <summary>
    /// Calculates adjacencies for each spatial partition.
    /// </summary>
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct IJobParallelForBatch_PrecalculateSpatialPartitionAdjacencies : IJobParallelForBatch
    {
        [ReadOnly] public int spatialPartitionPerAxis;
        [ReadOnly] public int spatialPartitionDirectionsLength;
        [ReadOnly] public NativeArray<int2> spatialPartitionDirections;

        [NativeDisableParallelForRestriction] public NativeArray<FixedList64Bytes<int>> spatialPartitionAdjacencies;

        public void Execute(int startIndex, int count)
        {
            for (var i = 0; i < count; ++i)
            {
                var spatialHash         = startIndex + i;
                var spatialHashCoord    = new int2(
                    spatialHash % spatialPartitionPerAxis,
                    spatialHash / spatialPartitionPerAxis);

                var adjacencies         = spatialPartitionAdjacencies[spatialHash];

                for (var directionIndex = 0; directionIndex < spatialPartitionDirectionsLength; ++directionIndex)
                {
                    // Adjacencies that fall outside the spatial partitioning region are indicated by '-1'.
                    int adjacencyHash   = -1;

                    var neighborSpatialPartition    = spatialHashCoord + spatialPartitionDirections[directionIndex];
                    if (neighborSpatialPartition.x >= 0 && neighborSpatialPartition.x < spatialPartitionPerAxis &&
                        neighborSpatialPartition.y >= 0 && neighborSpatialPartition.y < spatialPartitionPerAxis)
                    {
                        adjacencyHash   = neighborSpatialPartition.y * spatialPartitionPerAxis + neighborSpatialPartition.x;
                    }

                    adjacencies.Add(adjacencyHash);
                }

                spatialPartitionAdjacencies[spatialHash] = adjacencies;
            }
        }

        public static JobHandle ScheduleBatch(NativeArray<int2> spatialPartitionDirections, NativeArray<FixedList64Bytes<int>> spatialPartitionAdjacencies, JobHandle inputDeps)
        {
            var job = new IJobParallelForBatch_PrecalculateSpatialPartitionAdjacencies
            {

                spatialPartitionPerAxis             = GameSettings.SpatialPartitionsPerAxis,
                spatialPartitionDirectionsLength    = GameSettings.SpatialPartitionDirectionsLength,
                spatialPartitionDirections          = spatialPartitionDirections,
                spatialPartitionAdjacencies         = spatialPartitionAdjacencies
            };

            return job.ScheduleBatch(GameSettings.SpatialPartitionsPerAxis * GameSettings.SpatialPartitionsPerAxis, GameSettings.SpatialPartitionsPerAxis, inputDeps);
        }
    }
}
