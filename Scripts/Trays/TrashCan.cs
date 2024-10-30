using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : ValidationTray
{
    public bool noFailure;
    public List<ItemType> recordMiss;
    public bool recordNBack;

    public bool isDisabled;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetRecordNback(){
        recordNBack = true;
    }
    public void SetRecord(List<Item> items){
        foreach(Item it in items){
            if(!recordMiss.Contains(it.itemType)){
                recordMiss.Add(it.itemType);
            }
        }
        recordNBack = false;
    }
    public void SetRecord(List<GameObject> items){
        foreach(GameObject itt in items){
            Item it = itt.GetComponent<Item>();
            if(it == null && itt.transform.parent != null){
                it = itt.transform.parent.gameObject.GetComponent<Item>();
            }
            if(!recordMiss.Contains(it.itemType)){
                recordMiss.Add(it.itemType);
            }
        }
        recordNBack = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(!isDisabled){
            
            Item it = other.gameObject.GetComponent<Item>();
            GameObject destroy = other.gameObject;
            if(it == null && other.transform.parent != null){
                it = other.transform.parent.gameObject.GetComponent<Item>();
                destroy = other.transform.parent.gameObject;
            }
            if(recordNBack && it.nback){
                participantInfos.TaskMissed();
                Destroy(destroy);
            }else if(it != null && !noFailure && !recordNBack){
                participantInfos.TaskMissed();
                Destroy(destroy);
            }
            else{
                Destroy(destroy);
            }
        }
    }
}
