using System;
using UnityEngine;

/// <summary>Simple chasing enemy for the offline combat prototype.</summary>
[RequireComponent(typeof(CharacterController))]
public sealed class PrototypeZombieEnemy : MonoBehaviour
{
    [SerializeField] private float maximumHealth = 70f;
    [SerializeField] private float moveSpeed = 1.45f;
    [SerializeField] private float attackDistance = 1.45f;
    [SerializeField] private float attackDamage = 12f;
    [SerializeField] private float attackInterval = 1.3f;

    private CharacterController controller;
    private PrototypePlayerHealth target;
    private Transform visual;
    private Vector3 visualStart;
    private float health;
    private float nextAttackTime;
    private float verticalSpeed;
    private bool defeated;

    public event Action<PrototypeZombieEnemy> Defeated;
    public bool IsDefeated => defeated;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = maximumHealth;
        if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);
        if (visual != null) visualStart = visual.localPosition;
        if (target == null)
        {
            PrototypePlayerHealth player = FindAnyObjectByType<PrototypePlayerHealth>();
            if (player != null) target = player;
        }
    }

    public void Initialize(PrototypePlayerHealth player, Transform visualRoot)
    {
        target = player;
        visual = visualRoot;
        if (visual != null) visualStart = visual.localPosition;
    }

    private void Update()
    {
        if (defeated || target == null || !target.IsAlive) return;

        Vector3 towardPlayer = target.transform.position - transform.position;
        towardPlayer.y = 0f;
        float distance = towardPlayer.magnitude;
        Vector3 direction = distance > 0.001f ? towardPlayer / distance : Vector3.zero;
        bool moving = distance > attackDistance;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 8f * Time.deltaTime);
        }

        if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
        else verticalSpeed += Physics.gravity.y * Time.deltaTime;

        Vector3 velocity = (moving ? direction * moveSpeed : Vector3.zero) + Vector3.up * verticalSpeed;
        controller.Move(velocity * Time.deltaTime);

        if (visual != null)
        {
            float bob = moving ? Mathf.Abs(Mathf.Sin(Time.time * 5f)) * 0.075f : Mathf.Sin(Time.time * 1.5f) * 0.018f;
            visual.localPosition = visualStart + Vector3.up * bob;
        }

        if (!moving && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackInterval;
            target.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        if (defeated || damage <= 0f) return;
        health = Mathf.Max(0f, health - damage);
        if (health > 0f) return;

        defeated = true;
        controller.enabled = false;
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) renderer.enabled = false;
        PrototypeResourceType dropType = UnityEngine.Random.value < 0.5f
            ? PrototypeResourceType.Wood
            : PrototypeResourceType.Scrap;
        PrototypeResourcePickup.Spawn(transform.position, dropType, UnityEngine.Random.Range(1, 4));
        Defeated?.Invoke(this);
        Destroy(gameObject, 0.2f);
    }
}
