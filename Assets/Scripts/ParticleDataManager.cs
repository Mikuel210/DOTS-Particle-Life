using System.Collections.Generic;
using EditorAttributes;
using UnityEditor;
using UnityEngine;

public class ParticleDataManager : Singleton<ParticleDataManager>
{
    public List<ParticleDataSO> particleTypes;

    [Button("Randomize Values")]
    public void RandomizeValues()
    {
        foreach (ParticleDataSO data in particleTypes)
        {
            for (int i = 0; i < data.interactions.Length; i++)
            {
                data.interactions[i].attractionForce = Random.Range(0, 100) / 100f;
                data.interactions[i].minimumDistance = Random.Range(10, 20);
            }

            EditorUtility.SetDirty(data);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
