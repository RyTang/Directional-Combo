using UnityEngine;

public class InputHandler : MonoBehaviour
{  
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