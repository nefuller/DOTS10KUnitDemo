using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS10KUnitDemo
{
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

        float3 bounds = new float3(GameSettings.WorldSize.size.x, 1000.0f, GameSettings.WorldSize.size.y);

        public void Reset(Mesh mesh, Material material)
        {
            instanceMesh        = mesh;
            instanceMaterial    = material;

            masterRng           = new Unity.Mathematics.Random(masterSeed);

            unitBufferData      = new NativeArray<Compute_Unit>(GameSettings.UnitMax, Allocator.Persistent);

            ResetArgBuffer();
            ResetPositionBuffer();

            instanceMaterial.SetBuffer("_UnitBuffer", unitBuffer);
        }

        protected override void OnCreate()
        {
            updateUnitBufferQuery = GetEntityQuery(ComponentType.ReadOnly<UnitPosition>(), ComponentType.ReadOnly<UnitIndex>());
        }

        protected override void OnUpdate()
        {
            IJobChunk_UpdateUnitBuffer.ScheduleParallel(updateUnitBufferQuery, ref unitBufferData, this.Dependency).Complete();
            unitBuffer.SetData(unitBufferData);

            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(float3.zero, bounds), argBuffer);
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

        void ResetArgBuffer()
        {
            var args = new NativeArray<uint>(5, Allocator.Temp);
            args[0] = instanceMesh.GetIndexCount(0);
            args[1] = (uint)GameSettings.UnitMax;
            args[2] = instanceMesh.GetIndexStart(0);
            args[3] = instanceMesh.GetBaseVertex(0);
            args[4] = 0;

            argBuffer = new ComputeBuffer(5, UnsafeUtility.SizeOf<uint>(), ComputeBufferType.IndirectArguments);
            argBuffer.SetData(args);

            args.Dispose();
        }

        void ResetPositionBuffer()
        {
            for (var i = 0; i < GameSettings.UnitMax; ++i)
            {
                var team = i < (GameSettings.UnitMax / 2) ? Team.Red : Team.Blue;

                unitBufferData[i] = new Compute_Unit()
                {
                    position = new float3(
                        UnityEngine.Random.Range(-GameSettings.UnitSpawnAreaHalfSize.x, GameSettings.UnitSpawnAreaHalfSize.x),
                        0f,
                        UnityEngine.Random.Range(-GameSettings.UnitSpawnAreaHalfSize.y, GameSettings.UnitSpawnAreaHalfSize.y)),
                    color = team == Team.Red ? new float4(1f, 0, 0, 1f) : new float4(0, 0, 1f, 1f),
                    rotation = quaternion.identity.value
                };

                Archetype_Unit.Create(EntityManager, i, team, unitBufferData[i].position, unitBufferData[i].rotation, new Unity.Mathematics.Random(masterRng.NextUInt()));
            }

            unitBuffer = new ComputeBuffer(GameSettings.UnitMax, UnsafeUtility.SizeOf<Compute_Unit>());
            unitBuffer.SetData(unitBufferData);
        }
    }
}
