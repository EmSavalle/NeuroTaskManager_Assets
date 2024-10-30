using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class ConveyorBelt : MonoBehaviour
{
    // Start is called before the first frame update// Speed at which the conveyor moves objects
    public float conveyorSpeed = 2.0f;

    public GameObject stopperGO;
    public ConveyorStopper cs;
    public BoxCollider stopperCollider;
    public Spawner spawner;
    public string itemTag = "Item";
    public List<ValidationTray> vt;
    public TrashCan trashCan;
    public Blocker blocker;
    public bool stopper;
    public bool stopperTriggered;
    public bool beltResumeEmpty;
    public bool intialysingBelt,deintialysingBelt;
    // Direction of the conveyor movement
    public Vector3 conveyorDirection = Vector3.forward;
    private static System.Random rng = new System.Random(); 
    public List<GameObject> content = new List<GameObject>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ResumeBelt(){
        stopperTriggered=false;
    }
    public IEnumerator InitialyzeBelt(Task t){
        intialysingBelt=true;

        // Trays setup
        ValidationTrayType vType = t.validationType;
        int nbTrays = t.nbValidationTrays;
        List<GameObject> items = t.objects;
        items = Shuffle(items);
        int assignedObject=0;
        if(t.blocker){
            StartCoroutine(blocker.Activate());
        }
        foreach (ValidationTray v in vt){
            if(v.type == vType && nbTrays>0){
                if(v.type == ValidationTrayType.DELIVERY){
                    DeliveryTray dv = (DeliveryTray) v;
                    Item it = items[assignedObject].GetComponent<Item>();
                    if(it==null){it = items[assignedObject].transform.parent.GetComponent<Item>();}
                    yield return StartCoroutine(dv.ActivateTray(it));
                    assignedObject+=1;
                    nbTrays-=1;
                }
                else if(v.type == ValidationTrayType.BOX){
                    BoxingTray dv = (BoxingTray) v;
                    Item it = items[assignedObject].GetComponent<Item>();
                    if(it==null){it = items[assignedObject].transform.parent.GetComponent<Item>();}
                    yield return StartCoroutine(dv.ActivateTray(t.boxingTask.boxingRequirements));
                    assignedObject+=1;
                    nbTrays-=1;
                }
                else{
                    Item it = items[assignedObject].GetComponent<Item>();
                    if(it==null){it = items[assignedObject].transform.parent.GetComponent<Item>();}
                    yield return StartCoroutine(v.ActivateTray(it));
                    nbTrays-=1;
                    assignedObject+=1;
                }
            }
        }
        if(nbTrays != 0){
            Debug.Log("Number of trays found under required amount");
        }
        trashCan.noFailure=t.noFailure;
        if(t.taskType == TaskType.SORTING){
            trashCan.SetRecord(items.GetRange(0,assignedObject));
        }
        if(t.taskType == TaskType.NBACK){
            trashCan.SetRecordNback();
        }
        
        // Spawner setup
        spawner.type = (t.taskType == TaskType.COUNTING || t.taskType == TaskType.ASSEMBLY) 
            ? SpawnerType.BATCH 
            : SpawnerType.CONTINUOUS;

        //Stopper setup
        if(t.continuousBelt){
            cs.gameObject.SetActive(false);
        }
        else{
            cs.gameObject.SetActive(true);
        }
        intialysingBelt=false;
    }
    public IEnumerator DeinitialyseBelt(Task t){
        deintialysingBelt=true;
        
        if(t.blocker){
            StartCoroutine(blocker.Deactivate());
        }
        // Trays setup
        ValidationTrayType vType = t.validationType;
        int nbTrays = t.nbValidationTrays;
        List<GameObject> items = t.objects;
        int assignedObject=0;
        foreach (ValidationTray v in vt){
            if(v.type == vType && nbTrays>0){
                Item it = items[assignedObject].GetComponent<Item>();
                if(it==null){it = items[assignedObject].transform.parent.GetComponent<Item>();}
                ItemType i = it.itemType;
                
                yield return StartCoroutine(v.DeactivateTray());
                nbTrays-=1;
            }
        }
        deintialysingBelt=false;

    }
    private void OnTriggerEnter(Collider collider){
        if(collider.gameObject.tag == "Item"){
            content.Add(collider.gameObject);
        }
    }
    private void OnTriggerExit(Collider collider){
        if(content.Contains(collider.gameObject)){
            content.Remove(collider.gameObject);
        }
    }

    public IEnumerator EmptyBelt(){
        bool st,sttri;
        st = stopper;
        sttri = stopperTriggered;
        stopper = false;
        stopperTriggered = false;
        while(content.Count>0){
            stopper = false;
            stopperTriggered = false;
            yield return null;
        }
        stopper = st;
        stopperTriggered = sttri;
    }
    private void OnTriggerStay(Collider other)
    {
        // Check if the object on the conveyor has a Rigidbody component
        Rigidbody rb = other.attachedRigidbody;
        Item it = other.gameObject.GetComponent<Item>();
        if(it == null && other.transform.parent != null){
            it = other.transform.parent.gameObject.GetComponent<Item>();
        }
        if(it != null) {
            if (rb != null && !it.blockerHold)
            {
                if(other.gameObject.tag == "Item"){
                    if(!stopper){
                        
                        // Apply movement in the specified direction
                        Vector3 movement = conveyorDirection * conveyorSpeed * Time.deltaTime;
                        rb.MovePosition(rb.position + movement);
                    }
                    else if(stopperGO.activeSelf){
                        if(!checkStopper()){
                            // Apply movement in the specified direction
                            Vector3 movement = conveyorDirection * conveyorSpeed * Time.deltaTime;
                            rb.MovePosition(rb.position + movement);
                        }
                    }
                    else{
                        // Apply movement in the specified direction
                        Vector3 movement = conveyorDirection * conveyorSpeed * Time.deltaTime;
                        rb.MovePosition(rb.position + movement);
                    }
                }
                
            }
        }
    }

    public bool checkStopper(){
        // Get the center and size of the box collider in world space
        Vector3 boxCenter = stopperCollider.transform.TransformPoint(stopperCollider.center);
        Vector3 boxSize = stopperCollider.size * 0.5f; // Half the size because OverlapBox uses extents

        // Get all colliders within the box
        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize, stopperCollider.transform.rotation);

        // Check if any collider has the tag "Item"
        foreach (Collider col in colliders)
        {
            if (col.CompareTag(itemTag))
            {
                return true; // Exit if an item is found
            }
        }

        return false;
    }
    
    public void StartDelivery(List<GameObject> gameObjects){
        foreach (ValidationTray v in vt){
            if(v.type == ValidationTrayType.DELIVERY && v.activated){
                StartCoroutine(((DeliveryTray)v).StartDelivery(gameObjects));
            }
        }
    }public void StartBoxing(List<GameObject> gameObjects){
        foreach (ValidationTray v in vt){
            if(v.type == ValidationTrayType.BOX && v.activated){
                StartCoroutine(((BoxingTray)v).StartBoxing(gameObjects));
            }
        }
    }
    public void StopDelivery(){
        foreach (ValidationTray v in vt){
            if(v.type == ValidationTrayType.DELIVERY && v.activated){
                ((DeliveryTray)v).stopDelivery = true;
            }
        }
    }public void StopBoxing(){
        foreach (ValidationTray v in vt){
            if(v.type == ValidationTrayType.BOX && v.activated){
                ((BoxingTray)v).stopBoxing = true;
            }
        }
    }
    
    public List<GameObject> Shuffle(List<GameObject>  list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            GameObject value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }
        return list;  
    }
}

public enum ConveyorType {CONTINUOUS,BATCH}
