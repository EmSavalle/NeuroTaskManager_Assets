using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Blocker : MonoBehaviour
{
    public Vector3 hiddenPos,readyPos, activatedPos;
    public Vector3 hiddenRot,readyRot, activatedRot;

    public float movementSpeed;
    public float blockingSpeed;
    public float objectMovementSpeed;
    public bool ready;
    public bool activated;
    public GameObject movingObject;

    public bool fakeActivation;

    public List<UnityEngine.XR.InputDevice> inputDevices;
    public UnityEngine.XR.InputDevice inputDeviceLeft;
    public UnityEngine.XR.InputDevice inputDeviceRight;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            Debug.Log(string.Format("Device found with name '{0}' and role '{1}'", device.name, device.role.ToString()));
        }
        if(inputDeviceLeft.role.ToString()!="LeftHanded" || inputDeviceRight.role.ToString()!="RightHanded"){
            
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Controller | UnityEngine.XR.InputDeviceCharacteristics.Left, inputDevices);
            Debug.Log(inputDevices.Count.ToString() + " left controller found");
            if (inputDevices.Count > 0)
            {
                inputDeviceLeft = inputDevices[0];
            }
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Controller | UnityEngine.XR.InputDeviceCharacteristics.Right, inputDevices);
            Debug.Log(inputDevices.Count.ToString() + " right controller found");
            if (inputDevices.Count > 0)
            {
                inputDeviceRight = inputDevices[0];
            }
        }
        float triggerValue; bool triggerPressed;
        inputDeviceLeft.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);
        inputDeviceLeft.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed);
        if(triggerValue != 0 || triggerPressed){
            Debug.Log("Left trigger pressed");
            StartCoroutine(StartBlocking());
        }
        inputDeviceRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);
        inputDeviceRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed);
        if(triggerValue != 0 || triggerPressed){
            Debug.Log("Right trigger pressed");
            StartCoroutine(StartBlocking());
        }
        if(fakeActivation){
            fakeActivation = false;
            StartCoroutine(StartBlocking());
        }
    }
    public IEnumerator Activate(){
        ready = false;
        Debug.Log("Base tray - Activating tray");
        //Animation tray activation
        Quaternion targetRotation = Quaternion.Euler(readyRot);  
        while (Vector3.Distance(transform.localPosition, readyPos) > 0.01f|| (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f))  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, readyPos, movementSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, movementSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Debug.Log("Blocker activated");
        yield break;
    }

    public IEnumerator Deactivate(){
        ready = false;
        Quaternion targetRotation = Quaternion.Euler(hiddenRot);  
        while (Vector3.Distance(transform.localPosition, hiddenPos) > 0.01f|| (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f))  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, hiddenPos, movementSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, movementSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return null;
        }
        yield break;
    }

    public IEnumerator StartBlocking(){
        activated = true;
        Quaternion targetRotation = Quaternion.Euler(activatedRot);  
        while (Vector3.Distance(transform.localPosition, activatedPos) > 0.01f|| (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f))  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, activatedPos, blockingSpeed*2 * Time.deltaTime);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, blockingSpeed);
            
            // Wait for the next frame before continuing
            yield return null;
        }
        yield break;
    }

    public IEnumerator StopBlocking(){
        activated = false;
        Quaternion targetRotation = Quaternion.Euler(readyRot);
        while (Vector3.Distance(transform.localPosition, readyPos) > 0.01f|| (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f))  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, readyPos, blockingSpeed * 2* Time.deltaTime);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, blockingSpeed);
            
            // Wait for the next frame before continuing
            yield return null;
        }
        yield break;
    }
    
    private void OnTriggerStay(Collider other)
    {
        if(activated){
            Item it = other.gameObject.GetComponent<Item>();
            if(it == null){it = other.transform.parent.gameObject.GetComponent<Item>();}
            if( it != null){
                it.blockerHold = true;
                Vector3 newPosition = it.gameObject.transform.position+gameObject.transform.right*Time.deltaTime;
                it.gameObject.transform.position=newPosition;
                movingObject = it.gameObject;
            }
            
        }
    }
    private void OnTriggerExit(Collider other){
        if(activated){
            Item it = other.gameObject.GetComponent<Item>();
            if(it == null){it = other.transform.parent.gameObject.GetComponent<Item>();}
            if(movingObject == it.gameObject && it != null){
                StartCoroutine(StopBlocking());
                it.blockerHold =false;
            }
        }
    }
    public void ActivateBlocker(){
        StartCoroutine(StartBlocking());
    }
    
}
