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
    public QuestionnaireButton hit;
    public bool triggerPressed;
    void Start()
    {
    }
    void Update()
    {

        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        if(inputDeviceRight.role.ToString()!="RightHanded"){
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Controller | UnityEngine.XR.InputDeviceCharacteristics.Right, inputDevices);
            //Debug.Log(inputDevices.Count.ToString() + " right controller found");
            if (inputDevices.Count > 0)
            {
                inputDeviceRight = inputDevices[0];
            }
        }
        float triggerValue; bool triggerPressed;
        inputDeviceRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);
        inputDeviceRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed);
        if(triggerValue != 0 || triggerPressed ){
            Debug.Log("Questionnaire trigger press");
            triggerPressed = true;
        }
        else{
            triggerPressed = false;
        }

        

        // Cast the ray and check for collisions
        if (hit != null && triggerPressed)
        {
            
            hit.Select();
            hit=null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<QuestionnaireButton>() != null){
            hit = other.gameObject.GetComponent<QuestionnaireButton>();
        }
    }
}
