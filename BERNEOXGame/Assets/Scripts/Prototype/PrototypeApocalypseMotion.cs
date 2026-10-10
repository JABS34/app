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

    public Transform[] AnimatedTransforms => windTargets;

    public void Configure(Transform[] foliage, Light ember, Light[] lamps)
    {
        windTargets = foliage;
        campfireLight = ember;
        unstableLights = lamps;
        CacheRestPose();
    }

    private void Awake() => CacheRestPose();

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
