using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS10KUnitDemo
{
    /// <summary>
    /// Uploads component data for units to the GPU so they can be rendered via GPU instancing.
    /// </summary>
    [UpdateAfter(typeof(System_UnitMovement))]
    public sealed partial class System_UnitRenderer : SystemBase
    {
        Mesh instanceMesh;
        Material instanceMaterial;

        ComputeBuffer argBuffer;
        ComputeBuffer unitBuffer;

        static readonly uint masterSeed = 1;
        Unity.Mathematics.Random masterRng;

        NativeArray<Compute_Unit> unitBufferData;

        EntityQuery updateUnitBufferQuery;

        float3 instanceMeshBounds;

        public void Reset(Mesh mesh, Material material)
        {
            instanceMesh        = mesh;
            instanceMaterial    = material;
            instanceMeshBounds  = new float3(GameSettings.WorldSize.size.x, 1000.0f, GameSettings.WorldSize.size.y);

            // All units have their own rng that is seeded with a value from the master rng.
            masterRng           = new Unity.Mathematics.Random(masterSeed);

            unitBufferData      = new NativeArray<Compute_Unit>(GameSettings.UnitMax, Allocator.Persistent);

            ResetArgBuffer();
            ResetUnitBuffer();

            instanceMaterial.SetBuffer("_UnitBuffer", unitBuffer);
        }

        protected override void OnCreate()
        {
            updateUnitBufferQuery = GetEntityQuery(ComponentType.ReadOnly<UnitPosition>(), ComponentType.ReadOnly<UnitIndex>());
        }

        protected override void OnUpdate()
        {
            // Update the unit buffer with entity component data, then upload it to the GPU.
            IJobChunk_UpdateUnitBuffer.ScheduleParallel(updateUnitBufferQuery, ref unitBufferData, this.Dependency).Complete();
            unitBuffer.SetData(unitBufferData);

            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(float3.zero, instanceMeshBounds), argBuffer);
        }

        protected override void OnDestroy()
        {
            argBuffer.Release();
            argBuffer.Dispose();
            argBuffer = null;

            unitBuffer.Release();
            unitBuffer.Dispose();
            unitBuffer = null;

            unitBufferData.Dispose();
        }

        /// <summary>
        /// Sets up the args buffer required by Graphics.DrawMeshInstancedIndirect().
        /// </summary>
        void ResetArgBuffer()
        {
            var args = new NativeArray<uint>(5, Allocator.Temp);
            args[0] = instanceMesh.GetIndexCount(0);
            args[1] = (uint)GameSettings.UnitMax;
            args[2] = instanceMesh.GetIndexStart(0);
            args[3] = instanceMesh.GetBaseVertex(0);
            args[4] = 0;

            // Upload args buffer to GPU.
            argBuffer = new ComputeBuffer(5, UnsafeUtility.SizeOf<uint>(), ComputeBufferType.IndirectArguments);
            argBuffer.SetData(args);

            args.Dispose();
        }

        /// <summary>
        /// Initializes units randomly within the play area.
        /// </summary>
        void ResetUnitBuffer()
        {
            for (var i = 0; i < GameSettings.UnitMax; ++i)
            {
                // Make half of the units red team and half blue team.
                var team = i % 2 == 0 ? Team.Red : Team.Blue;

                // Initialize the unit's compute buffer representation.
                unitBufferData[i] = new Compute_Unit()
                {
                    position = new float3(
                        UnityEngine.Random.Range(-GameSettings.UnitSpawnAreaHalfSize.x, GameSettings.UnitSpawnAreaHalfSize.x),
                        0f,
                        UnityEngine.Random.Range(-GameSettings.UnitSpawnAreaHalfSize.y, GameSettings.UnitSpawnAreaHalfSize.y)),
                    color = team == Team.Red ? new float4(1f, 0, 0, 1f) : new float4(0, 0, 1f, 1f),
                    rotation = quaternion.identity.value
                };

                // Initialize the unit's ECS representation.
                Archetype_Unit.Create(EntityManager, i, team, unitBufferData[i].position, unitBufferData[i].rotation, new Unity.Mathematics.Random(masterRng.NextUInt()));
            }

            // Upload unit buffer to GPU.
            unitBuffer = new ComputeBuffer(GameSettings.UnitMax, UnsafeUtility.SizeOf<Compute_Unit>());
            unitBuffer.SetData(unitBufferData);
        }
    }
}
