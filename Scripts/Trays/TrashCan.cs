using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : ValidationTray
{
    public bool noFailure;
    public List<ItemType> recordMiss;
    public bool recordNBack;

    public bool isDisabled;
    public Queue<Tuple<ItemShape,ItemColor,string>> nbackRecords;
    public TaskManager taskManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void StudyNback(Item it){
        TaskDifficulty taskDifficulty= taskManager.currentDifficulty;
        int id = 0;
        for (int i = 0; i < taskManager.nBackTasks.Count; i++){
            if(taskManager.nBackTasks[i].taskDifficulty == taskDifficulty){
                id = i;
            }
        }
        int nCount = taskManager.nBackTasks[id].nbackNumber;
        if(isNback(it)){
            participantInfos.TaskError();
        }
        while(nbackRecords.Count>=nCount){
            nbackRecords.Dequeue();
        }
        Tuple<ItemShape,ItemColor,string> tp = new Tuple<ItemShape, ItemColor, string>(it.itemShape,it.itemColor,it.itemText);
        nbackRecords.Enqueue(tp);
    }
    public bool isNback(Item i){
        Tuple<ItemShape,ItemColor,string> tp = new Tuple<ItemShape, ItemColor, string>(i.itemShape,i.itemColor,i.itemText);
        return nbackRecords.Contains(tp);
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
                if(it.target){
                    participantInfos.TaskMissed();
                    Destroy(destroy);
                }else if(it != null && !noFailure){
                    participantInfos.TaskMissed();
                    Destroy(destroy);
                }
                else{
                    Destroy(destroy);
                }
            }
        }
    }
}
