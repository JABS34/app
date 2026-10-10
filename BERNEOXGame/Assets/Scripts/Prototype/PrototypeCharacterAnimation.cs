using UnityEngine;

/// <summary>Lightweight procedural walk, idle and attack motion for the prototype rigs.</summary>
public sealed class PrototypeCharacterAnimation : MonoBehaviour
{
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftLeg;
    [SerializeField] private Transform rightLeg;
    [SerializeField] private Transform head;
    [SerializeField] private Transform bodyMotionTarget;
    private CharacterController controller;
    private PrototypeMeleeAttack meleeAttack;
    private PrototypePlayerHealth playerHealth;
    private Quaternion leftArmRest;
    private Quaternion rightArmRest;
    private Quaternion leftLegRest;
    private Quaternion rightLegRest;
    private Quaternion headRest;
    private Vector3 bodyPositionRest;
    private Quaternion bodyRotationRest;
    private Transform zombieRig;
    private Quaternion zombieRigRest;
    private bool isZombie;
    private float attackUntil;
    private float hitUntil;

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
        playerHealth = GetComponent<PrototypePlayerHealth>();
        isZombie = GetComponent<PrototypeZombieEnemy>() != null;
        zombieRig = leftArm != null ? leftArm.parent : null;
        if (zombieRig != null) zombieRigRest = zombieRig.localRotation;
        if (meleeAttack != null) meleeAttack.Attacked += PlayAttack;
        if (playerHealth != null) playerHealth.HealthChanged += OnHealthChanged;
        CaptureRestPose();
    }

    private void OnDestroy()
    {
        if (meleeAttack != null) meleeAttack.Attacked -= PlayAttack;
        if (playerHealth != null) playerHealth.HealthChanged -= OnHealthChanged;
    }

    public void PlayAttack() => attackUntil = Time.time + 0.28f;
    public void PlayHit() => hitUntil = Time.time + 0.22f;

    private void OnHealthChanged(float current, float maximum)
    {
        if (current < maximum) PlayHit();
    }

    public void SetBodyMotionTarget(Transform target)
    {
        bodyMotionTarget = target;
        if (bodyMotionTarget != null)
        {
            bodyPositionRest = bodyMotionTarget.localPosition;
            bodyRotationRest = bodyMotionTarget.localRotation;
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        bool walking = controller != null && new Vector2(controller.velocity.x, controller.velocity.z).sqrMagnitude > 0.08f;
        float phase = Time.time * (walking ? 8.5f : 1.6f);
        float stride = walking ? Mathf.Sin(phase) : Mathf.Sin(phase) * 0.035f;
        float swing = walking ? 28f : 3f;
        float breathing = Mathf.Sin(Time.time * (isZombie ? 2.3f : 1.65f));
        float attack = Mathf.Clamp01((attackUntil - Time.time) / 0.28f);
        float hit = Mathf.Clamp01((hitUntil - Time.time) / 0.22f);
        if (bodyMotionTarget != null)
        {
            float strideBounce = walking ? Mathf.Abs(Mathf.Sin(phase * 2f)) * 0.035f : 0f;
            bodyMotionTarget.localPosition = bodyPositionRest + Vector3.up * (breathing * 0.012f + strideBounce);
            float lean = walking ? 4f : 0f;
            bodyMotionTarget.localRotation = bodyRotationRest * Quaternion.Euler(lean + breathing * 0.7f - hit * 11f, Mathf.Sin(phase) * (walking ? 2f : 0.8f), stride * (walking ? 1.6f : 0.5f));
        }
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
            if (attack > 0f) armMotion = rightArmRest * Quaternion.Euler(-105f * attack, 0f, -12f * attack);
            if (hit > 0f) armMotion = Quaternion.Slerp(armMotion, rightArmRest * Quaternion.Euler(30f, 0f, 24f), hit);
            rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, armMotion, Time.deltaTime * 14f);
        }
        if (leftLeg != null) leftLeg.localRotation = leftLegRest * Quaternion.Euler(stride * (walking ? 32f : 2f), 0f, 0f);
        if (rightLeg != null) rightLeg.localRotation = rightLegRest * Quaternion.Euler(-stride * (walking ? 32f : 2f), 0f, 0f);
        if (leftArm != null && hit > 0f && !isZombie)
            leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, leftArmRest * Quaternion.Euler(35f * hit, 0f, -22f * hit), Time.deltaTime * 10f);
        if (head != null)
        {
            float look = Mathf.Sin(Time.time * 0.7f) * (isZombie ? 8f : 4.5f);
            float scan = !isZombie && !walking ? Mathf.Sin(Time.time * 0.31f) * 13f : 0f;
            float tilt = isZombie ? -12f + Mathf.Sin(Time.time * 1.1f) * 6f : Mathf.Sin(Time.time * 1.65f) * 1.4f;
            head.localRotation = headRest * Quaternion.Euler(look + tilt, scan + Mathf.Sin(Time.time * 0.45f) * (isZombie ? 10f : 5f), Mathf.Sin(Time.time * 0.8f) * (isZombie ? 4f : 1.6f));
        }
    }

    private void CaptureRestPose()
    {
        if (leftArm != null) leftArmRest = leftArm.localRotation;
        if (rightArm != null) rightArmRest = rightArm.localRotation;
        if (leftLeg != null) leftLegRest = leftLeg.localRotation;
        if (rightLeg != null) rightLegRest = rightLeg.localRotation;
        if (head != null) headRest = head.localRotation;
        if (bodyMotionTarget != null)
        {
            bodyPositionRest = bodyMotionTarget.localPosition;
            bodyRotationRest = bodyMotionTarget.localRotation;
        }
    }
}
