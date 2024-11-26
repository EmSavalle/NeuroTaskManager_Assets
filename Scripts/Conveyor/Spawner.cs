using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
public class Spawner : MonoBehaviour
{
    public TaskManager tm;

    public Transform spreadMin,spreadMax;
    public bool spawning;
    public SpawnerType type;

    public GameObject stopper;
    //Batch spawning
    public int[] objectCounts;
    public bool batchSpawned = false;
    public BoxCollider batchCheckerCollider; // Assign your BoxCollider in the inspector
    public string itemTag = "Item";

    //Interactions
    public ConveyorBelt belt;


    public ParticipantInfos participantInfos;
    private static System.Random rng = new System.Random(); 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator StartSpawning(Task t){
        participantInfos.StartProcess();
        if(t.taskType == TaskType.NBACK){

            Debug.Log("Spawner - Start spawning nback");
            yield return StartCoroutine(StartSpawningNback(t));
        }
        else if(type == SpawnerType.CONTINUOUS){
            Debug.Log("Spawner - Start spawning continuous");
            yield return StartCoroutine(StartSpawningContinuous(t));
        }
        
        participantInfos.EndProcess();
    }
    public IEnumerator StartSpawningContinuous(Task t){
        if(tm.verbose){
            Debug.Log("Spawner - Spawning started");
        }
        if(t.taskType == TaskType.COLORSHAPE){
            yield return StartCoroutine(tm.UpdateColorShapeTask());
        }
        spawning=true;
        System.Random rnd = new System.Random();
        float duration = t.duration;
        float startTime = Time.time;
        float lastSpawn = 0f;
        List<GameObject> items = t.objects;
        int currentBatchItem = 0;
        while(Time.time<startTime+duration ){
            switch(t.taskType){
                case TaskType.COLORSHAPE:
                    
                    if(lastSpawn+t.deliveryTime<Time.time&& (!t.clearForSpawn ||(t.clearForSpawn && belt.isEmpty()))){
                        yield return StartCoroutine(tm.UpdateColorShapeTask());
                        
                        lastSpawn = Time.time;
                        GameObject it = items[rnd.Next(items.Count)];
                        Vector3 randomPosition = new Vector3((spreadMin.localPosition.x+ spreadMax.localPosition.x)/2, (spreadMin.localPosition.y+ spreadMax.localPosition.y)/2,(spreadMin.localPosition.z+ spreadMax.localPosition.z)/2)+spreadMax.parent.position;
                        GameObject go = Instantiate(it,randomPosition,Quaternion.identity);
                        go.SetActive(true);
                        go.transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(0, 4) * 90, transform.eulerAngles.z);
                    }
                    break;
                case TaskType.GONOGO:
                    if(lastSpawn+t.deliveryTime<Time.time&& (!t.clearForSpawn ||(t.clearForSpawn && belt.isEmpty()))){
                        GonoGoTask g; int indGo=0;
                        for (int i = 0; i < tm.gonoGoTasks.Count; i++){
                            if(tm.gonoGoTasks[i].taskDifficulty == tm.currentDifficulty){
                                indGo = i;
                            }
                        }
                        g = tm.gonoGoTasks[indGo];
                        lastSpawn = Time.time;
                        GameObject it = items[rnd.Next(items.Count)];
                        bool isGo = UnityEngine.Random.Range(0,100) <= g.percentageGo;
                        while(it.GetComponent<Item>().target != isGo){
                            
                            it = items[rnd.Next(items.Count)];
                            yield return null;
                        }
                        Vector3 randomPosition = new Vector3((spreadMin.localPosition.x+ spreadMax.localPosition.x)/2, (spreadMin.localPosition.y+ spreadMax.localPosition.y)/2,(spreadMin.localPosition.z+ spreadMax.localPosition.z)/2)+spreadMax.parent.position;
                        GameObject go = Instantiate(it,randomPosition,Quaternion.identity);
                        //go.GetComponent<Item>().target=isGo;
                        go.SetActive(true);
                        go.transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(0, 4) * 90, transform.eulerAngles.z);
                    }
                    break;
                case TaskType.MATCHING:
                    if(lastSpawn+t.deliveryTime<Time.time&& (!t.clearForSpawn ||(t.clearForSpawn && belt.isEmpty()))){
                        
                        lastSpawn = Time.time;
                        GameObject it = items[rnd.Next(items.Count)];
                        
                        Vector3 randomPosition = new Vector3(
                            UnityEngine.Random.Range(spreadMin.localPosition.x, spreadMax.localPosition.x), 
                            UnityEngine.Random.Range(spreadMin.localPosition.y, spreadMax.localPosition.y), 
                            UnityEngine.Random.Range(spreadMin.localPosition.z, spreadMax.localPosition.z)
                        )+spreadMax.parent.position;
                        GameObject go = Instantiate(it,randomPosition,Quaternion.identity);
                        go.SetActive(true);
                        go.transform.eulerAngles = new Vector3(transform.eulerAngles.x, UnityEngine.Random.Range(0, 4) * 90, transform.eulerAngles.z);
                    }
                    break;
                
                default:
                    Debug.Log("Default");
                    break;
            }
            
