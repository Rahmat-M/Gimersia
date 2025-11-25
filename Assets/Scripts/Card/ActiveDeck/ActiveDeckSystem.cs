using System.Collections;
using UnityEngine;

public class ActiveDeckSystem : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public PlayerCombat playerCombat;
    public Transform playerTransform;

    [Header("Active Card Settings")]
    [Tooltip("Starting active card (ultimate)")]
    public CardData startingActiveCard;

    [Tooltip("Delay sebelum active card muncul saat game start")]
    public float activeCardSpawnDelay = 2.5f;

    [Header("Input Settings")]
    [Tooltip("Key untuk activate ultimate")]
    public KeyCode activateKey = KeyCode.Space;

    [Header("Visual Effects")]
    public GameObject areaIndicatorPrefab; // Circle indicator untuk area skill
    private GameObject currentAreaIndicator;

    // Active card slot (hanya 1)
    private CardData activeCard;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private bool isReady = false;

    // Channeling/Continuous skill state
    private bool isChanneling = false;
    private float channelingTimer = 0f;
    private Coroutine channelingCoroutine;

    // Events
    public delegate void ActiveCardDrawnHandler(CardData card);
    public event ActiveCardDrawnHandler OnActiveCardDrawn;

    public delegate void ActiveCardUsedHandler(CardData card);
    public event ActiveCardUsedHandler OnActiveCardUsed;

    public delegate void ActiveCardCooldownUpdateHandler(float remainingTime, float totalCooldown);
    public event ActiveCardCooldownUpdateHandler OnCooldownUpdate;

    public delegate void ActiveCardReadyHandler();
    public event ActiveCardReadyHandler OnActiveCardReady;

    public delegate void ChannelingStartedHandler(CardData card, float duration);
    public event ChannelingStartedHandler OnChannelingStarted;

    public delegate void ChannelingUpdateHandler(float remainingTime, float totalDuration);
    public event ChannelingUpdateHandler OnChannelingUpdate;

    public delegate void ChannelingEndedHandler();
    public event ChannelingEndedHandler OnChannelingEnded;

    void Start()
    {
        // Spawn starting active card dengan delay
        if (startingActiveCard != null)
        {
            StartCoroutine(SpawnStartingActiveCard());
        }
        else
        {
            Debug.LogWarning("[ActiveDeck] No starting active card assigned!");
        }
    }

    void Update()
    {
        // Update cooldown
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            OnCooldownUpdate?.Invoke(cooldownTimer, activeCard.cooldown);

            if (cooldownTimer <= 0)
            {
                isOnCooldown = false;
                isReady = true;
                Debug.Log($"[ActiveDeck] {activeCard.cardName} ready!");
                OnActiveCardReady?.Invoke();
            }
        }

        // Update channeling
        if (isChanneling)
        {
            channelingTimer -= Time.deltaTime;
            OnChannelingUpdate?.Invoke(channelingTimer, activeCard.skillDuration);

            if (channelingTimer <= 0)
            {
                StopChanneling();
            }
        }

        // Check input untuk activate
        if (Input.GetKeyDown(activateKey) && CanUseActiveCard())
        {
            UseActiveCard();
        }

        // Show/hide area indicator (untuk area skills)
        UpdateAreaIndicator();
    }

    IEnumerator SpawnStartingActiveCard()
    {
        Debug.Log($"[ActiveDeck] Waiting {activeCardSpawnDelay}s before spawning active card...");
        yield return new WaitForSeconds(activeCardSpawnDelay);

        if (startingActiveCard == null || !startingActiveCard.isActiveCard)
        {
            Debug.LogWarning("[ActiveDeck] Starting active card is invalid!");
            yield break;
        }

        Debug.Log($"[ActiveDeck] Spawning active card: {startingActiveCard.cardName}");
        activeCard = startingActiveCard;
        isReady = true;

        // Trigger event untuk animasi draw
        OnActiveCardDrawn?.Invoke(activeCard);
    }

    bool CanUseActiveCard()
    {
        if (activeCard == null)
        {
            Debug.Log("[ActiveDeck] No active card equipped!");
            return false;
        }

        if (!isReady)
        {
            Debug.Log("[ActiveDeck] Active card not ready yet!");
            return false;
        }

        if (isOnCooldown)
        {
            Debug.Log($"[ActiveDeck] Active card on cooldown: {cooldownTimer:F1}s remaining");
            return false;
        }

        // Optional: Check mana/energy cost
        if (activeCard.manaCost > 0)
        {
            // Implement mana system check here
            // return playerMana >= activeCard.manaCost;
        }

        return true;
    }

    public void UseActiveCard()
    {
        if (!CanUseActiveCard()) return;

        Debug.Log($"[ActiveDeck] Using active card: {activeCard.cardName}");

        // Check if continuous skill
        if (activeCard.isContinuousSkill && activeCard.skillDuration > 0)
        {
            // Start channeling
            StartChanneling();
        }
        else
        {
            // Execute instant skill
            ExecuteActiveSkill();
        }

        // Set cooldown
        isOnCooldown = true;
        cooldownTimer = activeCard.cooldown;

        // Trigger event
        OnActiveCardUsed?.Invoke(activeCard);
    }

    void StartChanneling()
    {
        isChanneling = true;
        channelingTimer = activeCard.skillDuration;

        Debug.Log($"[ActiveDeck] Starting channeling: {activeCard.cardName} for {activeCard.skillDuration}s");
        OnChannelingStarted?.Invoke(activeCard, activeCard.skillDuration);

        // Start continuous effect coroutine
        if (channelingCoroutine != null)
            StopCoroutine(channelingCoroutine);

        channelingCoroutine = StartCoroutine(ChannelingLoop());
    }

    void StopChanneling()
    {
        if (!isChanneling) return;

        isChanneling = false;
        channelingTimer = 0f;

        Debug.Log($"[ActiveDeck] Channeling ended: {activeCard.cardName}");
        OnChannelingEnded?.Invoke();

        if (channelingCoroutine != null)
        {
            StopCoroutine(channelingCoroutine);
            channelingCoroutine = null;
        }
    }

    IEnumerator ChannelingLoop()
    {
        float elapsed = 0f;

        while (elapsed < activeCard.skillDuration)
        {
            // Execute skill effect
            ExecuteActiveSkill();

            // Wait for next spawn
            yield return new WaitForSeconds(activeCard.spawnInterval);

            elapsed += activeCard.spawnInterval;
        }

        // Channeling complete
        StopChanneling();
    }

    void ExecuteActiveSkill()
    {
        switch (activeCard.activeSkillType)
        {
            case ActiveSkillType.AreaDamage:
                ExecuteAreaDamage();
                break;

            case ActiveSkillType.BurstDamage:
                ExecuteBurstDamage();
                break;

            case ActiveSkillType.MultiShot:
                ExecuteMultiShot();
                break;

            case ActiveSkillType.Buff:
                ExecuteBuff();
                break;

            case ActiveSkillType.Heal:
                ExecuteHeal();
                break;

            case ActiveSkillType.TimeSlow:
                ExecuteTimeSlow();
                break;

            case ActiveSkillType.Teleport:
                ExecuteTeleport();
                break;
        }
    }

    void ExecuteAreaDamage()
    {
        // Random position dalam radius untuk continuous skill
        Vector3 targetPos = playerTransform.position;

        if (activeCard.isContinuousSkill)
        {
            // Random spawn position dalam radius
            Vector2 randomOffset = Random.insideUnitCircle * activeCard.skillRange;
            targetPos += new Vector3(randomOffset.x, randomOffset.y, 0);
        }

        Debug.Log($"[ActiveDeck] Area Damage: {activeCard.damage} at {targetPos}");

        // Spawn visual effect
        if (activeCard.skillEffectPrefab != null)
        {
            GameObject effect = Instantiate(activeCard.skillEffectPrefab, targetPos, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // Find all enemies in range (untuk instant skill, gunakan radius penuh)
        float damageRadius = activeCard.isContinuousSkill ? 2f : activeCard.skillRange;
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, damageRadius);

        foreach (var hit in hits)
        {
            // Check if enemy (adjust tag sesuai project Anda)
            if (hit.CompareTag("Enemy"))
            {
                // Apply damage to enemy
                var enemy = hit.GetComponent<EnemyHealth>(); // Sesuaikan dengan script enemy Anda
                if (enemy != null)
                {
                    // enemy.TakeDamage(activeCard.damage);
                    Debug.Log($"[ActiveDeck] Hit enemy: {hit.name} for {activeCard.damage} damage");
                }
            }
        }
    }

    void ExecuteBurstDamage()
    {
        Debug.Log($"[ActiveDeck] Burst Damage: {activeCard.damage}");

        // Get nearest enemy atau mouse position
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPos.z = 0;

        // Spawn effect
        if (activeCard.skillEffectPrefab != null)
        {
            GameObject effect = Instantiate(activeCard.skillEffectPrefab, targetPos, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Find closest enemy to mouse
        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, 1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log($"[ActiveDeck] Burst hit: {hit.name}");
                // Apply massive damage
                break; // Only hit one enemy
            }
        }
    }

    void ExecuteMultiShot()
    {
        Debug.Log($"[ActiveDeck] Multi Shot: {activeCard.multiHitCount} projectiles");

        if (activeCard.skillProjectilePrefab == null)
        {
            Debug.LogWarning("[ActiveDeck] No projectile prefab assigned!");
            return;
        }

        // Spawn multiple projectiles in a spread pattern
        float angleStep = 360f / activeCard.multiHitCount;

        for (int i = 0; i < activeCard.multiHitCount; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject proj = Instantiate(activeCard.skillProjectilePrefab, playerTransform.position, Quaternion.identity);
            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetDirection(direction);
                projectile.SetDamage(activeCard.damage);
            }
        }
    }

    void ExecuteBuff()
    {
        Debug.Log($"[ActiveDeck] Buff applied for {activeCard.effectDuration}s");
        StartCoroutine(ApplyBuffCoroutine());
    }

    IEnumerator ApplyBuffCoroutine()
    {
        // Implement buff logic (speed boost, damage boost, invincibility, etc)
        // Example: increase player speed
        var movement = playerTransform.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            float originalSpeed = movement.moveSpeed;
            movement.moveSpeed *= 2f; // Double speed

            Debug.Log("[ActiveDeck] Speed doubled!");
            yield return new WaitForSeconds(activeCard.effectDuration);

            movement.moveSpeed = originalSpeed;
            Debug.Log("[ActiveDeck] Buff expired");
        }
    }

    void ExecuteHeal()
    {
        Debug.Log($"[ActiveDeck] Heal: {activeCard.healAmount} HP");

        if (playerHealth != null)
        {
            playerHealth.Heal(activeCard.healAmount);
        }

        // Spawn heal effect
        if (activeCard.skillEffectPrefab != null)
        {
            GameObject effect = Instantiate(activeCard.skillEffectPrefab, playerTransform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    void ExecuteTimeSlow()
    {
        Debug.Log($"[ActiveDeck] Time Slow for {activeCard.effectDuration}s");
        StartCoroutine(TimeSlowCoroutine());
    }

    IEnumerator TimeSlowCoroutine()
    {
        Time.timeScale = 0.5f; // Slow time to 50%
        yield return new WaitForSecondsRealtime(activeCard.effectDuration);
        Time.timeScale = 1f; // Back to normal
        Debug.Log("[ActiveDeck] Time back to normal");
    }

    void ExecuteTeleport()
    {
        Debug.Log("[ActiveDeck] Teleport/Dash");

        // Get mouse position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // Teleport to mouse (atau dash ke arah mouse)
        Vector2 direction = (mousePos - playerTransform.position).normalized;
        Vector3 teleportPos = playerTransform.position + (Vector3)(direction * activeCard.skillRange);

        playerTransform.position = teleportPos;

        // Spawn teleport effects
        if (activeCard.skillEffectPrefab != null)
        {
            Instantiate(activeCard.skillEffectPrefab, playerTransform.position, Quaternion.identity);
        }
    }

    void UpdateAreaIndicator()
    {
        // Show area indicator when hovering (untuk area skills)
        if (activeCard != null &&
            (activeCard.activeSkillType == ActiveSkillType.AreaDamage) &&
            CanUseActiveCard() &&
            areaIndicatorPrefab != null)
        {
            if (currentAreaIndicator == null)
            {
                currentAreaIndicator = Instantiate(areaIndicatorPrefab, playerTransform);
                currentAreaIndicator.transform.localPosition = Vector3.zero;
            }

            // Update indicator size
            float scale = activeCard.skillRange * 2f;
            currentAreaIndicator.transform.localScale = new Vector3(scale, scale, 1f);

            // Show indicator hanya saat hold key (optional)
            currentAreaIndicator.SetActive(Input.GetKey(activateKey));
        }
        else
        {
            if (currentAreaIndicator != null)
            {
                currentAreaIndicator.SetActive(false);
            }
        }
    }

    // Replace active card (untuk upgrade system nanti)
    public void ReplaceActiveCard(CardData newCard)
    {
        if (!newCard.isActiveCard)
        {
            Debug.LogWarning($"[ActiveDeck] {newCard.cardName} is not an active card!");
            return;
        }

        CardData oldCard = activeCard;
        activeCard = newCard;
        isOnCooldown = false;
        cooldownTimer = 0;
        isReady = true;

        Debug.Log($"[ActiveDeck] Replaced {oldCard?.cardName} with {newCard.cardName}");
    }

    // Getters
    public CardData GetActiveCard() => activeCard;
    public bool IsOnCooldown() => isOnCooldown;
    public float GetCooldownRemaining() => cooldownTimer;
    public bool IsReady() => isReady && !isOnCooldown;
    public bool IsChanneling() => isChanneling;
    public float GetChannelingRemaining() => channelingTimer;
}