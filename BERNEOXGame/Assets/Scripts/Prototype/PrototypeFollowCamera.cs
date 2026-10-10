using UnityEngine;

public sealed class PrototypeFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 4.8f, -7f);
    [SerializeField] private float smooth = 8f;

    public void SetTarget(Transform newTarget) => target = newTarget;

    private void LateUpdate()
    {
        if (!target) return;
        // Keep the camera behind the character as its facing direction changes.
        Vector3 desired = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}
