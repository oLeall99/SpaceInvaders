using UnityEngine;

public class base_bunker : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health/lives of the base (default 4)")]
    public int maxHealth = 4;
    private int currentHealth;

    [Header("Visual Damage Settings (Option 1: Sprites)")]
    [Tooltip("Assign 4 sprites ordered from intact to heavily damaged (Index 0 = Full Health, Index 3 = 1 HP)")]
    public Sprite[] damageSprites;

    [Header("Visual Damage Settings (Option 2: Animator)")]
    [Tooltip("Name of the state in Animator Controller if using Animator frame control")]
    public string animationStateName = "";

    [Header("Explosion Effect")]
    [Tooltip("Optional effect spawned when the base is completely destroyed")]
    public GameObject destructionEffect;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isDestroyed = false;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Automatically assign 'Base' tag
        try
        {
            gameObject.tag = "Base";
        }
        catch (System.Exception)
        {
            Debug.LogWarning("base_bunker: Tag 'Base' is not registered in Unity Tag Manager.");
        }

        // If using sprite array, disable Animator component so it doesn't overwrite spriteRenderer.sprite
        if (damageSprites != null && damageSprites.Length > 0 && animator != null)
        {
            animator.enabled = false;
        }

        // Ensure collider exists for trigger/physics collision
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true;
        }

        // Ensure a Kinematic Rigidbody2D exists so 2D trigger events are guaranteed to fire in Unity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }

        UpdateVisuals();
    }

    private System.Collections.Generic.HashSet<GameObject> processedBullets = new System.Collections.Generic.HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleBulletCollision(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleBulletCollision(collision.gameObject);
    }

    public void HandleBulletCollision(GameObject other)
    {
        if (isDestroyed || other == null) return;

        // Check if collision is with a Bullet
        if (other.CompareTag("Bullet") || other.name.ToLower().Contains("bullet"))
        {
            if (processedBullets.Contains(other)) return;
            processedBullets.Add(other);

            // Destroy the incoming bullet
            Destroy(other);

            // Apply 1 point of damage
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            isDestroyed = true;

            // Destroy bunker
            if (destructionEffect != null)
            {
                GameObject effect = Instantiate(destructionEffect, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f);
            }
            Destroy(gameObject);
        }
        else
        {
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        int damageStage = maxHealth - currentHealth; // 0 = Full, 1 = 3HP, 2 = 2HP, 3 = 1HP

        // Method 1: Swap Sprites from array
        if (spriteRenderer != null && damageSprites != null && damageSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(damageStage, 0, damageSprites.Length - 1);
            if (damageSprites[spriteIndex] != null)
            {
                spriteRenderer.sprite = damageSprites[spriteIndex];
            }
        }

        // Method 2: Update Animator parameters and frame position if animator is enabled
        if (animator != null && animator.enabled)
        {
            animator.SetInteger("Health", currentHealth);
            animator.SetInteger("DamageStage", damageStage);

            if (!string.IsNullOrEmpty(animationStateName))
            {
                float normalizedTime = (float)damageStage / Mathf.Max(1, maxHealth - 1);
                animator.Play(animationStateName, 0, normalizedTime);
            }
        }
    }
}
