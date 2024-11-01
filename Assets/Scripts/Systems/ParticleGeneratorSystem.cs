using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct ParticleGeneratorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationConfig>();
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;


        var config = SystemAPI.GetSingleton<SimulationConfig>();
        var buffer = SystemAPI.GetSingletonBuffer<ParticleGeneratorPrefab>();
        var random = new Unity.Mathematics.Random(config.randomSeed);

        int w = config.width;
        int h = config.height;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var bufferElement = buffer.ElementAt(random.NextInt(buffer.Length));

                Entity entity = state.EntityManager.Instantiate(bufferElement.prefabEntity);

                state.EntityManager.SetComponentData(entity, new LocalTransform()
                {
                    Position = new float3((x - w / 2) * config.spacing, (y - h / 2) * config.spacing, 0),
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
            }
        }

    }
}
