using Unity.Entities;
using UnityEngine;

namespace DOTS10KUnitDemo
{
    public class EntryPoint_DOTS : MonoBehaviour
    {
        public Mesh tankMesh;
        public Material tankMaterial;

        private void OnEnable()
        {
            var world = World.DefaultGameObjectInjectionWorld;

            var systemUnitAvoidance = world.GetOrCreateSystem<System_UnitAvoidance>();
            systemUnitAvoidance.Reset();
            systemUnitAvoidance.Enabled = true;

            var systemUnitMovement = world.GetOrCreateSystem<System_UnitMovement>();
            systemUnitMovement.Reset();
            systemUnitMovement.Enabled = true;

            var systemUnitRenderer = world.GetOrCreateSystem<System_UnitRenderer>();
            systemUnitRenderer.Reset(tankMesh, tankMaterial);
            systemUnitRenderer.Enabled = true;
        }
    }
}