using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class CountingButton : Button
{
    public CountingTablet qt;
    public int value;

    public UnityEngine.XR.InputDevice leftController;
    public UnityEngine.XR.InputDevice rightController;
    public bool leftFound,rightFound;

    public bool waitingForTrigger = false;
    public Color32 backupColor;
    public new void Start(){
        base.Start();
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);
        foreach (var device in inputDevices)
        {
            Debug.Log(string.Format("Device found with name '{0}' and role '{1}'", device.name, device.role.ToString()));
        }
    }
    public new void Update(){
        base.Update();
        bool triggerValue;
        if(leftController == null){
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }
        leftFound = leftController != null;
        rightFound = rightController != null;
        if(waitingForTrigger){
            if(leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue){
                Select();
                waitingForTrigger = false;
            }
            else if(rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue){
                Select();
                waitingForTrigger = false;
            }
        }
        if(leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue){
            Debug.Log("Left controller click");
        }
        else if(rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue){
            Debug.Log("Right controller click");
        }
        bool pressed;
        leftController.IsPressed(InputHelpers.Button.Trigger, out pressed);
        if(pressed){
            Debug.Log("Left controller click");
        }
        rightController.IsPressed(InputHelpers.Button.Trigger, out pressed);
        if(pressed){
            Debug.Log("Right controller click");
        }
        
    }
    public override void Select()
    {
        base.Select();
        qt.Select(value);
    }public override void Unselect()
    {
        base.Unselect();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Interactor"){
            Debug.Log("Triggered");
            waitingForTrigger = true;
            Select();
        }
    }private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Interactor"){
            Debug.Log("Triggered");
            waitingForTrigger = false;
        }
    }

}
