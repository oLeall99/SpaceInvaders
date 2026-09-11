using UnityEngine;

public class AlienBullet : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of the alien bullet moving downwards")]
    public float speed = 8f;

    [Tooltip("Y coordinate threshold below camera where bullet is destroyed")]
    public float minY = -15f;

    private bool hasCollided = false;

    void Start()
    {
        // Ensure bullet has a 2D Trigger Collider
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        // Ensure Kinematic Rigidbody2D for triggers
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }

        // Set Tag
        try
        {
            gameObject.tag = "AlienBullet";
        }
        catch (System.Exception)
        {
            // Tag may not be defined in Tag Manager
        }
    }

    void Update()
    {
        // Move downwards
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // Destroy bullet if it passes below the bottom of the camera
        if (transform.position.y <= minY)
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
        if (hasCollided) return;

        // 1. Check if collision is with Base
        if (other.CompareTag("Base") || other.name.ToLower().Contains("base"))
        {
            base_bunker bunker = other.GetComponent<base_bunker>();
            if (bunker != null)
            {
                hasCollided = true;
                bunker.TakeDamage(1);
                Destroy(gameObject);
            }
        }
        // 2. Check if collision is with Player
        else if (other.CompareTag("Player") || other.name.ToLower().Contains("player"))
        {
            player playerComp = other.GetComponent<player>();
            if (playerComp != null)
            {
                hasCollided = true;
                playerComp.TakeDamage(1);
                Destroy(gameObject);
            }
        }
    }
}
