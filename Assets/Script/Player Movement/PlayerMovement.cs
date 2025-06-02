using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private ContinuousMazeGenerator continuousMazeGenerator;
    public AnimationCurve moveCurve;
    [SerializeField] private float maxSpeed = 5f, startingSpeed, comboSpeedMultiplier, speedUpDuration = 2f;


    private float horizontalInput, verticalInput, currentSpeed;
    private int comboCounter;

    private Queue<Vector3> targetPositions = new Queue<Vector3>();
    private Vector3 currentTarget;

    private bool inMotion = false;

    private void Awake()
    {
        inMotion = false;
        currentSpeed = startingSpeed;
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
                    targetPositions.Enqueue((Vector3)newPosition);
                    if (!inMotion)
                    {
                        Debug.Log("In Motion, Calling Coroutine");
                        comboCounter += 1;
                        StartCoroutine(MoveToNextPoint());
                    }
                }
                else
                {
                    comboCounter = 1; // FIXME: When Combo is broken, multiplier not increasing for some reason
                    Debug.Log("Wrong Direction");
                }
            }
        }
    }


    private IEnumerator MoveToNextPoint()
    {
        inMotion = true;

        while (targetPositions.Count > 0)
        {
            currentTarget = targetPositions.Dequeue();
            Debug.Log($"Moving to Current Target {currentTarget}");

            float elapsed = 0f;

            StartCoroutine(SpeedUpOverTime(Mathf.Min(maxSpeed, startingSpeed + comboCounter * comboSpeedMultiplier), speedUpDuration)); // TODO: Test Combo Speed Multiplier

            while (Vector3.Distance(transform.position, currentTarget) > 0.01f)
            {
                elapsed += Time.deltaTime;

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    currentTarget,
                    currentSpeed * Time.deltaTime
                ); // FIXME: Sometimes it'll clip through the wall when moving diagonally

                yield return null;
            }

            transform.position = currentTarget;
            // currentSpeed = 0f; // Reset speed if needed
            // TODO: FIGURE OUT IF NEED TO CANCEL MOVEMENT WHEN WRONG
        }

        inMotion = false;
    }

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
}