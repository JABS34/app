using UnityEngine;

/// <summary>Subtle wind, drifting ash and unstable light for the abandoned settlement.</summary>
public sealed class PrototypeApocalypseMotion : MonoBehaviour
{
    [SerializeField] private Transform[] windTargets;
    [SerializeField] private Light campfireLight;
    [SerializeField] private Light[] unstableLights;

    private Quaternion[] restRotations;
    private float campfireBaseIntensity;
    private float[] unstableBaseIntensity;
    [SerializeField] private MeshFilter[] waterSurfaces;
    private Vector3[][] waterRestVertices;
    private Vector3[][] waterAnimatedVertices;

    public Transform[] AnimatedTransforms => windTargets;

    public void Configure(Transform[] foliage, Light ember, Light[] lamps)
    {
        windTargets = foliage;
        campfireLight = ember;
        unstableLights = lamps;
        CacheRestPose();
    }

    public void ConfigureWater(MeshFilter[] surfaces)
    {
        waterSurfaces = surfaces;
        CacheWaterRestPose();
    }

    private void Awake()
    {
        CacheRestPose();
        if (waterSurfaces == null || waterSurfaces.Length == 0)
        {
            MeshFilter[] candidates = GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < candidates.Length; i++)
            {
                if (candidates[i] == null || candidates[i].name != "Animated Pond Water") continue;
                waterSurfaces = new[] { candidates[i] };
                break;
            }
        }
        CacheWaterRestPose();
    }

    private void CacheWaterRestPose()
    {
        waterRestVertices = new Vector3[waterSurfaces != null ? waterSurfaces.Length : 0][];
        waterAnimatedVertices = new Vector3[waterRestVertices.Length][];
        for (int i = 0; i < waterRestVertices.Length; i++)
            if (waterSurfaces[i] != null && waterSurfaces[i].sharedMesh != null)
            {
                waterRestVertices[i] = waterSurfaces[i].sharedMesh.vertices;
                waterAnimatedVertices[i] = new Vector3[waterRestVertices[i].Length];
            }
    }

    private void CacheRestPose()
    {
        if (windTargets != null)
        {
            restRotations = new Quaternion[windTargets.Length];
            for (int i = 0; i < windTargets.Length; i++)
                if (windTargets[i] != null) restRotations[i] = windTargets[i].localRotation;
        }
        campfireBaseIntensity = campfireLight != null ? campfireLight.intensity : 0f;
        if (unstableLights != null)
        {
            unstableBaseIntensity = new float[unstableLights.Length];
            for (int i = 0; i < unstableLights.Length; i++)
                if (unstableLights[i] != null) unstableBaseIntensity[i] = unstableLights[i].intensity;
        }
    }

    private void Update()
    {
        float time = Time.unscaledTime;
        if (waterSurfaces != null && waterRestVertices != null)
        {
            for (int i = 0; i < waterSurfaces.Length; i++)
            {
                MeshFilter filter = waterSurfaces[i];
                if (filter == null || filter.sharedMesh == null || waterRestVertices[i] == null) continue;
                Vector3[] vertices = waterRestVertices[i];
                Vector3[] animated = waterAnimatedVertices[i];
                if (animated == null || animated.Length != vertices.Length)
                {
                    animated = new Vector3[vertices.Length];
                    waterAnimatedVertices[i] = animated;
                }
                for (int v = 0; v < vertices.Length; v++)
                {
                    Vector3 point = vertices[v];
                    float waveA = Mathf.Sin(point.x * 0.70f + time * 1.45f);
                    float waveB = Mathf.Sin(point.z * 0.82f - time * 1.12f + point.x * 0.35f);
                    point.y += (waveA + waveB) * 0.018f;
                    animated[v] = point;
                }
                filter.sharedMesh.vertices = animated;
                filter.sharedMesh.RecalculateNormals();
            }
        }
        if (windTargets != null && restRotations != null)
        {
            for (int i = 0; i < windTargets.Length; i++)
            {
                Transform target = windTargets[i];
                if (target == null) continue;
                float phase = time * 0.75f + i * 1.71f;
                float bend = Mathf.Sin(phase) * 2.2f + Mathf.Sin(phase * 0.43f) * 1.1f;
                target.localRotation = restRotations[i] * Quaternion.Euler(bend * 0.35f, 0f, bend);
            }
        }

        if (campfireLight != null)
        {
            float flicker = Mathf.PerlinNoise(time * 7.2f, 0.41f);
            campfireLight.intensity = campfireBaseIntensity * Mathf.Lerp(0.72f, 1.24f, flicker);
        }

        if (unstableLights != null && unstableBaseIntensity != null)
        {
            for (int i = 0; i < unstableLights.Length; i++)
            {
                Light lamp = unstableLights[i];
                if (lamp == null) continue;
                float noise = Mathf.PerlinNoise(time * 3.4f + i * 5.1f, 0.73f);
                float stutter = noise < 0.27f ? 0.12f : Mathf.Lerp(0.68f, 1.12f, noise);
                lamp.intensity = unstableBaseIntensity[i] * stutter;
            }
        }
    }
}
