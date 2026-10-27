using System;
using UnityEngine;

/// <summary>Runs the provisional offline horde and preparation loop.</summary>
public sealed class PrototypeHordeDirector : MonoBehaviour
{
    [SerializeField, Min(0f)] private float firstHordeDelay = 60f;
    [SerializeField, Min(1f)] private float preparationInterval = 180f;
    [SerializeField, Min(1)] private int firstHordeSize = 3;
    [SerializeField, Min(0)] private int extraZombiesPerRound = 2;

    [SerializeField] private PrototypePlayerHealth player;
    [SerializeField] private GameObject zombieTemplate;
    [SerializeField] private PrototypeZombieEnemy[] dormantZombies;
    private bool started;
    private bool hordeActive;
    private float nextHordeAt;
    private int livingZombies;
    private float runStartedAt;

    public int Round { get; private set; }
    public bool HordeActive => hordeActive;
    public int LivingZombies => livingZombies;
    public int ZombiesDefeated { get; private set; }
    public float RunElapsedSeconds => Mathf.Max(0f, Time.time - runStartedAt);
    public float TimeUntilNextHorde => Mathf.Max(0f, nextHordeAt - Time.time);
    public string StatusText
    {
        get
        {
            if (hordeActive) return "RONDA " + Round + "  ·  ZOMBIS " + livingZombies;
            string timer = FormatTime(TimeUntilNextHorde);
            return Round == 0 ? "PREPÁRATE  ·  PRIMERA HORDA EN " + timer : "RONDA " + Round + " COMPLETA  ·  SIGUIENTE EN " + timer;
        }
    }

    public void Initialize(PrototypePlayerHealth playerHealth, GameObject template)
    {
        player = playerHealth;
        zombieTemplate = template;
    }

    private void Awake()
    {
        runStartedAt = Time.time;
    }

    public void SetDormantZombies(PrototypeZombieEnemy[] zombies) => dormantZombies = zombies;

    private void Update()
    {
        if (player == null || !player.IsAlive || zombieTemplate == null) return;
        if (!started)
        {
            started = true;
            nextHordeAt = Time.time + firstHordeDelay;
        }

        if (!hordeActive && Time.time >= nextHordeAt) StartNextHorde();
    }

    private void StartNextHorde()
    {
        Round++;
        hordeActive = true;
        int count = firstHordeSize + (Round - 1) * extraZombiesPerRound;
        float healthMultiplier = 1f + (Round - 1) * 0.14f;
        float speedMultiplier = 1f + (Round - 1) * 0.035f;

        for (int i = 0; i < count; i++)
        {
            float angle = (Mathf.PI * 2f * i / count) + UnityEngine.Random.Range(-0.22f, 0.22f);
            float radius = UnityEngine.Random.Range(11f, 16f);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            PrototypeZombieEnemy enemy = Round == 1 && dormantZombies != null && i < dormantZombies.Length
                ? dormantZombies[i]
                : null;
            GameObject zombie;
            if (enemy != null)
            {
                zombie = enemy.gameObject;
                zombie.transform.SetPositionAndRotation(player.transform.position + offset, Quaternion.identity);
                zombie.SetActive(true);
                enemy.enabled = true;
            }
            else
            {
                zombie = Instantiate(zombieTemplate, player.transform.position + offset, Quaternion.identity);
                zombie.SetActive(true);
                enemy = zombie.GetComponent<PrototypeZombieEnemy>();
            }
            zombie.name = "Zombie_Ronda_" + Round.ToString("00") + "_" + (i + 1).ToString("00");
            enemy.Initialize(player, zombie.transform.childCount > 0 ? zombie.transform.GetChild(0) : null,
                healthMultiplier, speedMultiplier);
            enemy.Defeated += OnZombieDefeated;
        }
        livingZombies = count;
    }

    private void OnZombieDefeated(PrototypeZombieEnemy enemy)
    {
        enemy.Defeated -= OnZombieDefeated;
        ZombiesDefeated++;
        livingZombies = Mathf.Max(0, livingZombies - 1);
        if (livingZombies > 0) return;
        hordeActive = false;
        nextHordeAt = Time.time + preparationInterval;
    }

    private void OnDestroy()
    {
        foreach (PrototypeZombieEnemy enemy in FindObjectsByType<PrototypeZombieEnemy>())
            if (enemy != null) enemy.Defeated -= OnZombieDefeated;
    }

    private static string FormatTime(float seconds)
    {
        int whole = Mathf.CeilToInt(seconds);
        return (whole / 60).ToString("00") + ":" + (whole % 60).ToString("00");
    }
}
