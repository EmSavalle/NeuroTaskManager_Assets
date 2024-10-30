using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidationTray : MonoBehaviour
{
    public bool destroyWhenReceived;
    //public ItemType receiving;
    public List<Tuple<ItemShape,ItemColor,int>> receiving = new List<Tuple<ItemShape, ItemColor, int>>();
    public bool activated;

    public Vector3 startPosition, endPosition;
    public float moveSpeed;

    public ValidationTrayType type;

    public ParticipantInfos participantInfos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator ActivateTray(Item r){
        Debug.Log("Base tray - Activating tray");
        //Animation tray activation
        while (Vector3.Distance(transform.localPosition, endPosition) > 0.01f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Debug.Log("Tray activated");
        activated = true;
        receiving= new List<Tuple<ItemShape, ItemColor, int>>();
        Tuple<ItemShape, ItemColor, int> tp = new Tuple<ItemShape, ItemColor, int>(r.itemShape,r.itemColor,r.itemNumber);
        receiving.Add(tp);
        yield break;
    }
    public IEnumerator ActivateTray(List<Item> rs){
        Debug.Log("Base tray - Activating tray");
        //Animation tray activation
        while (Vector3.Distance(transform.localPosition, endPosition) > 0.01f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Debug.Log("Tray activated");
        activated = true;
        receiving= new List<Tuple<ItemShape, ItemColor, int>>();
        foreach(Item r in rs){
            Tuple<ItemShape, ItemColor, int> tp = new Tuple<ItemShape, ItemColor, int>(r.itemShape,r.itemColor,r.itemNumber);
            receiving.Add(tp);
        }
        yield break;
    }
    public IEnumerator ActivateTray(List<GameObject> rs){
        Debug.Log("Base tray - Activating tray");
        //Animation tray activation
        while (Vector3.Distance(transform.localPosition, endPosition) > 0.01f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Debug.Log("Tray activated");
        activated = true;
        receiving= new List<Tuple<ItemShape, ItemColor, int>>();
        foreach(GameObject go in rs){
            Item r = go.GetComponent<Item>();
            if(r == null){r=go.transform.parent.GetComponent<Item>();}
            Tuple<ItemShape, ItemColor, int> tp = new Tuple<ItemShape, ItemColor, int>(r.itemShape,r.itemColor,r.itemNumber);
            receiving.Add(tp);
        }
        yield break;
    }

    public IEnumerator DeactivateTray(){
        receiving= new List<Tuple<ItemShape, ItemColor, int>>();
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
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Validation tray - item received");
        Item it = other.gameObject.GetComponent<Item>();
        if(it == null && other.transform.parent != null){
            it = other.transform.parent.gameObject.GetComponent<Item>();
        }
        if(it != null){
            if(ValidateItem(it)){
                Debug.Log("Validation tray - right item");
                participantInfos.TaskSuccess();
            }
            else{
                Debug.Log("Validation tray - wrong item");
                participantInfos.TaskError();
            }
            if(destroyWhenReceived){
                Destroy(other.gameObject);
            }

        }
    }
    public bool ValidateItem(Item i){
        
        Tuple<ItemShape, ItemColor, int> tp = new Tuple<ItemShape, ItemColor, int>(i.itemShape,i.itemColor,i.itemNumber);

        return receiving.Contains(tp);
    }
}

public enum ValidationTrayType {NONE,BINS,TRAY,BOX,DELIVERY}