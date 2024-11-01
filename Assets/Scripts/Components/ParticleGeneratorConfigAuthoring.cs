using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SimulationConfigAuthoring : MonoBehaviour
{
    [Header("Generator")]
    public int width;
    public int height;
    public float spacing;
    public List<GameObject> prefabs;

    [Header("Simulation")]
    public float timeScale;


    private class Baker : Baker<SimulationConfigAuthoring>
    {
        public override void Bake(SimulationConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SimulationConfig()
            {
                width = authoring.width,
                height = authoring.height,
                spacing = authoring.spacing,
                randomSeed = (uint)Random.Range(0, 1000),
                timeScale = authoring.timeScale
            });

            var buffer = AddBuffer<ParticleGeneratorPrefab>(entity);

            foreach (GameObject prefab in authoring.prefabs)
            {
                buffer.Add(new ParticleGeneratorPrefab()
                {
                    prefabEntity = GetEntity(prefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}

public struct SimulationConfig : IComponentData
{
    public int width;
    public int height;
    public float spacing;
    public uint randomSeed;

    public float timeScale;
}

public struct ParticleGeneratorPrefab : IBufferElementData
{
    public Entity prefabEntity;
}