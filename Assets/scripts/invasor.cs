using UnityEngine;

public class invasor : MonoBehaviour
{
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

    private enum MovementState
    {
        MovingRightFromCenter,
        MovingLeftToOtherSide,
        MovingRightToCenter
    }

    private MovementState currentState = MovementState.MovingRightFromCenter;
    private float timer = 0f;
    private int currentStepCount = 0;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateGrid();
        }
    }

    void Update()
    {
        if (!enableMovement) return;

        timer += Time.deltaTime;
        if (timer >= stepInterval)
        {
            timer = 0f;
            ExecuteStep();
        }
    }

    private void ExecuteStep()
    {
        switch (currentState)
        {
            case MovementState.MovingRightFromCenter:
                // Move right 1 step until completing xStepsCount (10 steps)
                transform.position += new Vector3(stepDistanceX, 0f, 0f);
                currentStepCount++;

                if (currentStepCount >= xStepsCount)
                {
                    currentState = MovementState.MovingLeftToOtherSide;
                    currentStepCount = 0;
                }
                break;

            case MovementState.MovingLeftToOtherSide:
                // Move left 1 step back to origin (10 steps) and to the other side (10 steps) -> total 20 steps
                transform.position -= new Vector3(stepDistanceX, 0f, 0f);
                currentStepCount++;

                if (currentStepCount >= xStepsCount * 2)
                {
                    currentState = MovementState.MovingRightToCenter;
                    currentStepCount = 0;
                }
                break;

            case MovementState.MovingRightToCenter:
                // Move right 1 step back to origin (10 steps)
                transform.position += new Vector3(stepDistanceX, 0f, 0f);
                currentStepCount++;

                if (currentStepCount >= xStepsCount)
                {
                    // Reached original point, drop Y by dropDistanceY (5 units) and restart cycle
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
}
