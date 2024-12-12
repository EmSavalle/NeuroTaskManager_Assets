using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class VRRaycastInteraction : MonoBehaviour
{
    public LayerMask interactableLayer; // Set this to the layer for your buttons
    public float rayDistance = 5.0f; // Distance of the raycast
    public string interactionButton = "Fire1"; // Button for interaction (map to trigger or grip)

    private QuestionnaireButton currentButton;

    public List<UnityEngine.XR.InputDevice> inputDevices;
    public UnityEngine.XR.InputDevice inputDeviceRight;
    public UnityEngine.XR.InputDevice inputDeviceLeft;
    public QuestionnaireButton hit;
    public bool triggerPressed;
    public bool left;
    public bool isTriggered;
    public bool wasPressed = false;
    void Start()
    {
    }
    void Update()
    {

            float triggerValue;
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);
        if(!left){
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Controller | UnityEngine.XR.InputDeviceCharacteristics.Right, inputDevices);
            //Debug.Log(inputDevices.Count.ToString() + " right controller found");
            if (inputDevices.Count > 0)
            {
                inputDeviceRight = inputDevices[0];
            }
            inputDeviceRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);
            inputDeviceRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed);
            if((triggerValue != 0 || triggerPressed) && !wasPressed)
            {
                Debug.Log("Questionnaire trigger press");
                triggerPressed = true;
                isTriggered = true;
                wasPressed = true;
            }
            else{
                triggerPressed = false;
                isTriggered = false;
                wasPressed = false;
            }
        }
        else{
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Controller | UnityEngine.XR.InputDeviceCharacteristics.Left, inputDevices);
            //Debug.Log(inputDevices.Count.ToString() + " right controller found");
            if (inputDevices.Count > 0)
            {
                inputDeviceLeft = inputDevices[0];
            }
            inputDeviceLeft.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);
            inputDeviceLeft.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed);
            if((triggerValue != 0 || triggerPressed ) && !wasPressed)
            {
                Debug.Log("Questionnaire trigger press");
                triggerPressed = true;
                isTriggered = true;
                wasPressed = true;
            }
            else{
                triggerPressed = false;
                isTriggered = false;
                wasPressed = false;
            }
        }

        

        // Cast the ray and check for collisions
        if (hit != null && triggerPressed)
        {
            Debug.Log("Selection clicked!");
            hit.Select();
            hit=null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<QuestionnaireButton>() != null)
        {
            hit = other.gameObject.GetComponent<QuestionnaireButton>();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<QuestionnaireButton>() != null)
        {
            hit = other.gameObject.GetComponent<QuestionnaireButton>();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<QuestionnaireButton>() != null)
        {
            hit = null;
        }
    }
}
