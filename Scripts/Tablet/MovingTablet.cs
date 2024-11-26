using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovingTablet : MonoBehaviour
{    public List<QuestionnaireButton> answer;
    public GameObject move;
    public float speed = 1f; // Movement speed
    public int direction; // Value to determine the movement direction (0 to 5)
    
    public new void Select(int value){
        Vector3 movement = Vector3.zero;

        switch (value)
        {
            case 0: // Up
                movement = Vector3.up;
                break;
            case 1: // Down
                movement = Vector3.down;
                break;
            case 2: // Left
                movement = Vector3.left;
                break;
            case 3: // Right
                movement = Vector3.right;
                break;
            case 4: // Forward
                movement = Vector3.forward;
                break;
            case 5: // Backward
                movement = Vector3.back;
                break;
            default:
                Debug.LogWarning("Invalid direction. Please use values between 0 and 5.");
                break;
        }

        // Apply movement
        move.transform.Translate(movement * speed * Time.deltaTime);
    }
}
