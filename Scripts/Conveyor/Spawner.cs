using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
public class Spawner : MonoBehaviour
{
    public TaskManager tm;
    public List<Transform> spawnPositions;

    public Transform spreadMin,spreadMax;
    public bool spawning;
    public SpawnerType type;

    public GameObject stopper;
    public ConveyorStopper cs;
    //Batch spawning
    public int[] objectCounts;
    public bool batchSpawned = false;
    public bool batchCleared = true;
    public BoxCollider batchCheckerCollider; // Assign your BoxCollider in the inspector
    public string itemTag = "Item";

    //Interactions
    public CountingTablet cTablet;
    public TMP_Text infosText;
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
        else if(type == SpawnerType.BATCH){
            Debug.Log("Spawner - Start spawning batch");
            yield return StartCoroutine(StartSpawningBatch(t));
        }
        participantInfos.EndProcess();
    }
    public IEnumerator StartSpawningContinuous(Task t){
        if(tm.verbose){
            Debug.Log("Spawner - Spawning started");
        }
        spawning=true;
        System.Random rnd = new System.Random();
        float duration = t.duration;
        float startTime = Time.time;
        float lastSpawn = 0f;
        List<GameObject> items = t.objects;
        int currentBatchItem = 0;
        if(t.continuousBatch){
            items = Shuffle(items);
        }
        while(Time.time<startTime+duration){
            switch(t.taskType){
                case TaskType.SORTING:
                    if(lastSpawn+t.deliveryTime<Time.time){
                        
                        lastSpawn = Time.time;
                        GameObject it = items[rnd.Next(items.Count)];
                        if(t.continuousBatch){
                            it = items[currentBatchItem];
                            currentBatchItem++;
                            if(currentBatchItem == items.Count){
                                items = Shuffle(items);
                                currentBatchItem = 0;
                            }
                        }
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
                case TaskType.MATCHING:
                    if(lastSpawn+t.deliveryTime<Time.time){
                        
                        lastSpawn = Time.time;
                        GameObject it = items[rnd.Next(items.Count)];
                        if(t.continuousBatch){
                            it = items[currentBatchItem];
                            currentBatchItem++;
                            if(currentBatchItem == items.Count){
                                items = Shuffle(items);
                                currentBatchItem = 0;
                            }
                        }
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
                case TaskType.BOXING:
                    if(lastSpawn+t.deliveryTime<Time.time){
                        
                        lastSpawn = Time.time;
                        GameObject it = items[rnd.Next(items.Count)];
                        if(t.continuousBatch){
                            it = items[currentBatchItem];
                            currentBatchItem++;
                            if(currentBatchItem == items.Count){
                                items = Shuffle(items);
                                currentBatchItem = 0;
                            }
                        }
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
        List<bool> nbacks = new List<bool>(); 
        foreach (NBack nb in tm.nBackTasks){
            if(nb.taskDifficulty == td){
                numbers = nb.numbers;
                nbacks = nb.nbacks; 
            }
        }
        float duration = t.duration;
        float startTime = Time.time;
        float lastSpawn = 0f;
        
        if(t.continuousBatch){
            items = Shuffle(items);
        }
        while(Time.time<startTime+duration && countNb < numbers.Count){
            if(lastSpawn+t.deliveryTime<Time.time){
                lastSpawn = Time.time;

                int nb = numbers[countNb];
                bool isNb = nbacks[countNb];
                GameObject it = new GameObject();
                foreach (GameObject itt in items){
                    if(itt.GetComponent<Item>().itemNumber == nb){
                        it = itt;
                    }
                }
                Vector3 randomPosition = new Vector3((spreadMin.localPosition.x+ spreadMax.localPosition.x)/2, (spreadMin.localPosition.y+ spreadMax.localPosition.y)/2,(spreadMin.localPosition.z+ spreadMax.localPosition.z)/2)+spreadMax.parent.position;
                GameObject go = Instantiate(it,randomPosition,Quaternion.identity);
                go.SetActive(true);
                if(isNb){
                    go.GetComponent<Item>().nback = isNb;
                    participantInfos.TaskSuccess();
                }
                countNb++;
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
    public IEnumerator StartSpawningBatch(Task t){
        if(tm.verbose){
            Debug.Log("Spawner - Spawning started");
        }
        spawning=true;
        System.Random rnd = new System.Random();
        float duration = t.duration;
        float startTime = Time.time;
        float lastSpawn = Time.time;
        List<GameObject> items = t.objects;
        while(Time.time<startTime+duration){
            int[] objects = GenerateBatch(t);
            if(t.taskType==TaskType.COUNTING){
                System.Random rnd2 = new System.Random();
                int selec  = rnd2.Next(0, t.objects.Count);  // creates a number between 1 and 12
                Item it = t.objects[selec].GetComponent<Item>();
                if(it == null){
                    it = t.objects[selec].transform.parent.GetComponent<Item>();
                }
                ItemColor itemColor = it.itemColor;
                ItemShape itemShape = it.itemShape;
                int itemNumber = it.itemNumber;
                string text = "Count " + itemColor.ToString()+" "+ itemShape.ToString();
                if(itemNumber != 0){text+=" with number "+itemNumber.ToString();}
                infosText.text = text;
                yield return StartCoroutine(cTablet.LaunchCounting());
                int returnedValued = cTablet.validatedAnswer;
                if(returnedValued == objects[selec]){
                    participantInfos.TaskSuccess();
                }else{
                    
                    participantInfos.TaskError();
                }
                cTablet.Reset();
                yield return StartCoroutine(belt.EmptyBelt());
            }
            yield return null;
        }
        if(tm.verbose){
            Debug.Log("Spawner - Spawning ended");
        }
        yield return StartCoroutine(belt.EmptyBelt());
        spawning=false;
        yield break;
    }
    public int[] GenerateBatch(Task t){
        bool rndBatch = false;
        if(t.taskType == TaskType.COUNTING){
            rndBatch = true;
        }
        if(rndBatch){
            objectCounts = new int[t.objects.Count];
            for (int i = 0; i < t.batchAmount; i++)
            {
                // Randomly select an object type from the array
                int randomIndex = UnityEngine.Random.Range(0, t.objects.Count);
                GameObject selectedObject = t.objects[randomIndex];
                objectCounts[randomIndex]++;

                // Generate a random position within the bounds of minPosition and maxPosition
                Vector3 randomPosition = new Vector3(
                    UnityEngine.Random.Range(spreadMin.localPosition.x, spreadMax.localPosition.x), 
                    UnityEngine.Random.Range(spreadMin.localPosition.y, spreadMax.localPosition.y), 
                    UnityEngine.Random.Range(spreadMin.localPosition.z, spreadMax.localPosition.z)
                )+spreadMax.parent.position;

                // Instantiate the selected object at the random position
                GameObject go = Instantiate(selectedObject, randomPosition, Quaternion.identity);
                go.SetActive(true);
            }
        }
        else{
            objectCounts = new int[t.objects.Count];
            for (int i = 0; i < t.objects.Count; i++)
            {
                // Randomly select an object type from the array
                GameObject selectedObject = t.objects[i];
                objectCounts[i]++;

                // Generate a random position within the bounds of minPosition and maxPosition
                Vector3 randomPosition = new Vector3(
                    UnityEngine.Random.Range(spreadMin.position.x, spreadMax.position.x), 
                    UnityEngine.Random.Range(spreadMin.position.y, spreadMax.position.y), 
                    UnityEngine.Random.Range(spreadMin.position.z, spreadMax.position.z)
                );

                // Instantiate the selected object at the random position
                GameObject go = Instantiate(selectedObject, randomPosition, Quaternion.identity);
                go.SetActive(true);
            }
        }
        batchSpawned = true;
        return objectCounts;
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
        // Get the center and size of the box collider in world space
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

        return false;
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

public enum SpawnerType {CONTINUOUS,BATCH}