            yield return new WaitForSeconds(Time.deltaTime);
        }
        if(tm.verbose){
            Debug.Log("Spawner - Spawning ended");
        }
        spawning=false;
        
        yield return StartCoroutine(belt.EmptyBelt());
        if(tm.verbose){
            Debug.Log("Spawner - Batch cleared");
        }
        yield break;
    }
    public IEnumerator StartSpawningNback(Task t){
        if(tm.verbose){
            Debug.Log("Spawner - Spawning started");
        }
        spawning=true;
        System.Random rnd = new System.Random();
        TaskDifficulty td = t.taskDifficulty;
        List<GameObject> items = t.objects;
        List<int> numbers = new List<int>(); 
        int countNb = 0;
        int nbackNumber = -1;
        List<bool> nbacks = new List<bool>(); 
        foreach (NBack nb in tm.nBackTasks){
            if(nb.taskDifficulty == td){
                numbers = nb.numbers;
                nbacks = nb.nbacks; 
                nbackNumber = nb.nbackNumber;
            }
        }
        float duration = t.duration;
        float startTime = Time.time;
        float lastSpawn = 0f;
        
        while(Time.time<startTime+duration && countNb < numbers.Count){
            if(lastSpawn+t.deliveryTime<Time.time && (!t.clearForSpawn ||(t.clearForSpawn && belt.isEmpty()))){
                lastSpawn = Time.time;

                int nb = numbers[countNb];
                bool isNb = nbacks[countNb];
                Vector3 randomPosition = new Vector3((spreadMin.localPosition.x+ spreadMax.localPosition.x)/2, (spreadMin.localPosition.y+ spreadMax.localPosition.y)/2,(spreadMin.localPosition.z+ spreadMax.localPosition.z)/2)+spreadMax.parent.position;
                GameObject go = Instantiate(t.objects[nb],randomPosition,Quaternion.identity);
                go.GetComponent<Item>().target=false;
                go.SetActive(true);
                countNb+=1;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        participantInfos.ComputeLastNBack();
        if(tm.verbose){
            Debug.Log("Spawner - Spawning ended");
        }
        spawning=false;
        
        yield return StartCoroutine(belt.EmptyBelt());
        if(tm.verbose){
            Debug.Log("Spawner - Batch cleared");
        }
        yield break;
    }
   
    public IEnumerator ClearBatch(){
        stopper.SetActive(false);
        while(CheckClearedBatch()){
            yield return new WaitForSeconds(Time.deltaTime);
        }
        batchSpawned = false;
        stopper.SetActive(true);
        yield break;
    }
    public IEnumerator WaitForBatchClear(){
        while(CheckClearedBatch()){yield return new WaitForSeconds(Time.deltaTime);}
    }
    public bool CheckClearedBatch(){
        return !belt.isEmpty();
        /*// Get the center and size of the box collider in world space
        Vector3 boxCenter = batchCheckerCollider.transform.TransformPoint(batchCheckerCollider.center);
        Vector3 boxSize = batchCheckerCollider.size * 0.5f; // Half the size because OverlapBox uses extents

        // Get all colliders within the box
        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize, batchCheckerCollider.transform.rotation);

        // Check if any collider has the tag "Item"
        foreach (Collider col in colliders)
        {
            if (col.CompareTag(itemTag))
            {
                return true; // Exit if an item is found
            }
        }

        return false;*/
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
