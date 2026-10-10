using UnityEngine;

public sealed class PrototypeFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -9f);
    [SerializeField] private float smooth = 8f;

    public void SetTarget(Transform newTarget) => target = newTarget;

    private void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}
