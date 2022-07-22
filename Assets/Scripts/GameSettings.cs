using Unity.Burst;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    public class GameSettings
    {
        /// <summary>
        /// Half-size of world centered at origin of coordinate system.
        /// </summary>
        public static readonly float2 WorldHalfSize = new float2(500f, 500f);

        /// <summary>
        /// Number of units to simulate.
        /// </summary>
        public static readonly int UnitMax = 10000;

        /// <summary>
        /// Distance from the world boundary that units are prevented from
        /// exceeding so they don't fall outside the spatial partitioning area.
        /// </summary>
        public static readonly float WorldBoundsEpsilon = 0.001f;

        /// <summary>
        /// Distance from world boundary at which units begin to receive
        /// boundary separation forces.
        /// </summary>
        public static float BoundarySeparationRange = 60f;

        /// <summary>
        /// Strength of boundary separation force.
        /// </summary>
        public static float BoundarySeparationStrength = 1f;

        /// <summary>
        /// Unit parameters.
        /// </summary>
        public static float UnitSpeed                       = 15.0f;
        public static float UnitTurnSpeed                   = 5.0f;
        public static float UnitSteerMinMax                 = 0.005f;

        public static readonly float2 UnitSpawnAreaHalfSize = new float2(300f, 300f);

        /// <summary>
        /// Flocking parameters.
        /// </summary>
        public static float AlignmentStrength               = 6.3f;
        public static float CohesionStrength                = 0.6f;
        public static float SeparationStrength              = 20f;
        public static float AlignmentRange                  = 25f;
        public static float CohesionRange                   = 25f;
        public static float SeparationRange                 = 5f;

        /// <summary>
        /// Spatial partitioning parameters.
        /// </summary>
        public static readonly int SpatialPartitionsPerAxis             = 256;
        public static readonly int SpatialPartitionsTotal               = SpatialPartitionsPerAxis * SpatialPartitionsPerAxis;
        public static readonly int SpatialPartitionDirectionsLength     = 9;
        public static readonly int2[] SpatialPartitionDirections = 
        {
            new int2(-1, -1),
            new int2(0, -1),
            new int2(1, -1),
            new int2(-1, 0),
            new int2(0, 0),
            new int2(1, 0),
            new int2(-1, 1),
            new int2(0, 1),
            new int2(1, 1)
        };

        public static readonly WorldSize WorldSize = new WorldSize()
        {
            size                = WorldHalfSize * 2,
            halfSize            = WorldHalfSize,
        };

        public static readonly WorldBounds WorldBounds = new WorldBounds()
        {
            halfSize            = WorldHalfSize - new float2(WorldBoundsEpsilon, WorldBoundsEpsilon),
            separationHalfSize  = WorldHalfSize - new float2(BoundarySeparationRange, BoundarySeparationRange)
        };
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct WorldSize
    {
        public float2 halfSize;
        public float2 size;
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct WorldBounds
    {
        public float2 halfSize;
        public float2 separationHalfSize;
    }
}
