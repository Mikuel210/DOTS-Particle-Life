using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ParticleDataAuthoring : MonoBehaviour
{
    public ParticleDataSO particleData;


    public class Baker : Baker<ParticleDataAuthoring>
    {
        public override void Bake(ParticleDataAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            ParticleDataSO particleData = authoring.particleData;

            // Add the particle component
            AddComponent(entity, new Particle()
            {
                index = ParticleDataManager.Instance.particleTypes.IndexOf(particleData)
            });

            // Add the interaction buffer
            ParticleInteraction[] interactions = particleData.interactions;
            var nativeArray = new NativeArray<ParticleInteraction>(interactions.Length, Allocator.Temp);
            nativeArray.CopyFrom(interactions);

            DynamicBuffer<InteractionBufferElement> interactionBuffer = AddBuffer<InteractionBufferElement>(entity);

            for (int i = 0; i < interactions.Length; i++)
            {
                ParticleInteraction interaction = interactions[i];

                interactionBuffer.Add(new InteractionBufferElement()
                {
                    index = i,
                    attractionForce = interaction.attractionForce,
                    minimumDistance = interaction.minimumDistance
                });
            }
        }
    }
}

public struct Particle : IComponentData
{
    public int index;
    public float3 velocity;
}

[InternalBufferCapacity(3)]
public struct InteractionBufferElement : IBufferElementData
{
    public int index;
    public float attractionForce;
    public float minimumDistance;
}