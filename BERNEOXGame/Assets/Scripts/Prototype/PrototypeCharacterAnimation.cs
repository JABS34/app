using UnityEngine;

/// <summary>Lightweight procedural walk, idle and attack motion for the prototype rigs.</summary>
public sealed class PrototypeCharacterAnimation : MonoBehaviour
{
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftLeg;
    [SerializeField] private Transform rightLeg;
    [SerializeField] private Transform head;
    private CharacterController controller;
    private PrototypeMeleeAttack meleeAttack;
    private Quaternion leftArmRest;
    private Quaternion rightArmRest;
    private Quaternion leftLegRest;
    private Quaternion rightLegRest;
    private Quaternion headRest;
    private float attackUntil;

    public void Configure(Transform leftArmPivot, Transform rightArmPivot, Transform leftLegPivot,
        Transform rightLegPivot, Transform headPivot)
    {
        leftArm = leftArmPivot;
        rightArm = rightArmPivot;
        leftLeg = leftLegPivot;
        rightLeg = rightLegPivot;
        head = headPivot;
        CaptureRestPose();
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        meleeAttack = GetComponent<PrototypeMeleeAttack>();
        if (meleeAttack != null) meleeAttack.Attacked += PlayAttack;
        CaptureRestPose();
    }

    private void OnDestroy()
    {
        if (meleeAttack != null) meleeAttack.Attacked -= PlayAttack;
    }

    public void PlayAttack() => attackUntil = Time.time + 0.28f;

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        bool walking = controller != null && new Vector2(controller.velocity.x, controller.velocity.z).sqrMagnitude > 0.08f;
        float phase = Time.time * (walking ? 8.5f : 1.6f);
        float stride = walking ? Mathf.Sin(phase) : Mathf.Sin(phase) * 0.035f;
        float swing = walking ? 28f : 3f;
        if (leftArm != null) leftArm.localRotation = leftArmRest * Quaternion.Euler(-stride * swing, 0f, 0f);
        if (rightArm != null)
        {
            Quaternion armMotion = rightArmRest * Quaternion.Euler(stride * swing, 0f, 0f);
            if (Time.time < attackUntil) armMotion = rightArmRest * Quaternion.Euler(-72f, 0f, 0f);
            rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, armMotion, Time.deltaTime * 14f);
        }
        if (leftLeg != null) leftLeg.localRotation = leftLegRest * Quaternion.Euler(stride * (walking ? 32f : 2f), 0f, 0f);
        if (rightLeg != null) rightLeg.localRotation = rightLegRest * Quaternion.Euler(-stride * (walking ? 32f : 2f), 0f, 0f);
        if (head != null)
        {
            float look = Mathf.Sin(Time.time * 0.7f) * 2.5f;
            head.localRotation = headRest * Quaternion.Euler(look, Mathf.Sin(Time.time * 0.45f) * 4f, 0f);
        }
    }

    private void CaptureRestPose()
    {
        if (leftArm != null) leftArmRest = leftArm.localRotation;
        if (rightArm != null) rightArmRest = rightArm.localRotation;
        if (leftLeg != null) leftLegRest = leftLeg.localRotation;
        if (rightLeg != null) rightLegRest = rightLeg.localRotation;
        if (head != null) headRest = head.localRotation;
    }
}
