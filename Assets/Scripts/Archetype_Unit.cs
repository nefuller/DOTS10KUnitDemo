using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS10KUnitDemo
{
    public static class Archetype_Unit
    {
        public static ComponentType[] Archetype =
        {
            ComponentType.ReadWrite<UnitTag>(),
            ComponentType.ReadWrite<UnitIndex>(),
            ComponentType.ReadWrite<UnitTeam>(),
            ComponentType.ReadWrite<UnitPosition>(),
            ComponentType.ReadWrite<UnitRotation>(),
            ComponentType.ReadWrite<UnitVelocity>(),
            ComponentType.ReadWrite<UnitSpatialHash>(),
            ComponentType.ReadWrite<UnitSpeed>(),
            ComponentType.ReadWrite<UnitRNG>(),
            ComponentType.ReadWrite<UnitAvoidance>()
        };

        public static Entity Create(EntityManager entityManager, int index, Team team, float3 position, quaternion rotation, Unity.Mathematics.Random masterRng)
        {
            Entity unitEntity = entityManager.CreateEntity(Archetype);

            entityManager.AddComponentData(unitEntity, new UnitTag());
            entityManager.AddComponentData(unitEntity, new UnitIndex(index));
            entityManager.SetComponentData(unitEntity, new UnitTeam(team));
            entityManager.SetComponentData(unitEntity, new UnitPosition(position));
            entityManager.SetComponentData(unitEntity, new UnitRotation(rotation));
            entityManager.SetComponentData(unitEntity, new UnitVelocity(math.mul(rotation, new float3(0, 0, 1))));
            entityManager.SetComponentData(unitEntity, new UnitSpatialHash(SpatialPartition.GridHash(in position, in GameSettings.WorldSize, GameSettings.SpatialPartitionsPerAxis)));
            entityManager.SetComponentData(unitEntity, new UnitSpeed(GameSettings.UnitSpeed));
            entityManager.SetComponentData(unitEntity, new UnitRNG(masterRng.NextUInt()));
            entityManager.SetComponentData(unitEntity, new UnitAvoidance(0f));

            return unitEntity;
        }
    }

    public enum Team
    {
        Red,
        Blue
    }

    public struct UnitTag : IComponentData
    {
    }

    public struct UnitIndex : IComponentData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitIndex(int index)
        {
            this.index = index;
        }

        public int index;
    }

    public struct UnitTeam : IComponentData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitTeam(Team team)
        {
            this.team = team;
        }

        public Team team;
    }

    public struct UnitPosition : IComponentData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitPosition(float3 position)
        {
            this.position = position;
        }

        public float3 position;
    }

    public struct UnitRotation : IComponentData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitRotation(quaternion rotation)
        {
            this.rotation = rotation;
        }

        public quaternion rotation;
    }

    public struct UnitSpatialHash : IComponentData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitSpatialHash(int hash)
        {
            this.hash = hash;
        }

        public int hash;
    }

    public struct UnitSpeed : IComponentData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitSpeed(float speed)
        {
            this.speed = speed;
        }

        public float speed;
    }

    public struct UnitRNG : IComponentData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitRNG(uint seed)
        {
            this.rng = new Unity.Mathematics.Random(seed);
        }

        public Unity.Mathematics.Random rng;
    }

    public struct UnitAvoidance : IComponentData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitAvoidance(float3 avoidance)
        {
            this.avoidance = avoidance;
        }

        public float3 avoidance;
    }

    public struct UnitVelocity : IComponentData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnitVelocity(float3 velocity)
        {
            this.velocity = velocity;
        }

        public float3 velocity;
    }
}