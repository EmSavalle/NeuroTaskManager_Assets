using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class BoxingTray : ValidationTray
{
    public List<GameObject> boxes;
    public bool randomizeBoxes;
    public int currentBox;
    public GameObject box;
    
    public GameObject boxSpot;
    public bool buttonBox;
    public GameObject validationTabletObj;
    public ValidationTablet validationTablet;
    public bool randomRotation;

    public bool stopBoxing;
    public bool boxingComplete;

    public Vector3 startPositionBox, endPositionBox;
    public bool isBoxing = false;
    private static System.Random rng = new System.Random();  

    public new List<BoxingRequirements> receiving;
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
        if(buttonBox){
            if(validationTablet.validated){
                boxingComplete = true;
                validationTablet.validated=false;
            }
        }   
        if(!buttonBox){
            if(boxSpot.transform.childCount>0){
                Item t = box.transform.GetChild(0).GetComponent<ReceiverItem>();
                if(t != null){
                    if(t.itemType == ItemType.COMPLETEDRECEIVER){
                        boxingComplete = true;
                    }
                }
            }
            
        }

    }

    public IEnumerator StartBoxing(List<GameObject> objects){
        if(buttonBox){
            validationTablet.GetComponent<Tablet>().StartTablet();
        }
        boxes = objects;
        if(randomizeBoxes){
            boxes = Shuffle(boxes);
        }
        stopBoxing = false;
        currentBox = 0;
        Debug.Log("Box Tray -Starting box");
        while(!stopBoxing){
            Debug.Log("Box Tray - looping box");
            if(boxingComplete){
                yield return StartCoroutine(DeliverObject());
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield break;
    }
    public void StopBox(){
        stopBoxing = true;
    }
    public IEnumerator DeliverObject(){
        if(isBoxing){
            while (Vector3.Distance(boxSpot.transform.localPosition, startPositionBox) > 0.01f)  // 0.01 is tolerance for close enough
            {
                // Move towards the target
                boxSpot.transform.localPosition = Vector3.MoveTowards(boxSpot.transform.localPosition, startPositionBox, moveSpeed * Time.deltaTime);
                
                // Wait for the next frame before continuing
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        if(boxSpot.transform.childCount!=0){
            GatherBox();
        }
        InstantiateDeliverable();
        while (Vector3.Distance(boxSpot.transform.localPosition, endPositionBox) > 0.01f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            boxSpot.transform.localPosition = Vector3.MoveTowards(boxSpot.transform.localPosition, endPositionBox, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        isBoxing=true;
        boxingComplete = false;
    }
    public void GatherBox(){
        if(CheckBox(box)){
            participantInfos.TaskSuccess();
        }
        else{
            participantInfos.TaskError();
        }
        Destroy(box);
        boxingComplete = true;
    }
    public bool CheckBox(GameObject box){
        Box box1 = box.GetComponent<Box>();

        return box1.CheckValidity(true);
    }
    public void InstantiateDeliverable(){
        Debug.Log("box Tray - Instantiating box");
        if(boxSpot.transform.childCount!=0){
            Destroy(boxSpot.transform.GetChild(0).gameObject);
        }
        if(currentBox<boxes.Count){
            GameObject go = Instantiate(boxes[0],boxSpot.transform.position, boxSpot.transform.rotation);
            SetupBox(go.GetComponent<Box>(),true);
            go.transform.parent = boxSpot.transform;
            currentBox ++;
            box = go;
        }
        if(currentBox==boxes.Count){
            if(randomizeBoxes){
               boxes = Shuffle(boxes);
            }
            currentBox=0;
        }
    }
    public void SetupBox(Box box, bool random){
        System.Random rnd = new System.Random();
        int r = rnd.Next(receiving.Count);
        BoxingRequirements br = receiving[r];
        box.weightRequirement=br.weight;
        box.itemsShapeGoals = br.itemShapes;
        box.itemsColorGoals = br.itemColors;
        box.shapeRequirements = br.shapesRequirements;
        box.colorRequirements = br.colorRequirements;
        box.weightRequirements = br.weightRequirements;
        if(br.numberRequirements.Count != 0){
            box.numberRequirement = br.numberRequirements[0].number;
            box.exactNumber = br.numberRequirements[0].exactNumber;
        }
        

    }
    public new IEnumerator DeactivateTray(){
        if(boxSpot.transform.childCount!=0){
            GatherBox();
        }
        while (Vector3.Distance(boxSpot.transform.localPosition, startPositionBox) > 0.01f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            boxSpot.transform.localPosition = Vector3.MoveTowards(boxSpot.transform.localPosition, startPositionBox, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        receiving= new List<BoxingRequirements>();
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
    public IEnumerator ActivateTray(List<BoxingRequirements> rs){
        
        while (Vector3.Distance(boxSpot.transform.localPosition, endPositionBox) > 0.1f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            boxSpot.transform.localPosition = Vector3.MoveTowards(boxSpot.transform.localPosition, endPositionBox, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Debug.Log("box Tray - Activating tray");
        //Animation tray activation
        while (Vector3.Distance(transform.localPosition, endPosition) > 0.1f)  // 0.01 is tolerance for close enough
        {
            // Move towards the target
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPosition, moveSpeed * Time.deltaTime);
            
            // Wait for the next frame before continuing
            yield return new WaitForSeconds(Time.deltaTime);
        }
        receiving = rs;
        Debug.Log("Tray activated");
        activated = true;
        yield break;
    }
    public void ValidateBox(){
        if(buttonBox){
            if(validationTablet.GetValidation()){
                GatherBox();
            }
        }
    }

    
}
