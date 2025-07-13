using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimpleComboMultiplierUI : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public TextMeshProUGUI textObject;

    // Update is called once per frame
    void Update()
    {
        // FIXME: This is temporary should change to something that only updates when needed
        textObject.SetText($"{playerMovement.GetComboCounter()}x");
    }
}
