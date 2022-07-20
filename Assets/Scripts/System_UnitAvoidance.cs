using Unity.Entities;

namespace DOTS10KUnitDemo
{
    public partial class System_UnitAvoidance : SystemBase
    {
        EntityQuery unitsQuery;

        public SpatialPartitionData spatialPartitionData;

        public void Reset()
        {
            spatialPartitionData        = SpatialPartitionData.Create();
            unitsQuery = GetEntityQuery(ComponentType.ReadOnly<UnitSpatialHash>());
        }

        protected override void OnUpdate()
        {
            spatialPartitionData.Clear();

            this.Dependency = IJobChunk_UpdateSpatialPartitions.ScheduleParallel(unitsQuery, ref spatialPartitionData.spatialPartitionData, this.Dependency);
            this.Dependency = IJobChunk_UpdateAvoidance.ScheduleParallel(unitsQuery, spatialPartitionData.spatialPartitionData, spatialPartitionData.spatialPartitionAdjacencies, this.Dependency);
        }

        protected override void OnDestroy()
        {
            spatialPartitionData.Dispose();
        }
    }
}
