using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidationTray : MonoBehaviour
{
    public bool destroyWhenReceived;
    //public ItemType receiving;
    public List<Tuple<ItemShape,ItemColor,string>> receiving = new List<Tuple<ItemShape, ItemColor, string>>();
    public bool activated;

    public Vector3 startPosition, endPosition;
    public float moveSpeed;

    public ValidationTrayType type;

    public ParticipantInfos participantInfos;
    public bool left;
    public TaskManager tm;
    public TrashCan trashCan;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator ActivateTray(Item r){
        //Animation tray activation
        while (Vector3.Distance(transform.localPosition, endPosition) > 0.01f)  
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return null;
        }
        activated = true;
        receiving= new List<Tuple<ItemShape, ItemColor, string>>();
        Tuple<ItemShape, ItemColor, string> tp = new Tuple<ItemShape, ItemColor, string>(r.itemShape,r.itemColor,r.itemText);
        receiving.Add(tp);
        yield break;
    }
    public IEnumerator ActivateTray(List<Item> rs){
        //Animation tray activation
        while (Vector3.Distance(transform.localPosition, endPosition) > 0.01f)  
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        activated = true;
        receiving= new List<Tuple<ItemShape, ItemColor, string>>();
        foreach(Item r in rs){
            Tuple<ItemShape, ItemColor, string> tp = new Tuple<ItemShape, ItemColor, string>(r.itemShape,r.itemColor,r.itemText);
            receiving.Add(tp);
        }
        yield break;
    }
    public IEnumerator ActivateTray(List<GameObject> rs){
        //Animation tray activation
        while (Vector3.Distance(transform.localPosition, endPosition) > 0.01f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        activated = true;
        receiving= new List<Tuple<ItemShape, ItemColor, string>>();
        foreach(GameObject go in rs){
            Item r = go.GetComponent<Item>();
            if(r == null){r=go.transform.parent.GetComponent<Item>();}
            Tuple<ItemShape, ItemColor, string> tp = new Tuple<ItemShape, ItemColor, string>(r.itemShape,r.itemColor,r.itemText);
            receiving.Add(tp);
        }
        yield break;
    }

    public IEnumerator DeactivateTray(){
        receiving= new List<Tuple<ItemShape, ItemColor, string>>();
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
        if(tm.currentTaskType == TaskType.COLORSHAPE){return ValidateItemColorShape(i);}
        if(tm.currentTaskType == TaskType.GONOGO){return ValidateItemGoNoGo(i);}
        if(tm.currentTaskType == TaskType.NBACK){return ValidateItemNBack(i);}
        Tuple<ItemShape, ItemColor, string> tp = new Tuple<ItemShape, ItemColor, string>(i.itemShape,i.itemColor,i.itemText);

        return receiving.Contains(tp);
    }
    public bool ValidateItemNBack(Item i){
        return trashCan.isNback(i);
    }
    public bool ValidateItemGoNoGo(Item i){
        TaskDifficulty td = tm.currentDifficulty;
        ItemColor itemColor = i.itemColor;
        ItemShape itemShape = i.itemShape;
        string itemText = i.itemText;
        bool isNumber = int.TryParse(itemText, out _);
        GonoGoTask gng=tm.gonoGoTasks[0];
        for (int j = 0; j < tm.gonoGoTasks.Count; j++){
            if(tm.gonoGoTasks[j].taskDifficulty==td){
                gng = tm.gonoGoTasks[j];
            }
        }
        bool color=gng.objectDimensions.Contains(ObjectDimension.COLOR);
        bool shape = gng.objectDimensions.Contains(ObjectDimension.SHAPE);
        bool text = gng.objectDimensions.Contains(ObjectDimension.TEXT);
        bool isFitting = true;
        if(color && itemColor != gng.aimedColor){
            isFitting = false;
        }
        if(shape&& itemShape != gng.aimedShape){
            isFitting = false;
        }
        if(text && (isNumber != (gng.aimedText==ItemText.NUMBER))){
            isFitting = false;
        }
        return isFitting;
    }
    public bool ValidateItemColorShape(Item i){
        TaskDifficulty td = tm.currentDifficulty;
        ItemColor itemColor = i.itemColor;
        ItemShape itemShape = i.itemShape;
        string itemText = i.itemText;
        bool isNumber = int.TryParse(itemText, out _);
        ColorShapeTask cst=tm.colorShapeTasks[0];
        for (int j = 0; j < tm.colorShapeTasks.Count; j++){
            if(tm.colorShapeTasks[j].taskDifficulty==td){
                cst = tm.colorShapeTasks[j];
            }
        }
        bool ret = false;
        if(cst.hasHyperDimension){
            ObjectDimension od = cst.currentDimension;
            string newSort="";
            switch(od){
                case ObjectDimension.COLOR:
                    newSort = cst.colorSorting[itemColor];
                    break;
                case ObjectDimension.TEXT:
                    if(isNumber){
                        newSort = cst.textSorting[ItemText.NUMBER];
                    }
                    else{
                        
                        newSort = cst.textSorting[ItemText.LETTER];
                    }
                    
                    break;
                case ObjectDimension.SHAPE:
                    newSort = cst.shapeSorting[itemShape];
                    break;

            }
            switch(newSort){
                case "TEXT":
                    if(isNumber && ((left && cst.textSort.Item1==ItemText.NUMBER)||(!left && cst.textSort.Item2==ItemText.NUMBER))){
                        ret = true;
                    }
                    if(!isNumber && ((left && cst.textSort.Item1==ItemText.LETTER)||(!left && cst.textSort.Item2==ItemText.LETTER))){
                        ret = true;
                    }
                    break;
                case "SHAPE":
                    if((left && cst.shapeSort.Item1 == itemShape)||(!left && cst.shapeSort.Item2 == itemShape)){
                        ret = true;
                    }
                    break;
                case "COLOR":
                    if((left && cst.colorSort.Item1 == itemColor)||(!left && cst.colorSort.Item2 == itemColor)){
                        ret = true;
                    }
                    break;
            }
            return ret;
        }
        else{
            ObjectDimension od = cst.currentDimension;
            switch(od){
                case ObjectDimension.COLOR:
                    if((left && cst.colorSort.Item1 == itemColor)||(!left && cst.colorSort.Item2 == itemColor)){
                        ret = true;
                    }
                    break;
                case ObjectDimension.TEXT:
                    if(isNumber && ((left && cst.textSort.Item1==ItemText.NUMBER)||(!left && cst.textSort.Item2==ItemText.NUMBER))){
                        ret = true;
                    }
                    if(!isNumber && ((left && cst.textSort.Item1==ItemText.LETTER)||(!left && cst.textSort.Item2==ItemText.LETTER))){
                        ret = true;
                    }
                    break;
                case ObjectDimension.SHAPE:
                    if((left && cst.shapeSort.Item1 == itemShape)||(!left && cst.shapeSort.Item2 == itemShape)){
                        ret = true;
                    }
                    break;

            }
            return ret;
        }
        //return false;
    }
}

public enum ValidationTrayType {NONE,BINS,TRAY,BOX,DELIVERY}