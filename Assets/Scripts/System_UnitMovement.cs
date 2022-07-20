using Unity.Entities;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    [UpdateAfter(typeof(System_UnitAvoidance))]
    public partial class System_UnitMovement : SystemBase
    {
        public void Reset()
        {
        }

        protected override void OnUpdate()
        {
            var deltaTime                   = Time.DeltaTime;

            var worldSize                   = GameSettings.WorldSize;
            var worldBounds                 = GameSettings.WorldBounds;

            var unitTurnSpeed               = GameSettings.UnitTurnSpeed;
            var unitSteerMinMax             = GameSettings.UnitSteerMinMax;

            var spatialPartitionsPerAxis    = GameSettings.SpatialPartitionsPerAxis;

            Dependency = Entities.WithName("System_UnitMovement")
                .WithAll<UnitTag>()
                .ForEach((ref UnitPosition unitPosition, ref UnitRotation unitRotation, ref UnitVelocity unitVelocity, ref UnitSpatialHash unitSpatialHash, in UnitSpeed unitSpeed, in UnitRNG unitRng, in UnitAvoidance unitAvoidance) =>
                {
                    var forward = float3.zero;
                    if (math.lengthsq(unitAvoidance.avoidance) > 0.001f)
                    {
                        forward = math.normalize(unitVelocity.velocity);
                        forward = math.lerp(forward, unitAvoidance.avoidance, unitTurnSpeed * deltaTime);
       
                        unitRotation.rotation = quaternion.LookRotation(forward, new float3(0f, 1f, 0f));
                    }
                    else
                    {
                        forward = math.mul(math.mul(unitRotation.rotation, quaternion.Euler(0f, unitRng.rng.NextFloat(-unitSteerMinMax, unitSteerMinMax), 0f)), new float3(0f, 0f, 1f));

                        unitRotation.rotation = quaternion.LookRotation(forward, new float3(0f, 1f, 0f));
                    }

                    unitVelocity.velocity = forward * unitSpeed.speed;
                    unitPosition.position += unitVelocity.velocity * deltaTime;

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

                    unitSpatialHash.hash = SpatialPartition.GridHash(in unitPosition.position, in worldSize, spatialPartitionsPerAxis);
                }).ScheduleParallel(Dependency);
        }
    }
}