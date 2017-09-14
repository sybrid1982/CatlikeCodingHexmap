using UnityEngine;

public class HexFeatureManager : MonoBehaviour {
    public HexFeatureCollection[] urbanCollections, farmCollections, plantCollections;

    Transform container;

    public void Clear()
    {
        if (container)
        {
            Destroy(container.gameObject);
        }
        container = new GameObject("Features Container").transform;
        container.SetParent(transform, false);
    }
    public void Apply()
    {

    }
    public void AddFeature(HexCell cell, Vector3 position)
    {
        HexHash hash = HexMetrics.SampleHashGrid(position);

        Transform prefab = PickPrefab(
            urbanCollections, cell.UrbanLevel, hash.a, hash.d);
        Transform otherPrefab = PickPrefab(
            farmCollections, cell.FarmLevel, hash.b, hash.d);

        prefab = ComparePrefabs(prefab, otherPrefab, hash.a, hash.b);
        float bestHash;
        if (hash.b < hash.a)
            bestHash = hash.b;
        else
            bestHash = hash.a;

        otherPrefab = PickPrefab(
            plantCollections, cell.PlantLevel, hash.c, hash.d);
        prefab = ComparePrefabs(prefab, otherPrefab, bestHash, hash.c);

        if (!prefab)
        {
            return;
        }
        Transform instance = Instantiate(prefab);
        position.y += instance.localScale.y * 0.5f;
        instance.localPosition = HexMetrics.Perturb(position);
        instance.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
        instance.SetParent(container, false);
    }

    Transform PickPrefab (HexFeatureCollection[] collection, 
        int level, float hash, float choice)
    {
        if (level > 0)
        {
            float[] thresholds = HexMetrics.GetFeatureThresholds(level - 1);
            for(int i = 0; i < thresholds.Length; i++)
            {
                if(hash < thresholds[i])
                {
                    return collection[i].Pick(choice);
                }
            }
        }
        return null;
    }

    Transform ComparePrefabs(Transform prefab, Transform otherPrefab, float hasha, float hashb)
    {
        if (prefab)
        {
            if (otherPrefab && hashb < hasha)
            { 
                return otherPrefab;
            } else
            {
                return prefab;
            }
        }
        else if (otherPrefab)
        {
            return (otherPrefab);
        }
        else
        {
            return null;
        }
    }
}
