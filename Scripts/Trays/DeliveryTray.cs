using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class DeliveryTray : ValidationTray
{
    public List<GameObject> deliverable;
    public List<GameObject> randomizedDeliverable;
    public bool randomizeDeliverable;
    public int currentDeliverable;
    public GameObject delivery;
    public GameObject deliverySpot;
    public bool buttonDelivery;
    public GameObject validationTabletObj;
    public ValidationTablet validationTablet;
    public bool randomRotation;

    public bool stopDelivery;
    public bool deliveryComplete;

    public Vector3 startPositionDelivery, endPositionDelivery;
    public bool isDelivering = false;
    private static System.Random rng = new System.Random();  

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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(validationTabletObj != null && validationTablet == null){
            validationTablet=validationTabletObj.GetComponent<ValidationTablet>();
        }   
        /*if(!buttonDelivery && isDelivering && delivery != null){
            
            if(delivery.GetComponent<ReceiverItem>().itemType==ItemType.COMPLETEDRECEIVER){
                deliveryComplete=true;
            }
            
        }*/

    }

    public IEnumerator StartDelivery(List<GameObject> objects){
        if(buttonDelivery){
            validationTablet.GetComponent<Tablet>().StartTablet();
        }
        deliverable = objects;
        if(randomizeDeliverable){
            deliverable = Shuffle(deliverable);
        }
        stopDelivery = false;
        isDelivering = true;
        currentDeliverable = 0;
        Debug.Log("Delivery Tray -Starting delivery");
        while(!stopDelivery){
            //Debug.Log("Delivery Tray - looping delivery");
            if(deliveryComplete){
                yield return StartCoroutine(DeliverObejct());
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        isDelivering = false;
        yield break;
    }
    public void StopDelivery(){
        stopDelivery = true;
    }
    public IEnumerator DeliverObejct(){
        deliveryComplete = false;
        if(isDelivering){
            while (Vector3.Distance(deliverySpot.transform.localPosition, startPositionDelivery) > 0.01f)  // 0.01 is tolerance for close enough
            {
                // Move towards the target
                deliverySpot.transform.localPosition = Vector3.MoveTowards(deliverySpot.transform.localPosition, startPositionDelivery, moveSpeed * Time.deltaTime);
                
                // Wait for the next frame before continuing
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        if(deliverySpot.transform.childCount!=0){
            GatherDelivery();
        }
        InstantiateDeliverable();
        while (Vector3.Distance(deliverySpot.transform.localPosition, endPositionDelivery) > 0.01f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            deliverySpot.transform.localPosition = Vector3.MoveTowards(deliverySpot.transform.localPosition, endPositionDelivery, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        isDelivering=true;
    }
    public void GatherDelivery(){
        
        if(CheckDelivery(delivery)){
            participantInfos.TaskSuccess();
        }
        else{
            participantInfos.TaskError();
        }
        Destroy(delivery);
        deliveryComplete = false;
    }
    public bool CheckDelivery(GameObject deli){
        if(deli == null){return false;}
        //return deli.GetComponent<ReceiverItem>().isValidated;
        return false;
        
    }
    public void InstantiateDeliverable(){
        //Debug.Log("Delivery Tray - Instantiating delivery");
        if(deliverySpot.transform.childCount!=0){
            Destroy(deliverySpot.transform.GetChild(0).gameObject);
        }
        if(currentDeliverable<deliverable.Count){
            GameObject go = Instantiate(deliverable[currentDeliverable],deliverySpot.transform.position, deliverySpot.transform.rotation);
            go.transform.parent = deliverySpot.transform;
            go.transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(0, 4) * 90, transform.eulerAngles.z);
            currentDeliverable ++;
            delivery = go;
        }
        if(currentDeliverable==deliverable.Count){
            if(randomizeDeliverable){
               deliverable = Shuffle(deliverable);
            }
            currentDeliverable=0;
        }
    }
    public new IEnumerator DeactivateTray(){
        if(deliverySpot.transform.childCount!=0){
            GatherDelivery();
        }
        while (Vector3.Distance(deliverySpot.transform.localPosition, startPositionDelivery) > 0.01f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            deliverySpot.transform.localPosition = Vector3.MoveTowards(deliverySpot.transform.localPosition, startPositionDelivery, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        receiving= new List<System.Tuple<ItemShape, ItemColor, string>>();
        activated = false;
        while (Vector3.Distance(transform.localPosition, startPosition) > 0.01f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPosition, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return null;
        }
        yield break;

    }
    public new IEnumerator ActivateTray(Item r){
        
        while (Vector3.Distance(deliverySpot.transform.localPosition, endPositionDelivery) > 0.1f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            deliverySpot.transform.localPosition = Vector3.MoveTowards(deliverySpot.transform.localPosition, endPositionDelivery, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Debug.Log("Delivery Tray - Activating tray");
        //Animation tray activation
        while (Vector3.Distance(transform.localPosition, endPosition) > 0.1f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Debug.Log("Tray activated");
        activated = true;
        yield break;
    }
    public void ValidateDelivery(){
        if(buttonDelivery){
            if(validationTablet.GetValidation()){
                GatherDelivery();
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(!buttonDelivery){
            delivery = other.gameObject;
            GatherDelivery();
        }
        
    }
    
}
