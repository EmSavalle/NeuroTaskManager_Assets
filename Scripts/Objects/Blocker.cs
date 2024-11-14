using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Blocker : MonoBehaviour
{
    public TaskManager taskManager;
    public LSLManager lSLManager;
    public int streamIndex=-1;
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
    public Vector3 hiddenPosNBack;
    public Vector3 hiddenRotNBack;
    public float discoverSpeedNBack;

    [Header("ColorShape")]
    public Vector3 readyPosColorShape;
    public Vector3 readyRotColorShape;
    public Vector3 leftPosColorShape;
    public Vector3 leftRotColorShape;
    public Vector3 rightPosColorShape;
    public Vector3 rightRotColorShape;
    
    public Vector3 hiddenPosColorShape;
    public float discoverSpeedColorShape;

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
    
    private List<Coroutine> activeCoroutines = new List<Coroutine>();
    private PlayerInput playerInput;
    private InputAction leftArrowAction;
    private InputAction rightArrowAction;
    public float startMovementTime = 0;
    public float mytime;
    public bool waitingForObject = false;
    public bool isOvertriggertime;
    public float untriggerTime;

    [Header("Time detection")]
    
    public TimerDetectorEnter timerDetectorEnter;
    public GameObject objectEntered;
    public float timeEntered;
    public List<float> timeMeasured;
    public List<float> results;
    public TaskType taskType;
    public TaskDifficulty taskDifficulty;
    // Start is called before the first frame update
    void Start()
    {
        /*playerInput = new PlayerInput();
        leftArrowAction = playerInput.actions["LeftArrow"];
        rightArrowAction = playerInput.actions["RightArrow"];

        // Bind the function to the action
        leftArrowAction.performed += ctx => OnLeftArrowPressed();
        rightArrowAction.performed += ctx => OnRightArrowPressed();
        leftArrowAction.Enable();
        rightArrowAction.Enable();*/
    }
    private void OnLeftArrowPressed()
    {
        Debug.Log("Left arrow key pressed");
    }
    private void OnRightArrowPressed()
    {
        Debug.Log("Right arrow key pressed");
    }
    // Update is called once per frame
    void Update()
    {
        if(streamIndex == -1 && lSLManager != null){
            for (int i = 0; i < lSLManager.lSLStreams.Count; i++){
                if(lSLManager.lSLStreams[i].letType == LetType.TRIGGERPRESS){
                    streamIndex = i;
                }
            }
        }
        if(waitingForObject && startMovementTime+untriggerTime<Time.time && movingObject == null){
            StopMovingObject();
            waitingForObject = false;
        }
        mytime = Time.time;
        isOvertriggertime = startMovementTime+untriggerTime<Time.time;
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

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
        if(streamIndex != -1){
            lSLManager.SendStringToOutlet(lSLManager.lSLStreams[streamIndex],"Trigger");
        }
        StopAllCoroutines();
        TaskType tt = taskManager.currentTaskType;
        moveObject = true;
        startMovementTime = Time.time;
        waitingForObject = true;
        switch(tt){
            case TaskType.COLORSHAPE:
                yield return StartCoroutine(TriggerColorShape(left));
                break;
            case TaskType.NBACK:
                yield return StartCoroutine(TriggerNBack());
                break;
            case TaskType.GONOGO:
                yield return StartCoroutine(TriggerGoNoGo());
                break;
        }
        yield break;
    }
    public void StartMovingObject(){
        moveObject = true;
        startMovementTime = Time.time;
        waitingForObject = true;
    }
    public void StopMovingObject(){
        moveObject = false;
    }
    public void ObjectDetected(Item i){
        waitingForObject=false;
        TaskType tt = taskManager.currentTaskType;
        if(tt== TaskType.GONOGO && !moveObject){
            StartCoroutine(StartCountDown(TaskType.GONOGO));
        }
        else if(tt == TaskType.NBACK && !moveObject){
            StartCoroutine(StartCountDown(TaskType.NBACK));
        }
        else if(tt == TaskType.COLORSHAPE && !moveObject){
            StartCoroutine(StartCountDown(TaskType.COLORSHAPE));
        }
        else{
            i.gameObject.transform.parent = gameObject.transform;
            i.gameObject.transform.SetParent(transform,false);
            movingObject = i.gameObject;
        }
    }
    public IEnumerator ObjectExited(){
        StopMovingObject();
        waitingForObject = false;
        yield return new WaitForSeconds(0.5f);
        TaskType tt = taskManager.currentTaskType;
        if(movingObject!=null){
            movingObject.transform.parent=null;
        }
        movingObject = null;
        switch(tt){
            case TaskType.COLORSHAPE:
                Debug.Log("Blocker - Readying ColorShape ");
                yield return StartCoroutine(MoveBlocker(readyPosColorShape,readyRotColorShape));
                break;
            case TaskType.NBACK:
                Debug.Log("Blocker - Readying Nback");
                yield return StartCoroutine(MoveBlocker(readyPosNBack,readyRotNBack));
                break;
            case TaskType.GONOGO:
                yield return new WaitForSeconds(0.5f);
                Debug.Log("Blocker - Readying GoNoGo");
                yield return StartCoroutine(MoveBlocker(readyPosGoNoGo,readyRotGoNoGo));
                break;
            default:
                break;
        }
        yield break;
    }

    public IEnumerator TriggerGoNoGo(){
        StartMovingObject();
        StopAllCoroutines();
        yield return StartCoroutine(MoveBlocker(readyPosGoNoGo,readyRotGoNoGo));
    }
    public IEnumerator TriggerNBack(){
        StartMovingObject();
        StopAllCoroutines();
        yield return StartCoroutine(MoveBlocker(readyPosNBack,readyRotNBack));
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
    public IEnumerator StartCountDown(TaskType tt){
        StopAllCoroutines();
        Coroutine count = StartCoroutine(CountDown(tt));
        activeCoroutines.Add(count);
        yield return count;
    }
    public IEnumerator CountDown(TaskType tt){
        float applySpeed =0f;
        if(tt==TaskType.GONOGO){       
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
        else if(tt==TaskType.NBACK){       
            applySpeed = discoverSpeedNBack;
            yield return MoveBlocker(hiddenPosNBack,hiddenRotNBack,applySpeed);
        }
        else if(tt==TaskType.COLORSHAPE){       
            applySpeed = discoverSpeedColorShape;
            yield return MoveBlocker(hiddenPosColorShape,readyRotColorShape,applySpeed);
        }
        
    }
    public void StopAllMovements(){
        if(activeCoroutines.Count>0){
            foreach (Coroutine coroutine in activeCoroutines)
            {
                if(coroutine != null){
                    StopCoroutine(coroutine);
                }
            }
            activeCoroutines.Clear(); 
        }
    }
    public IEnumerator MoveBlocker(Vector3 position, Vector3 rotation){
        StopAllMovements();
        Coroutine move = StartCoroutine(Move(position,rotation,movementSpeed));
        activeCoroutines.Add(move); // Track this coroutine
        yield return move;
    }public IEnumerator MoveBlocker(Vector3 position, Vector3 rotation, float speed){
        StopAllMovements();
        Coroutine move = StartCoroutine(Move(position,rotation,speed));
        activeCoroutines.Add(move); // Track this coroutine
        yield return move;
    }
    public IEnumerator Move(Vector3 position, Vector3 rotation, float speed){
        Quaternion targetRotation = Quaternion.Euler(rotation);  
        Vector3 previousPosition = transform.localPosition;
        Quaternion previousRotation = transform.localRotation;

        while ((Vector3.Distance(transform.localPosition, position) > 0.01f|| (Quaternion.Angle(transform.localRotation, targetRotation) > 0.01f)))
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
            i.blockerHold = true;
            TaskType tt = taskManager.currentTaskType;
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            rb.useGravity = false;
            movingObject = i.gameObject;
            
            ObjectDetected(i);
        }
    }    
    private void OnTriggerStay(Collider other)
    {
        if(moveObject){
            Item it = other.gameObject.GetComponent<Item>();
            if(it == null && other.transform.parent != null){it = other.transform.parent.gameObject.GetComponent<Item>();}
            if( it != null && it.gameObject == movingObject){
                waitingForObject = false;
                it.blockerHold = true;
                Vector3 newPosition = it.gameObject.transform.position+gameObject.transform.right*Time.deltaTime;
                it.gameObject.transform.position=newPosition;
            }
        }
    }
    private void OnTriggerExit(Collider other){
        
        Item it = other.gameObject.GetComponent<Item>();
        if(it == null && other.transform.parent != null){it = other.transform.parent.gameObject.GetComponent<Item>();}
        if( it != null){
            it.blockerHold = false;
            TaskType tt = taskManager.currentTaskType;
            if(tt != TaskType.GONOGO){
                it.transform.parent=null;
            }
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            rb.useGravity = true;
            moveObject = false;
            
            float time = Time.time - timeEntered;
            if(taskManager.currentDifficulty != taskDifficulty || taskManager.currentTaskType != taskType){
                taskDifficulty =taskManager.currentDifficulty ;
                taskType=taskManager.currentTaskType;
                if(timeMeasured.Count>0){
                    results.Add(timeMeasured.Average());
                    timeMeasured = new List<float>();
                }
            }
            timeMeasured.Add(time);

            StartCoroutine(ObjectExited());
            
        }
        
    }
    public void ActivateBlocker(){
        Debug.Log("Activating blocker");
        StartCoroutine(ObjectExited());
    }
    
}
