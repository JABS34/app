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
    private Transform zombieRig;
    private Quaternion zombieRigRest;
    private bool isZombie;
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
        isZombie = GetComponent<PrototypeZombieEnemy>() != null;
        zombieRig = leftArm != null ? leftArm.parent : null;
        if (zombieRig != null) zombieRigRest = zombieRig.localRotation;
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
        if (isZombie)
        {
            float stagger = Mathf.Sin(Time.time * 2.15f) * (walking ? 7f : 12f);
            if (zombieRig != null)
                zombieRig.localRotation = zombieRigRest * Quaternion.Euler(Mathf.Sin(phase * 0.6f) * 2.5f, 0f, stagger * 0.25f);
            if (leftArm != null) leftArm.localRotation = leftArmRest * Quaternion.Euler(-stride * swing - 8f + stagger * 0.3f, 0f, -8f + stagger * 0.6f);
        }
        else if (leftArm != null)
            leftArm.localRotation = leftArmRest * Quaternion.Euler(-stride * swing, 0f, 0f);
        if (rightArm != null)
        {
            Quaternion armMotion = rightArmRest * Quaternion.Euler(stride * swing + (isZombie ? -13f : 0f), 0f, isZombie ? 9f + Mathf.Sin(phase * 0.8f) * 6f : 0f);
            if (Time.time < attackUntil) armMotion = rightArmRest * Quaternion.Euler(-72f, 0f, 0f);
            rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, armMotion, Time.deltaTime * 14f);
        }
        if (leftLeg != null) leftLeg.localRotation = leftLegRest * Quaternion.Euler(stride * (walking ? 32f : 2f), 0f, 0f);
        if (rightLeg != null) rightLeg.localRotation = rightLegRest * Quaternion.Euler(-stride * (walking ? 32f : 2f), 0f, 0f);
        if (head != null)
        {
            float look = Mathf.Sin(Time.time * 0.7f) * (isZombie ? 5f : 2.5f);
            float tilt = isZombie ? -7f + Mathf.Sin(Time.time * 1.1f) * 4f : 0f;
            head.localRotation = headRest * Quaternion.Euler(look + tilt, Mathf.Sin(Time.time * 0.45f) * (isZombie ? 7f : 4f), 0f);
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
