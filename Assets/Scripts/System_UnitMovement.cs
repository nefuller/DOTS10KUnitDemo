using Unity.Entities;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    /// <summary>
    /// Updates position, rotation, velocity and spatial hash of units.
    /// </summary>
    [UpdateAfter(typeof(System_UnitAvoidance))]
    public partial class System_UnitMovement : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime                   = Time.DeltaTime;

            var worldSize                   = GameSettings.WorldSize;
            var worldBounds                 = GameSettings.WorldBounds;

            var unitSpeed                   = GameSettings.UnitSpeed;
            var unitTurnSpeed               = GameSettings.UnitTurnSpeed;
            var unitSteerMinMax             = GameSettings.UnitSteerMinMax;

            var spatialPartitionsPerAxis    = GameSettings.SpatialPartitionsPerAxis;

            Dependency = Entities.WithName("System_UnitMovement")
                .WithAll<UnitTag>()
                .ForEach((ref UnitPosition unitPosition, ref UnitRotation unitRotation, ref UnitVelocity unitVelocity, ref UnitSpatialHash unitSpatialHash, in UnitRNG unitRng, in UnitAvoidance unitAvoidance) =>
                {
                    var forward = float3.zero;
                    if (math.lengthsq(unitAvoidance.avoidance) > 0.001f)
                    {
                        // This unit has an avoidance force acting on it, so calcuate a new
                        // forward vector and rotation based on it.
                        forward = math.normalize(unitVelocity.velocity);
                        forward = math.lerp(forward, unitAvoidance.avoidance, unitTurnSpeed * deltaTime);
       
                        unitRotation.rotation = quaternion.LookRotation(forward, new float3(0f, 1f, 0f));
                    }
                    else
                    {
                        // This unit has does not have an avoidance force acting on it. Calcuate a new
                        // forward vector and rotation based on some random values so the units
                        // wander a little when off by themselves.
                        forward = math.mul(math.mul(unitRotation.rotation, quaternion.Euler(0f, unitRng.rng.NextFloat(-unitSteerMinMax, unitSteerMinMax), 0f)), new float3(0f, 0f, 1f));

                        unitRotation.rotation = quaternion.LookRotation(forward, new float3(0f, 1f, 0f));
                    }

                    // Update velocity and position of the unit.
                    unitVelocity.velocity = forward * unitSpeed;
                    unitPosition.position += unitVelocity.velocity * deltaTime;

                    // Ensure the unit remains inside the spatial partitioning region, regardless of
                    // whatever other forces are acting on it.
                    if (unitPosition.position.x < -worldBounds.halfSize.x)
                    {
                        unitPosition.position.x = -worldBounds.halfSize.x;
                    }
                    else if (unitPosition.position.x > worldBounds.halfSize.x)
                    {
                        unitPosition.position.x = worldBounds.halfSize.x;
                    }

                    if (unitPosition.position.z < -worldBounds.halfSize.y)
                    {
                        unitPosition.position.z = -worldBounds.halfSize.y;
                    }
                    else if (unitPosition.position.z > worldBounds.halfSize.y)
                    {
                        unitPosition.position.z = worldBounds.halfSize.y;
                    }

                    // Update the spatial hash for the unit.
                    unitSpatialHash.hash = SpatialPartition.GridHash(in unitPosition.position, in worldSize, spatialPartitionsPerAxis);
                }).ScheduleParallel(Dependency);
        }
    }
}