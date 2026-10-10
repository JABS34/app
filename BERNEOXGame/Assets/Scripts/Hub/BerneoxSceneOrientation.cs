using UnityEngine;

/// <summary>Applies the orientation required by an individual BERNEOX scene.</summary>
public sealed class BerneoxSceneOrientation : MonoBehaviour
{
    [SerializeField] private ScreenOrientation orientation = ScreenOrientation.Portrait;

    public void SetOrientation(ScreenOrientation value) => orientation = value;

    private void Awake()
    {
        Screen.orientation = orientation;
    }
}
