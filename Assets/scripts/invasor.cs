using UnityEngine;

public class invasor : MonoBehaviour
{
    public static invasor Instance;

    public const int ROWS = 5;

    [Header("Grid Dimensions")]
    [Tooltip("Number of columns (invaders per row)")]
    public int columns = 11;

    [Header("Spacing")]
    [Tooltip("Horizontal distance between invaders")]
    public float spacingX = 1.2f;

    [Tooltip("Vertical distance between rows")]
    public float spacingY = 1.2f;

    [Header("Row Prefabs (5 Fileiras)")]
    [Tooltip("Alien prefab for Row 1 (Top)")]
    public GameObject row1Prefab;

    [Tooltip("Alien prefab for Row 2")]
    public GameObject row2Prefab;

    [Tooltip("Alien prefab for Row 3")]
    public GameObject row3Prefab;

    [Tooltip("Alien prefab for Row 4")]
    public GameObject row4Prefab;

    [Tooltip("Alien prefab for Row 5 (Bottom)")]
    public GameObject row5Prefab;

    [Header("Row Scores (Pontuação por Fileira)")]
    public int row1Score = 30;
    public int row2Score = 20;
    public int row3Score = 20;
    public int row4Score = 10;
    public int row5Score = 10;

    [Header("Explosion Visual")]
    [Tooltip("Explosion prefab instantiated when an alien is destroyed")]
    public GameObject explosionPrefab;

    [Header("Positioning")]
    [Tooltip("If true, centers the grid relative to this object's position")]
    public bool centerGrid = true;

    [Tooltip("Generate automatically when Scene starts")]
    public bool generateOnStart = true;

    [Header("Step Movement Configuration")]
    [Tooltip("Enable step-based movement for the invader grid")]
    public bool enableMovement = true;

    [Tooltip("Time interval in seconds between movement steps")]
    public float stepInterval = 0.5f;

    [Tooltip("Distance in X per step (default 1 unit)")]
    public float stepDistanceX = 1.0f;

    [Tooltip("Total steps in X direction before reversing (default 10)")]
    public int xStepsCount = 10;

    [Tooltip("Units to drop down in Y after completing left/right cycle (default 5 units)")]
    public float dropDistanceY = 5.0f;

    [Header("Score UI")]
    [Tooltip("Optional Canvas UI Text component to display score")]
    public UnityEngine.UI.Text scoreText;

    public int currentScore = 0;

    [Header("Alien Shooting Configuration")]
    [Tooltip("Prefab for the alien bullet / laser")]
    public GameObject alienBulletPrefab;

    [Tooltip("Initial minimum delay in seconds between alien shots (when all aliens are alive)")]
    public float minShootDelay = 5.0f;

    [Tooltip("Initial maximum delay in seconds between alien shots (when all aliens are alive)")]
    public float maxShootDelay = 10.0f;

    [Tooltip("Fastest minimum delay in seconds when 1 alien remains")]
    public float fastestMinShootDelay = 0.5f;

    [Tooltip("Fastest maximum delay in seconds when 1 alien remains")]
    public float fastestMaxShootDelay = 1.5f;

    [Tooltip("Speed of alien bullets moving downwards")]
    public float alienBulletSpeed = 8.0f;

    private float shootTimer = 0f;
    private float nextShootDelay = 5f;

    private enum MovementState
    {
        MovingRightFromCenter,
        MovingLeftToOtherSide,
        MovingRightToCenter
    }

    private MovementState currentState = MovementState.MovingRightFromCenter;
    private float timer = 0f;
    private int currentStepCount = 0;

    [Header("Time-Based Movement Speedup")]
    [Tooltip("Enable movement speedup over time")]
    public bool enableTimeSpeedup = true;

    [Tooltip("Time interval in seconds to apply speedup (default 15 seconds)")]
    public float speedupTimeThreshold = 15.0f;

    [Tooltip("Amount to decrease stepInterval every 15 seconds (default 0.1s)")]
    public float stepIntervalDecreaseAmount = 0.1f;

    [Tooltip("Minimum limit for stepInterval to prevent movement from getting too fast")]
    public float minStepInterval = 0.05f;

    private float speedupTimer = 0f;

    [Header("Mothership Configuration (Nave Mãe)")]
    [Tooltip("Prefab for the Mothership (Nave Mãe)")]
    public GameObject motherShipPrefab;

    [Tooltip("Minimum spawn delay in seconds for Mothership (default 25s)")]
    public float minMotherShipDelay = 25.0f;

    [Tooltip("Maximum spawn delay in seconds for Mothership (default 50s)")]
    public float maxMotherShipDelay = 50.0f;

    [Tooltip("Top-Left spawn X coordinate (default -9)")]
    public float motherShipSpawnX = -9.0f;

