using UnityEngine;

public class alien : MonoBehaviour
{
    [Header("Alien Score")]
    [Tooltip("Points awarded when this alien is destroyed")]
    public int scoreValue = 10;

    [Header("Explosion Effect")]
    [Tooltip("Explosion prefab instantiated upon destruction")]
    public GameObject explosionPrefab;

    [HideInInspector]
    public int row = 0;

    private bool isDestroyed = false;

    void Start()
    {
        // Ensure Alien tag is assigned
        try
        {
            gameObject.tag = "Alien";
        }
        catch (System.Exception)
        {
            Debug.LogWarning("alien: Tag 'Alien' is not registered in Unity Tag Manager.");
        }

        // Ensure collider exists for trigger detection
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true;
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

        if (other.CompareTag("Bullet") || other.name.Contains("bullet") || other.name.Contains("Bullet"))
        {
            isDestroyed = true;

            // 1. Destroy the bullet
            Destroy(other);

            // 2. Add score to the invasor manager
            if (invasor.Instance != null)
            {
                invasor.Instance.AddScore(scoreValue);
                invasor.Instance.CheckWaveCleared();
            }

            // 3. Spawn explosion animation prefab if available
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 0.5f); // Destroy explosion after 0.5 seconds
            }

            // 4. Destroy this alien
            Destroy(gameObject);
        }
    }
}
