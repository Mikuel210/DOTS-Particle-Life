using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct ParticleSystem : ISystem
{
    const float FrictionFactor = 0.5f;
    const float Repulsion = 2;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Particle>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var timeScale = SystemAPI.GetSingleton<SimulationConfig>().timeScale;
        var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp);
        var query = entityQueryBuilder.WithAll<LocalTransform, Particle>().Build(state.EntityManager);
        var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var particles = query.ToComponentDataArray<Particle>(Allocator.TempJob);

        var job = new UpdateParticleJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            timeScale = timeScale,
            transforms = transforms,
            particles = particles
        };

        job.ScheduleParallel();
    }

    public static float DistanceSquared(float3 p, float3 q, out bool self)
    {
        float a = math.pow(p.x - q.x, 2);
        float b = math.pow(p.y - q.y, 2);
        float distanceSquared = math.abs(a + b);

        self = a == 0 && b == 0;

        return distanceSquared;
    }

    public static float3 Magnitude(float3 value)
    {
        var maxValue = math.max(math.abs(value.x), math.max(math.abs(value.y), math.abs(value.z)));
        return new float3(value.x / maxValue, value.y / maxValue, value.z / maxValue);
    }

    [BurstCompile]
    public partial struct UpdateParticleJob : IJobEntity
    {
        public float deltaTime;
        public float timeScale;

        [ReadOnly] public NativeArray<LocalTransform> transforms;
        [ReadOnly] public NativeArray<Particle> particles;


        public void Execute(ref LocalTransform transform1, ref Particle particle1, in DynamicBuffer<InteractionBufferElement> interactionBuffer1)
        {
            float3 acceleration = float3.zero;
            float3 p = transform1.Position;

            for (int i = 0; i < transforms.Length; i++)
            {
                LocalTransform transform2 = transforms[i];
                Particle particle2 = particles[i];

                InteractionBufferElement interactionBufferElement = interactionBuffer1[particle2.index];
                float attraction = interactionBufferElement.attractionForce;

                float3 q = transform2.Position;

                if (p.Equals(q)) continue;

                float distanceSquared = DistanceSquared(p, q, out bool self);
                if (self) continue;

                float distance = math.sqrt(distanceSquared);

                if (distance > interactionBufferElement.minimumDistance) continue;

                float force1 = attraction / distance;
                float force2 = Repulsion / distanceSquared;

                float force = force1 - force2;

                float3 direction = Magnitude(q - p);
                acceleration += force * direction;
            }

            particle1.velocity += acceleration * deltaTime * timeScale;
            particle1.velocity *= FrictionFactor;

            transform1 = transform1.Translate(particle1.velocity);
        }
    }
}
