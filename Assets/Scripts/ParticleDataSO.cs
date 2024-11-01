using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Particle Data", menuName = "Game/Particle Data")]
public class ParticleDataSO : ScriptableObject
{
    public ParticleInteraction[] interactions;
}

[Serializable]
public struct ParticleInteraction
{
    public float attractionForce;
    public float minimumDistance;
}