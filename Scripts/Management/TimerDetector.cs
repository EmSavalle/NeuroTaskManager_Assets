using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimerDetector : MonoBehaviour
{
    public TaskManager taskManager;
    // Start is called before the first frame update
    public TimerDetectorEnter timerDetectorEnter;
    public GameObject objectEntered;
    public float timeEntered;
    public List<float> timeMeasured;
    public List<float> results;
    public TaskType taskType;
    public TaskDifficulty taskDifficulty;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other){
        Item i = other.gameObject.GetComponent<Item>();
        if(i == null && other.transform.parent != null){
            i = other.transform.parent.gameObject.GetComponent<Item>();
        }
        if(i != null && other.gameObject == objectEntered){
            float time = Time.time - timeEntered;
            if(taskManager.currentDifficulty != taskDifficulty || taskManager.currentTaskType != taskType){
                taskManager.currentDifficulty = taskDifficulty;
                taskManager.currentTaskType = taskType;
                if(timeMeasured.Count>0){
                    results.Add(timeMeasured.Average());
                    timeMeasured = new List<float>();
                }
            }
        }
    } 
}