    [Tooltip("Top-Left spawn Y coordinate (default 4.0 - visible top of screen)")]
    public float motherShipSpawnY = 4.0f;

    [Tooltip("Mothership movement speed (default 5)")]
    public float motherShipSpeed = 5.0f;

    [Tooltip("Top-Right despawn X coordinate (default 9)")]
    public float motherShipDespawnX = 9.0f;

    private float motherShipTimer = 0f;
    private float nextMotherShipDelay = 25f;

    [Header("Wave Progress & Scaling")]
    public int currentWave = 1;
    public bool isWaveTransition = false;
    private Vector3 initialGridPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        initialGridPosition = transform.position;
        currentWave = 1;
        currentScore = 0;
        isGameOver = false;

        if (generateOnStart)
        {
            GenerateGrid();
        }
        UpdateScoreDisplay();
        ResetShootTimer();
        ResetMotherShipTimer();

        if (motherShipPrefab == null)
        {
            Debug.LogWarning("invasor: motherShipPrefab is NOT assigned in the Inspector! Please drag & drop the Nave Mãe prefab into the slot.");
        }
    }

    public void CheckWaveCleared()
    {
        if (isGameOver || isWaveTransition) return;

        StartCoroutine(VerifyWaveClearedRoutine());
    }

    private System.Collections.IEnumerator VerifyWaveClearedRoutine()
    {
        yield return new WaitForEndOfFrame();

        alien[] remainingAliens = GetComponentsInChildren<alien>();
        if (remainingAliens == null || remainingAliens.Length == 0)
        {
            StartCoroutine(NextWaveRoutine());
        }
    }

    private System.Collections.IEnumerator NextWaveRoutine()
    {
        isWaveTransition = true;

        yield return new WaitForSeconds(0.5f);

        currentWave++;

        // Increase initial movement values by +0.2f for the next wave
        stepDistanceX += 0.2f;
        alienBulletSpeed += 0.2f;

        yield return new WaitForSeconds(3.0f);

        // Reset grid container position to original start and generate fresh wave
        ClearGrid();
        transform.position = initialGridPosition;
        GenerateGrid();

        isWaveTransition = false;
    }

    private void ResetMotherShipTimer()
    {
        motherShipTimer = 0f;
        nextMotherShipDelay = Random.Range(minMotherShipDelay, maxMotherShipDelay);
        Debug.Log($"invasor: Next Mothership spawn in {nextMotherShipDelay:F1} seconds.");
    }

    private void ResetShootTimer()
    {
        shootTimer = 0f;

        // Calculate remaining aliens ratio
        alien[] activeAliens = GetComponentsInChildren<alien>();
        int activeCount = (activeAliens != null) ? activeAliens.Length : 0;
        int totalInitial = Mathf.Max(1, ROWS * columns);

        float ratio = Mathf.Clamp01((float)activeCount / (float)totalInitial);

        // Interpolate delay: as aliens decrease (ratio -> 0), cooldown reduces towards fastest delay
        float currentMinDelay = Mathf.Lerp(fastestMinShootDelay, minShootDelay, ratio);
        float currentMaxDelay = Mathf.Lerp(fastestMaxShootDelay, maxShootDelay, ratio);

        nextShootDelay = Random.Range(currentMinDelay, currentMaxDelay);
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    void Update()
    {
        HandleAlienShooting();
        HandleTimeSpeedup();
        HandleMotherShipSpawning();

        if (!enableMovement) return;

        timer += Time.deltaTime;
        if (timer >= stepInterval)
        {
            timer = 0f;
            ExecuteStep();
        }
    }

    private void HandleMotherShipSpawning()
    {
        if (motherShipPrefab == null) return;

        motherShipTimer += Time.deltaTime;
        if (motherShipTimer >= nextMotherShipDelay)
        {
            ResetMotherShipTimer();
            SpawnMotherShip();
        }
    }

    [ContextMenu("Spawn MotherShip Now")]
    public void SpawnMotherShip()
    {
        if (motherShipPrefab == null)
        {
            Debug.LogWarning("invasor: Cannot spawn Mothership because motherShipPrefab is unassigned in the Inspector!");
            return;
        }

        float spawnY = motherShipSpawnY;
        if (Camera.main != null && Camera.main.orthographic)
        {
            float camTop = Camera.main.transform.position.y + Camera.main.orthographicSize - 0.8f;
            if (spawnY > camTop)
            {
                spawnY = camTop;
            }
        }

        Vector3 spawnPos = new Vector3(motherShipSpawnX, spawnY, transform.position.z);
        GameObject ms = Instantiate(motherShipPrefab, spawnPos, Quaternion.identity);

        nave_mae msComp = ms.GetComponent<nave_mae>();
        if (msComp == null)
        {
            msComp = ms.AddComponent<nave_mae>();
        }
        msComp.speed = motherShipSpeed;
        msComp.maxX = motherShipDespawnX;
        msComp.scoreValue = 1500;
        if (msComp.explosionPrefab == null)
        {
            msComp.explosionPrefab = explosionPrefab;
        }

        Debug.Log($"invasor: Mothership spawned at {spawnPos} with speed {motherShipSpeed}!");
    }

    private void HandleTimeSpeedup()
    {
        if (!enableTimeSpeedup) return;

        speedupTimer += Time.deltaTime;
        if (speedupTimer >= speedupTimeThreshold)
        {
            speedupTimer = 0f;
            stepInterval = Mathf.Max(minStepInterval, stepInterval - stepIntervalDecreaseAmount);
            Debug.Log($"invasor: Aliens speed up! New stepInterval: {stepInterval:F2}s");
        }
    }

    private void HandleAlienShooting()
    {
        if (alienBulletPrefab == null) return;

        shootTimer += Time.deltaTime;
        if (shootTimer >= nextShootDelay)
        {
            ResetShootTimer();
            FireBottomAlienBullet();
        }
    }

    private void FireBottomAlienBullet()
    {
        // Get all active alien components in the grid
        alien[] allAliens = GetComponentsInChildren<alien>();
        if (allAliens == null || allAliens.Length == 0) return;

        // Group by column (rounded X position) to find the bottom-most alien of each column
        System.Collections.Generic.Dictionary<int, alien> bottomAliensByCol = new System.Collections.Generic.Dictionary<int, alien>();

        foreach (alien a in allAliens)
        {
            if (a == null || !a.gameObject.activeInHierarchy) continue;

            int colKey = Mathf.RoundToInt(a.transform.position.x * 10f);

            if (!bottomAliensByCol.ContainsKey(colKey))
            {
                bottomAliensByCol[colKey] = a;
            }
            else
            {
                // Alien with lower Y coordinate is closer to the player
                if (a.transform.position.y < bottomAliensByCol[colKey].transform.position.y)
                {
                    bottomAliensByCol[colKey] = a;
                }
            }
        }

        // Pick a random bottom-most alien from available columns
        var eligibleAliens = new System.Collections.Generic.List<alien>(bottomAliensByCol.Values);
        if (eligibleAliens.Count == 0) return;

        alien shooter = eligibleAliens[Random.Range(0, eligibleAliens.Count)];
        if (shooter != null)
        {
            Vector3 spawnPos = shooter.transform.position + Vector3.down * 0.5f;
            GameObject bulletObj = Instantiate(alienBulletPrefab, spawnPos, Quaternion.identity);

            AlienBullet bulletComp = bulletObj.GetComponent<AlienBullet>();
            if (bulletComp == null)
            {
                bulletComp = bulletObj.AddComponent<AlienBullet>();
            }
            bulletComp.speed = alienBulletSpeed;
        }
    }

    private void ExecuteStep()
    {
        switch (currentState)
        {
            case MovementState.MovingRightFromCenter:
                transform.position += new Vector3(stepDistanceX, 0f, 0f);
                currentStepCount++;

                if (currentStepCount >= xStepsCount)
                {
                    currentState = MovementState.MovingLeftToOtherSide;
                    currentStepCount = 0;
                }
                break;

            case MovementState.MovingLeftToOtherSide:
                transform.position -= new Vector3(stepDistanceX, 0f, 0f);
                currentStepCount++;

                if (currentStepCount >= xStepsCount * 2)
                {
                    currentState = MovementState.MovingRightToCenter;
                    currentStepCount = 0;
                }
                break;

            case MovementState.MovingRightToCenter:
                transform.position += new Vector3(stepDistanceX, 0f, 0f);
                currentStepCount++;

                if (currentStepCount >= xStepsCount)
                {
                    transform.position -= new Vector3(0f, dropDistanceY, 0f);
                    currentState = MovementState.MovingRightFromCenter;
                    currentStepCount = 0;
                }
                break;
        }
    }

    [ContextMenu("Generate Invaders Grid")]
    public void GenerateGrid()
    {
        // Reset movement state
        timer = 0f;
        currentStepCount = 0;
        currentState = MovementState.MovingRightFromCenter;

        GameObject[] rowPrefabs = new GameObject[ROWS] {
            row1Prefab,
            row2Prefab,
            row3Prefab,
            row4Prefab,
            row5Prefab
        };

        int[] rowScores = new int[ROWS] {
            row1Score,
            row2Score,
            row3Score,
            row4Score,
            row5Score
        };

        // Calculate total width and height for centering calculation
        float totalWidth = (columns - 1) * spacingX;
        float totalHeight = (ROWS - 1) * spacingY;

        Vector3 startPos = transform.position;
        if (centerGrid)
        {
            startPos.x -= totalWidth / 2f;
            startPos.y += totalHeight / 2f;
        }

        for (int row = 0; row < ROWS; row++)
        {
            GameObject prefabToSpawn = rowPrefabs[row];

            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"invasor: Prefab for Row {row + 1} is not assigned in the Inspector. Skipping row.");
                continue;
            }

            for (int col = 0; col < columns; col++)
            {
                Vector3 spawnPos = new Vector3(
                    startPos.x + (col * spacingX),
                    startPos.y - (row * spacingY),
                    startPos.z
                );

                GameObject invader = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, transform);
                invader.name = $"Invader_R{row + 1}_C{col + 1}";

                // Assign 'Alien' tag
                try
                {
                    invader.tag = "Alien";
                }
                catch (System.Exception)
                {
                    Debug.LogWarning("invasor: Tag 'Alien' is not registered in Unity Tag Manager.");
                }

                // Attach/configure alien script
                alien alienComp = invader.GetComponent<alien>();
                if (alienComp == null)
                {
                    alienComp = invader.AddComponent<alien>();
                }
                alienComp.row = row + 1;
                alienComp.scoreValue = rowScores[row];
                alienComp.explosionPrefab = explosionPrefab;
            }
        }
    }

    [ContextMenu("Clear Generated Invaders")]
    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    [Header("Game Over & Scene Flow")]
    [Tooltip("Name of the title/menu scene to return to on Game Over (default: scene_01)")]
    public string menuSceneName = "scene_01";

    public bool isGameOver = false;

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Save last score to PlayerPrefs
        PlayerPrefs.SetInt("LastScore", currentScore);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
        }
        PlayerPrefs.Save();

        StartCoroutine(GameOverRoutine());
    }

    private System.Collections.IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(3.5f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
    }

    private void OnGUI()
    {
        // Display Score at bottom right corner if no Canvas UI text is assigned
        if (scoreText == null)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.LowerRight;
            style.normal.textColor = Color.white;

            float margin = 20f;
            Rect rect = new Rect(Screen.width - 220f - margin, Screen.height - 50f - margin, 220f, 50f);
            GUI.Label(rect, "Score: " + currentScore.ToString(), style);
        }

        // Display Wave Transition Banner
        if (isWaveTransition && !isGameOver)
        {
            GUIStyle waveTitleStyle = new GUIStyle(GUI.skin.label);
            waveTitleStyle.fontSize = 36;
            waveTitleStyle.fontStyle = FontStyle.Bold;
            waveTitleStyle.alignment = TextAnchor.MiddleCenter;
            waveTitleStyle.normal.textColor = Color.green;

            GUIStyle scoreStyle = new GUIStyle(GUI.skin.label);
            scoreStyle.fontSize = 24;
            scoreStyle.fontStyle = FontStyle.Bold;
            scoreStyle.alignment = TextAnchor.MiddleCenter;
            scoreStyle.normal.textColor = Color.yellow;

            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.fontSize = 20;
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.normal.textColor = Color.white;

            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;

            GUI.Label(new Rect(centerX - 250f, centerY - 60f, 500f, 50f), "ONDA " + (currentWave - 1) + " CONCLUÍDA!", waveTitleStyle);
            GUI.Label(new Rect(centerX - 250f, centerY, 500f, 40f), "SCORE ATUAL: " + currentScore, scoreStyle);
            GUI.Label(new Rect(centerX - 300f, centerY + 50f, 600f, 30f), "PREPARE-SE PARA A ONDA " + currentWave + " (+0.2 VELOCIDADE)...", subStyle);
        }

        // Display Game Over banner with Total Score when player loses
        if (isGameOver)
        {
            GUIStyle gameOverStyle = new GUIStyle(GUI.skin.label);
            gameOverStyle.fontSize = 42;
            gameOverStyle.fontStyle = FontStyle.Bold;
            gameOverStyle.alignment = TextAnchor.MiddleCenter;
            gameOverStyle.normal.textColor = Color.red;

            GUIStyle scoreStyle = new GUIStyle(GUI.skin.label);
            scoreStyle.fontSize = 26;
            scoreStyle.fontStyle = FontStyle.Bold;
            scoreStyle.alignment = TextAnchor.MiddleCenter;
            scoreStyle.normal.textColor = Color.yellow;

            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.fontSize = 18;
            subStyle.alignment = TextAnchor.MiddleCenter;
            subStyle.normal.textColor = Color.white;

            float centerX = Screen.width / 2f;
            float centerY = Screen.height / 2f;

            GUI.Label(new Rect(centerX - 200f, centerY - 60f, 400f, 50f), "GAME OVER", gameOverStyle);
            GUI.Label(new Rect(centerX - 250f, centerY, 500f, 40f), "PONTUAÇÃO TOTAL: " + currentScore, scoreStyle);
            GUI.Label(new Rect(centerX - 200f, centerY + 50f, 400f, 30f), "Retornando ao Menu...", subStyle);
        }
    }
}
