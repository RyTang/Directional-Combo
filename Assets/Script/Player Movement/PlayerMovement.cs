using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private ContinuousMazeGenerator continuousMazeGenerator;
    public AnimationCurve moveCurve;
    [SerializeField] private float maxSpeed = 5f, startingSpeed, comboSpeedMultiplier, speedUpDuration = 2f;

    [SerializeField] private float comboDurationBuffer = 3; // TODO: Maybe this should reduce slightly as combo goes up

    private float horizontalInput, verticalInput, currentSpeed;
    private int comboCounter;
    private Queue<Vector3> targetPositions = new Queue<Vector3>();
    private Vector3 currentTarget;
    private Coroutine comboBufferCoroutine;
    private bool inMotion = false;

    private void Awake()
    {
        inMotion = false;
        currentSpeed = startingSpeed;
        comboCounter = 1;
    }

    public void MoveToMazeStartPosition()
    {
        transform.position = continuousMazeGenerator.mazeStartPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            horizontalInput = GetHorizontalControls();
            verticalInput = GetVerticalControls();
            if (new Vector2(horizontalInput, verticalInput) != Vector2.zero)
            {
                Vector2 inputDirection = Mathf.Abs(horizontalInput) > Mathf.Abs(verticalInput)
                    ? new Vector2(Mathf.Sign(horizontalInput), 0)
                    : new Vector2(0, Mathf.Sign(verticalInput));

                Vector3? newPosition = continuousMazeGenerator.ConsumeIfCorrect(inputDirection);

                if (newPosition != null)
                {
                    Debug.Log("Adding +1 to Combo Counter");
                    targetPositions.Enqueue((Vector3)newPosition);
                    comboCounter += 1;
                    // Start Combo if not in Motion
                    if (!inMotion)
                    {
                        StartCoroutine(MoveToNextPoint());
                    }
                }
                else
                {
                    comboCounter = 1;
                    Debug.Log("Wrong Direction");
                }
            }
        }
    }
    // 2 Aspects: Have a delay buffer before combo counter goes down
    // Otherwise, allow users to prequeue their inputs, but if they missed the input then they slow down. -> Need to think do they continue at that max speed until they read the point they failed or something else?

    private IEnumerator MoveToNextPoint()
    {
        inMotion = true;

        // Resets the combo buffer coroutine if it exists, allows leeway in player inputs
        if (comboBufferCoroutine != null)
        {
            StopCoroutine(comboBufferCoroutine);
            comboBufferCoroutine = null;
        }
       
        // While there are target positions to move to, continue moving to the next point
        while (targetPositions.Count > 0)
        {
            currentTarget = targetPositions.Dequeue();

            float elapsed = 0f;

            StartCoroutine(SpeedUpOverTime(Mathf.Min(maxSpeed, startingSpeed + comboCounter * comboSpeedMultiplier), speedUpDuration)); // TODO: Test Combo Speed Multiplier

            while (Vector3.Distance(transform.position, currentTarget) > 0.01f)
            {
                elapsed += Time.deltaTime;
                // If going diagonal log the transform position and current Target
                if (transform.position.x != currentTarget.x && transform.position.y != currentTarget.y)
                {
                    // if both don't match then there is an issue
                    Debug.LogError($"There is an Error, Moving Diagonally, Current: {transform.position}, Target: {currentTarget}");
                }
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    currentTarget,
                    currentSpeed * Time.deltaTime
                ); // FIXME: Sometimes it'll clip through the wall when moving diagonally

                yield return null;
            }

            transform.position = currentTarget;
            // TODO: FIGURE OUT IF NEED TO CANCEL MOVEMENT WHEN WRONG
        }

        inMotion = false;
        if (comboCounter > 1) comboBufferCoroutine = StartCoroutine(ComboBufferTime());
    }

    /// <summary>
    /// Gradually speeds up the player over a specified duration to the next speed.
    /// </summary>
    /// <param name="nextSpeed">Speed Wanted to achieved</param>
    /// <param name="duration">Duration to speed up to</param>
    /// <returns></returns>
    private IEnumerator SpeedUpOverTime(float nextSpeed, float duration)
    {
        float elapsed = 0f;
        float startSpeed = currentSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = moveCurve.Evaluate(t);

            currentSpeed = Mathf.Lerp(startSpeed, nextSpeed, curvedT);
            yield return null;
        }

        currentSpeed = nextSpeed;
    }

    private IEnumerator ComboBufferTime()
    {
        yield return new WaitForSeconds(comboDurationBuffer);
        comboCounter = 1;
        comboBufferCoroutine = null;
    }


    /// <summary>
    /// Retrieves the Current Raw input for Horizontal Actions
    /// </summary>
    /// <returns>Retrives the raw value of the input</returns>
    public virtual float GetHorizontalControls()
    {
        return Input.GetAxisRaw("Horizontal");
    }

    /// <summary>
    /// Retrieves the Current Raw input for Vertical Actions
    /// </summary>
    /// <returns>Retrives the Raw Value of the Input</returns>
    public virtual float GetVerticalControls()
    {
        return Input.GetAxisRaw("Vertical");
    }

    /// <summary>
    /// Returns the current Combo Counter
    /// </summary>
    /// <returns>Integer of the Current Combo   </returns>
    public virtual int GetComboCounter() {
        return comboCounter;
    }
}