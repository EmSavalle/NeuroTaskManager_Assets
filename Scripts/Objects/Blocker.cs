using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Blocker : MonoBehaviour
{
    public TaskManager taskManager;
    public Vector3 hiddenPos;
    public Vector3 hiddenRot;
    [Header("GoNoGo")]
    public Vector3 readyPosGoNoGo;
    public Vector3 readyRotGoNoGo;
    public Vector3 hiddenPosGoNoGo;
    public Vector3 hiddenRotGoNoGo;
    public float discoverSpeedLow,discoverSpeedMedium,discoverSpeedHigh;
    public bool moveObject;

    [Header("NBack")]
    public Vector3 readyPosNBack;
    public Vector3 readyRotNBack;
    public Vector3 activatedPosNBack;
    public Vector3 activatedRotNBack;

    [Header("ColorShape")]
    public Vector3 readyPosColorShape;
    public Vector3 readyRotColorShape;
    public Vector3 leftPosColorShape;
    public Vector3 leftRotColorShape;
    public Vector3 rightPosColorShape;
    public Vector3 rightRotColorShape;

    public float movementSpeed;
    public float blockingSpeed;
    public float objectMovementSpeed;
    public bool ready;
    public bool activated;
    public GameObject movingObject;

    public bool fakeActivationLeft,fakeActivationRight;

    public List<UnityEngine.XR.InputDevice> inputDevices;
    public UnityEngine.XR.InputDevice inputDeviceLeft;
    public UnityEngine.XR.InputDevice inputDeviceRight;
    public bool stopMove = false;
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
            //Debug.Log(inputDevices.Count.ToString() + " left controller found");
            if (inputDevices.Count > 0)
            {
                inputDeviceLeft = inputDevices[0];
            }
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Controller | UnityEngine.XR.InputDeviceCharacteristics.Right, inputDevices);
            //Debug.Log(inputDevices.Count.ToString() + " right controller found");
            if (inputDevices.Count > 0)
            {
                inputDeviceRight = inputDevices[0];
            }
        }
        float triggerValue; bool triggerPressed;
        inputDeviceLeft.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);
        inputDeviceLeft.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed);
        if(triggerValue != 0 || triggerPressed || fakeActivationLeft){
            fakeActivationLeft=false;
            Debug.Log("Right trigger pressed");
            StartCoroutine(Trigger(true));
        }
        inputDeviceRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);
        inputDeviceRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed);
        if(triggerValue != 0 || triggerPressed ||fakeActivationRight){
            fakeActivationRight = false;
            Debug.Log("Right trigger pressed");
            StartCoroutine(Trigger(false));
        }
    }
    public IEnumerator Trigger(bool left){
        TaskType tt = taskManager.currentTaskType;
        switch(tt){
            case TaskType.COLORSHAPE:
                yield return StartCoroutine(TriggerColorShape(left));
                break;
            case TaskType.NBACK:
                yield return StartCoroutine(TriggerNBack());
                break;
            case TaskType.GONOGO:
                yield return StartCoroutine(TriggerSorting());
                break;
        }
        yield break;
    }
    public void StartMovingObject(){
        moveObject = true;
    }
    public void StopMovingObject(){
        moveObject = false;
    }
    public void ObjectDetected(Item i){
        Debug.Log("Object entered");
        TaskType tt = taskManager.currentTaskType;

        i.gameObject.transform.parent = gameObject.transform;
        // Set the new parent for father (blocker)
        i.gameObject.transform.SetParent(transform,false);
        
        
        
        movingObject = i.gameObject;
        if(tt== TaskType.GONOGO){
            StartCoroutine(StartCountDown());
        }
    }
    public IEnumerator ObjectExited(){
        StopMovingObject();
        yield return null;
        stopMove = false;
        TaskType tt = taskManager.currentTaskType;
        if(movingObject!=null){
            movingObject.transform.parent=null;
        }
        movingObject = null;
        switch(tt){
            case TaskType.COLORSHAPE:
                yield return StartCoroutine(MoveBlocker(readyPosColorShape,readyRotColorShape));
                break;
            case TaskType.NBACK:
                yield return StartCoroutine(MoveBlocker(readyPosNBack,readyRotNBack));
                break;
            case TaskType.GONOGO:
                yield return StartCoroutine(MoveBlocker(readyPosGoNoGo,readyRotGoNoGo));
                break;
        }
        yield break;
    }

    public IEnumerator TriggerSorting(){
        StartMovingObject();
        StopCoroutine(StartCountDown());
        yield return StartCoroutine(MoveBlocker(readyPosGoNoGo,readyRotGoNoGo));
    }
    public IEnumerator TriggerNBack(){
        StartMovingObject();
        yield return StartCoroutine(MoveBlocker(activatedPosNBack,activatedRotNBack));
    }
    public IEnumerator TriggerColorShape(bool left){
        StartMovingObject();
        if(left){
            yield return StartCoroutine(MoveBlocker(leftPosColorShape,leftRotColorShape));
        }
        else{
            yield return StartCoroutine(MoveBlocker(rightPosColorShape,rightRotColorShape));
        }
        
    }
    public IEnumerator StartCountDown(){
        float applySpeed =0f;
        switch(taskManager.currentDifficulty){
            case TaskDifficulty.HIGH:
                applySpeed = discoverSpeedHigh;
                break;
            case TaskDifficulty.MEDIUM:
                applySpeed = discoverSpeedMedium;
                break;
            case TaskDifficulty.LOW:
                applySpeed = discoverSpeedLow;
                break;
        }
        yield return MoveBlocker(hiddenPosGoNoGo,hiddenRotGoNoGo,applySpeed);
    }
    public IEnumerator MoveBlocker(Vector3 position, Vector3 rotation){
        yield return StartCoroutine(MoveBlocker(position,rotation,movementSpeed));
    }
    public IEnumerator MoveBlocker(Vector3 position, Vector3 rotation, float speed){
        Quaternion targetRotation = Quaternion.Euler(rotation);  
        Vector3 previousPosition = transform.localPosition;
        Quaternion previousRotation = transform.localRotation;

        while (!stopMove && (Vector3.Distance(transform.localPosition, position) > 0.01f|| (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f)))
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, speed * Time.deltaTime);
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, speed * 20 * Time.deltaTime);
            
            // Calculate position and rotation changes
            Vector3 positionChange = transform.localPosition - previousPosition;
            Quaternion rotationChange = transform.localRotation * Quaternion.Inverse(previousRotation);
            //if(movingObject!=null){movingObject.transform.position+=positionChange;}
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
    public IEnumerator Activate(){
        Debug.Log("Blocker - Activating");
        yield return StartCoroutine(ObjectExited());
        yield break;
    }

    public IEnumerator Deactivate(){
        yield return StartCoroutine(MoveBlocker(hiddenPos,hiddenRot));
    }
    private void OnTriggerEnter(Collider other){
        Item i = other.gameObject.GetComponent<Item>();
        if(i == null && other.transform.parent != null){
            i = other.transform.parent.gameObject.GetComponent<Item>();
        }
        if(i != null){
            Debug.Log("Object entered");
            i.blockerHold = true;
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            rb.useGravity = false;
            ObjectDetected(i);
        }
    }    
    private void OnTriggerStay(Collider other)
    {
        if(moveObject){
            Item it = other.gameObject.GetComponent<Item>();
            if(it == null && other.transform.parent != null){it = other.transform.parent.gameObject.GetComponent<Item>();}
            if( it != null && it.gameObject == movingObject){
                it.blockerHold = true;
                Vector3 newPosition = it.gameObject.transform.position+gameObject.transform.right*Time.deltaTime;
                it.gameObject.transform.position=newPosition;
                movingObject = it.gameObject;
            }
        }
    }
    private void OnTriggerExit(Collider other){
        
        Item it = other.gameObject.GetComponent<Item>();
        if(it == null && other.transform.parent != null){it = other.transform.parent.gameObject.GetComponent<Item>();}
        if( it != null){
            it.transform.parent=null;
            it.blockerHold = false;
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            rb.useGravity = true;
            stopMove = true;
            
            StartCoroutine(ObjectExited());
            Debug.Log("Object exited");
        }
        
    }
    public void ActivateBlocker(){
        StartCoroutine(ObjectExited());
    }
    
}
