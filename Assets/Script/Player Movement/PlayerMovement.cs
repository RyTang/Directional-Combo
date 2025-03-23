using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private ContinuousMazeGenerator continuousMazeGenerator;
    private float horizontalInput, verticalInput;

    private void Awake()
    {
        // TODO: Need to wait until the maze is initialised to get the starting position
        transform.position = continuousMazeGenerator.mazeStartPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)){
            horizontalInput = GetHorizontalControls();
            verticalInput = GetVerticalControls();
            // TODO: Prob need to read based on input driven
            if (new Vector2 (horizontalInput,verticalInput) != Vector2.zero){
                Vector2 inputDirection = Mathf.Abs(horizontalInput) > Mathf.Abs(verticalInput) 
                    ? new Vector2(Mathf.Sign(horizontalInput), 0) 
                    : new Vector2(0, Mathf.Sign(verticalInput));

                Vector2? newPosition = continuousMazeGenerator.ConsumeIfCorrect(inputDirection);

                if (newPosition != null){
                    Debug.Log($"Correct Direction {inputDirection}, now at: {newPosition}");

                    transform.position = new Vector3(newPosition.Value.x, newPosition.Value.y, transform.position.z);
                }
                else {
                    Debug.Log("Wrong Direction");
                }
            }
        }
    }

    // TODO: Create Input Queue

    // TODO: Change this to reference the Input Handler, need to determine whether to use event based subscription to the Input or etc
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