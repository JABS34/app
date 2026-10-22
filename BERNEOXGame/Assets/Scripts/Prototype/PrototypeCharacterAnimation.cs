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
    private PrototypePlayerController playerMovement;
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
    private float gaitBlend;
    private float landingUntil;
    private float defeatBlend;
    private bool isDefeated;

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
        playerMovement = GetComponent<PrototypePlayerController>();
        isZombie = GetComponent<PrototypeZombieEnemy>() != null;
        zombieRig = leftArm != null ? leftArm.parent : null;
        if (zombieRig != null) zombieRigRest = zombieRig.localRotation;
        if (meleeAttack != null) meleeAttack.Attacked += PlayAttack;
        if (playerHealth != null) playerHealth.HealthChanged += OnHealthChanged;
        if (playerHealth != null) playerHealth.Defeated += PlayDefeat;
        CaptureRestPose();
    }

    private void OnDestroy()
    {
        if (meleeAttack != null) meleeAttack.Attacked -= PlayAttack;
        if (playerHealth != null) playerHealth.HealthChanged -= OnHealthChanged;
        if (playerHealth != null) playerHealth.Defeated -= PlayDefeat;
    }

    public void PlayAttack() => attackUntil = Time.time + 0.48f;
    public void PlayHit()
    {
        hitUntil = Time.time + 0.32f;
        landingUntil = Time.time + 0.14f;
    }

    private void PlayDefeat()
    {
        isDefeated = true;
        attackUntil = Time.time;
        if (playerMovement != null) playerMovement.SetSprintHeld(false);
    }

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
        defeatBlend = Mathf.MoveTowards(defeatBlend, isDefeated ? 1f : 0f, Time.deltaTime * 1.35f);
        float planarSpeed = controller != null ? new Vector2(controller.velocity.x, controller.velocity.z).magnitude : 0f;
        float targetGait = isDefeated ? 0f : playerMovement != null ? playerMovement.MovementBlend : Mathf.Clamp01(planarSpeed / 4f);
        gaitBlend = Mathf.MoveTowards(gaitBlend, targetGait, Time.deltaTime * (targetGait > gaitBlend ? 5f : 7f));
        bool walking = gaitBlend > 0.035f;
        float sprint = playerMovement != null ? playerMovement.SprintBlend : 0f;
        float gaitRate = Mathf.Lerp(1.7f, Mathf.Lerp(9.6f, 13.2f, sprint), gaitBlend);
        float phase = Time.time * gaitRate;
        float stride = Mathf.Sin(phase) * gaitBlend;
        float swing = Mathf.Lerp(2.5f, Mathf.Lerp(27f, 37f, sprint), gaitBlend);
        float breathing = Mathf.Sin(Time.time * (isZombie ? 2.3f : 1.65f));
        float attackProgress = Mathf.Clamp01(1f - (attackUntil - Time.time) / 0.48f);
        float attack = attackUntil > Time.time ? Mathf.Sin(attackProgress * Mathf.PI) : 0f;
        float hit = Mathf.Clamp01((hitUntil - Time.time) / 0.32f);
        float landing = Mathf.Clamp01((landingUntil - Time.time) / 0.14f);
        if (bodyMotionTarget != null)
        {
            float strideBounce = Mathf.Abs(Mathf.Sin(phase * 2f)) * 0.065f * gaitBlend;
            float runDip = landing * 0.09f;
            bodyMotionTarget.localPosition = bodyPositionRest + Vector3.up * (breathing * 0.016f + strideBounce - runDip);
            float lean = Mathf.Lerp(0f, 8.5f, gaitBlend) + Mathf.Lerp(0f, 5f, sprint);
            float attackLean = attack * -9f;
            bodyMotionTarget.localRotation = bodyRotationRest * Quaternion.Euler(lean + attackLean + breathing * 1.1f - hit * 16f,
                Mathf.Sin(phase) * Mathf.Lerp(0.8f, 4.5f, gaitBlend), stride * Mathf.Lerp(0.7f, 4f, gaitBlend));
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
            if (attack > 0f)
            {
                float windup = attackProgress < 0.28f ? Mathf.InverseLerp(0f, 0.28f, attackProgress) : 0f;
                float strike = attackProgress >= 0.28f ? Mathf.InverseLerp(1f, 0.28f, attackProgress) : 0f;
                armMotion = rightArmRest * Quaternion.Euler(Mathf.Lerp(-34f, 48f, windup) - 112f * strike,
                    0f, Mathf.Lerp(24f, -18f, windup) - 10f * strike);
            }
            if (hit > 0f) armMotion = Quaternion.Slerp(armMotion, rightArmRest * Quaternion.Euler(30f, 0f, 24f), hit);
            rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, armMotion, Time.deltaTime * 14f);
        }
        float legSwing = Mathf.Lerp(2f, Mathf.Lerp(29f, 39f, sprint), gaitBlend);
        if (leftLeg != null) leftLeg.localRotation = leftLegRest * Quaternion.Euler(stride * legSwing - landing * 7f, 0f, -gaitBlend * 1.5f);
        if (rightLeg != null) rightLeg.localRotation = rightLegRest * Quaternion.Euler(-stride * legSwing - landing * 7f, 0f, gaitBlend * 1.5f);
        if (leftArm != null && hit > 0f && !isZombie)
            leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, leftArmRest * Quaternion.Euler(35f * hit, 0f, -22f * hit), Time.deltaTime * 10f);
        if (head != null)
        {
            float look = Mathf.Sin(Time.time * 0.7f) * (isZombie ? 8f : 4.5f);
            float scan = !isZombie && !walking ? Mathf.Sin(Time.time * 0.31f) * 13f : 0f;
            float tilt = isZombie ? -12f + Mathf.Sin(Time.time * 1.1f) * 6f : Mathf.Sin(Time.time * 1.65f) * 1.4f;
            head.localRotation = headRest * Quaternion.Euler(look + tilt, scan + Mathf.Sin(Time.time * 0.45f) * (isZombie ? 10f : 5f), Mathf.Sin(Time.time * 0.8f) * (isZombie ? 4f : 1.6f));
        }
        if (isDefeated && !isZombie)
        {
            float easedFall = defeatBlend * defeatBlend * (3f - 2f * defeatBlend);
            if (bodyMotionTarget != null)
            {
                bodyMotionTarget.localPosition = bodyPositionRest + Vector3.up * (-0.46f * easedFall);
                bodyMotionTarget.localRotation = bodyRotationRest * Quaternion.Euler(68f * easedFall, 0f, 15f * easedFall);
            }
            if (leftArm != null) leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, leftArmRest * Quaternion.Euler(-18f, 0f, -72f), easedFall);
            if (rightArm != null) rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, rightArmRest * Quaternion.Euler(12f, 0f, 68f), easedFall);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation, leftLegRest * Quaternion.Euler(30f, 0f, -10f), easedFall);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation, rightLegRest * Quaternion.Euler(22f, 0f, 10f), easedFall);
            if (head != null) head.localRotation = Quaternion.Slerp(head.localRotation, headRest * Quaternion.Euler(32f, 0f, -12f), easedFall);
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
