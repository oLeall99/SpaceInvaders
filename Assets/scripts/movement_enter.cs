using System.Collections;
using UnityEngine;

public class movement_enter : MonoBehaviour
{
    public enum MovementAxis
    {
        Horizontal,
        Vertical
    }

    [Header("Movement Axis")]
    [Tooltip("Choose whether the movement is Horizontal (X) or Vertical (Y)")]
    public MovementAxis axis = MovementAxis.Horizontal;

    [Header("Position Limits")]
    [Tooltip("Starting coordinate on the chosen axis")]
    public float startPos = 0f;

    [Tooltip("Ending coordinate on the chosen axis")]
    public float endPos = 10f;

    [Header("Movement Configuration")]
    [Tooltip("Movement speed")]
    public float speed = 5f;

    [Tooltip("Fixed delay in seconds when reaching an endpoint before returning")]
    public float delay = 0.5f;

    [Header("Random Delay Settings")]
    [Tooltip("Enable to use a random delay between minDelay and maxDelay instead of fixed delay")]
    public bool useRandomDelay = false;

    [Tooltip("Minimum random delay in seconds")]
    public float minDelay = 0.5f;

    [Tooltip("Maximum random delay in seconds")]
    public float maxDelay = 2.0f;

    [Tooltip("If true, sets startPos to the object's current position at Start")]
    public bool useInitialPositionAsStart = false;

    private bool movingToEnd = true;
    private bool isWaiting = false;

    void Start()
    {
        Vector3 pos = transform.position;
        if (useInitialPositionAsStart)
        {
            startPos = (axis == MovementAxis.Horizontal) ? pos.x : pos.y;
        }

        // Set initial position on the active axis
        if (axis == MovementAxis.Horizontal)
        {
            pos.x = startPos;
        }
        else
        {
            pos.y = startPos;
        }
        transform.position = pos;
    }

    void Update()
    {
        if (isWaiting) return;

        Vector3 targetPosition = transform.position;
        float targetValue = movingToEnd ? endPos : startPos;

        if (axis == MovementAxis.Horizontal)
        {
            targetPosition.x = targetValue;
        }
        else
        {
            targetPosition.y = targetValue;
        }

        // Move smoothly towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Check if destination is reached
        float currentValue = (axis == MovementAxis.Horizontal) ? transform.position.x : transform.position.y;
        if (Mathf.Abs(currentValue - targetValue) < 0.001f)
        {
            StartCoroutine(WaitAndReverse());
        }
    }

    private IEnumerator WaitAndReverse()
    {
        isWaiting = true;

        float actualDelay = useRandomDelay ? Random.Range(minDelay, maxDelay) : delay;

        if (actualDelay > 0f)
        {
            yield return new WaitForSeconds(actualDelay);
        }

        movingToEnd = !movingToEnd;
        isWaiting = false;
    }
}

