using UnityEngine;

public class nave_mae : MonoBehaviour
{
    [Header("Mothership Settings")]
    [Tooltip("Movement speed of the mothership (default 5)")]
    public float speed = 5f;

    [Tooltip("X coordinate at top-right where the mothership disappears")]
    public float maxX = 12f;

    [Tooltip("Score points awarded when destroyed (default 1500)")]
    public int scoreValue = 1500;

    [Header("Explosion Visual")]
    [Tooltip("Explosion prefab instantiated on hit")]
    public GameObject explosionPrefab;

    private bool isDestroyed = false;

    void Start()
    {
        // Ensure Tag is assigned
        try
        {
            gameObject.tag = "Alien";
        }
        catch (System.Exception)
        {
            // Fallback tag check
        }

        // Ensure Trigger Collider exists
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true;
        }
    }

    void Update()
    {
        // Move from left to right at speed 5
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Disappear when reaching top-right boundary
        if (transform.position.x >= maxX)
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

    public void CheckCollision(GameObject other)
    {
        if (isDestroyed) return;

        if (other.CompareTag("Bullet") || other.name.ToLower().Contains("bullet"))
        {
            isDestroyed = true;

            // 1. Destroy incoming bullet
            Destroy(other);

            // 2. Award 1500 points to player score
            if (invasor.Instance != null)
            {
                invasor.Instance.AddScore(scoreValue);
            }

            // 3. Spawn explosion effect if set
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 0.5f);
            }

            // 4. Destroy mothership
            Destroy(gameObject);
        }
    }
}
