using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class player : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed of the player")]
    public float speed = 7f;

    [Tooltip("Enable horizontal screen boundaries")]
    public bool useBoundaries = true;

    [Tooltip("Minimum X position (left boundary)")]
    public float minX = -8f;

    [Tooltip("Maximum X position (right boundary)")]
    public float maxX = 8f;

    [Header("Shooting Settings")]
    [Tooltip("Bullet / Laser prefab to instantiate when pressing Space")]
    public GameObject bulletPrefab;

    [Tooltip("Optional spawn point for the bullet. If unassigned, uses player position.")]
    public Transform firePoint;

    [Tooltip("Speed of the bullet going upwards")]
    public float bulletSpeed = 12f;

    [Tooltip("Cooldown time between shots in seconds")]
    public float fireCooldown = 0.4f;

    [Header("Player Health & Lives")]
    [Tooltip("Maximum lives of the player (default 3)")]
    public int maxLives = 3;
    public int currentLives = 3;

    [Header("Lives Display Settings")]
    [Tooltip("Sprite used for the ship life icons. If left empty, uses the Player's SpriteRenderer sprite.")]
    public Sprite lifeIconSprite;

    [Tooltip("Scale factor for the ship life icons (default 0.75, 0.75)")]
    public Vector2 iconScale = new Vector2(0.75f, 0.75f);

    [Tooltip("Optional Canvas UI Images (3 elements) representing player lives")]
    public UnityEngine.UI.Image[] lifeUIImages;

    [Tooltip("Color of active/remaining lives (default White/Normal)")]
    public Color activeLifeColor = Color.white;

    [Tooltip("Color of lost lives (default Black)")]
    public Color lostLifeColor = Color.black;

    private float nextFireTime = 0f;

    void Start()
    {
        currentLives = maxLives;

        // Automatically assign 'Player' tag
        try
        {
            gameObject.tag = "Player";
        }
        catch (System.Exception)
        {
            Debug.LogWarning("player: Tag 'Player' is not registered in Unity Tag Manager.");
        }

        UpdateLivesUI();
    }

    public void TakeDamage(int damage)
    {
        currentLives -= damage;
        currentLives = Mathf.Max(0, currentLives);

        UpdateLivesUI();

        Debug.Log($"Player took damage! Remaining lives: {currentLives}/{maxLives}");

        if (currentLives <= 0)
        {
            Debug.Log("Player lost all lives! Game Over.");
            Destroy(gameObject);
        }
    }

    private void UpdateLivesUI()
    {
        Sprite currentSprite = lifeIconSprite;
        if (currentSprite == null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) currentSprite = sr.sprite;
        }

        if (lifeUIImages != null && lifeUIImages.Length > 0)
        {
            for (int i = 0; i < lifeUIImages.Length; i++)
            {
                if (lifeUIImages[i] != null)
                {
                    if (currentSprite != null)
                    {
                        lifeUIImages[i].sprite = currentSprite;
                    }
                    lifeUIImages[i].rectTransform.localScale = new Vector3(iconScale.x, iconScale.y, 1f);
                    lifeUIImages[i].color = (i < currentLives) ? activeLifeColor : lostLifeColor;
                }
            }
        }
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        float moveInput = 0f;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                moveInput -= 1f;
            }
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                moveInput += 1f;
            }
        }
#else
        // Legacy Input Manager fallback
        moveInput = Input.GetAxisRaw("Horizontal");
#endif

        Vector3 pos = transform.position;
        pos.x += moveInput * speed * Time.deltaTime;

        if (useBoundaries)
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
        }

        transform.position = pos;
    }

    private void HandleShooting()
    {
        bool spacePressed = false;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            spacePressed = keyboard.spaceKey.wasPressedThisFrame;
        }
#else
        spacePressed = Input.GetKeyDown(KeyCode.Space);
#endif

        if (spacePressed && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("player: Please assign bulletPrefab in the Inspector!");
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        // Assign 'Bullet' tag to the missile
        try
        {
            bullet.tag = "Bullet";
        }
        catch (System.Exception)
        {
            Debug.LogWarning("player: Tag 'Bullet' is not registered in Unity Tag Manager.");
        }

        // Attach movement helper to ensure the bullet moves upwards automatically
        PlayerBullet bulletMovement = bullet.GetComponent<PlayerBullet>();
        if (bulletMovement == null)
        {
            bulletMovement = bullet.AddComponent<PlayerBullet>();
        }
        bulletMovement.speed = bulletSpeed;
    }

    private void OnGUI()
    {
        // Render OnGUI lives display if Canvas UI Images are unassigned
        if (lifeUIImages == null || lifeUIImages.Length == 0)
        {
            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 20;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.normal.textColor = Color.white;
            textStyle.alignment = TextAnchor.MiddleLeft;

            float margin = 20f;
            float startY = Screen.height - 45f;

            // Draw "vidas:" label
            GUI.Label(new Rect(margin, startY, 70f, 30f), "vidas:", textStyle);

            // Fetch target Sprite from lifeIconSprite or Player SpriteRenderer
            Sprite targetSprite = lifeIconSprite;
            if (targetSprite == null)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null) targetSprite = sr.sprite;
            }

            if (targetSprite != null)
            {
                // Calculate dimensions based on actual sprite rect scaled by iconScale (0.75, 0.75)
                float iconWidth = targetSprite.rect.width * iconScale.x;
                float iconHeight = targetSprite.rect.height * iconScale.y;
                float spacing = 8f;
                float iconX = margin + 70f;

                for (int i = 0; i < maxLives; i++)
                {
                    Color col = (i < currentLives) ? activeLifeColor : lostLifeColor;
                    Rect iconRect = new Rect(iconX + i * (iconWidth + spacing), startY + (30f - iconHeight) / 2f, iconWidth, iconHeight);
                    DrawSpriteGUI(iconRect, targetSprite, col);
                }
            }
        }
    }

    private void DrawSpriteGUI(Rect position, Sprite sprite, Color color)
    {
        if (sprite == null || sprite.texture == null) return;

        Texture2D tex = sprite.texture;
        Rect rect = sprite.textureRect;
        Rect uv = new Rect(
            rect.x / tex.width,
            rect.y / tex.height,
            rect.width / tex.width,
            rect.height / tex.height
        );

        Color prevColor = GUI.color;
        GUI.color = color;
        GUI.DrawTextureWithTexCoords(position, tex, uv, true);
        GUI.color = prevColor;
    }
}

public class PlayerBullet : MonoBehaviour
{
    public float speed = 12f;
    public float maxY = 15f;

    void Start()
    {
        // Ensure bullet has a collider and Kinematic Rigidbody2D for physics triggers
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Update()
    {
        // Move bullet upwards
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Destroy bullet when out of screen bounds
        if (transform.position.y >= maxY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckCollision(collision.gameObject);
    }

    private void CheckCollision(GameObject other)
    {
        if (other.CompareTag("Base") || other.name.ToLower().Contains("base"))
        {
            // Destroy the bullet on base collision (base_bunker handles its own 1 HP damage deduction)
            Destroy(gameObject);
        }
    }
}
