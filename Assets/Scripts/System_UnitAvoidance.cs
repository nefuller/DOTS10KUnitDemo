using Unity.Entities;

namespace DOTS10KUnitDemo
{
    /// <summary>
    /// Updates spatial partitioning before calculating the avoidance force
    /// that should be applied to units.
    /// </summary>
    public partial class System_UnitAvoidance : SystemBase
    {
        public SpatialPartitionData spatialPartitionData;

        EntityQuery unitsQuery;

        public void Reset()
        {
            spatialPartitionData    = SpatialPartitionData.Create();
            unitsQuery              = GetEntityQuery(ComponentType.ReadOnly<UnitSpatialHash>());
        }

        protected override void OnUpdate()
        {
            // Clear spatial partition data before recalculating.
            spatialPartitionData.Clear();

            // Calculate spatial partitions, and then calculate unit avoidance forces.
            this.Dependency = IJobChunk_UpdateSpatialPartitions.ScheduleParallel(unitsQuery, ref spatialPartitionData.spatialPartitionData, this.Dependency);
            this.Dependency = IJobChunk_UpdateAvoidance.ScheduleParallel(unitsQuery, spatialPartitionData.spatialPartitionData, spatialPartitionData.spatialPartitionAdjacencies, this.Dependency);
        }

        protected override void OnDestroy()
        {
            spatialPartitionData.Dispose();
        }
    }
}
