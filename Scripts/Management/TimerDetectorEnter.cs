using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerDetectorEnter : MonoBehaviour
{
    public Blocker timerDetector;
    public bool isOriginal;
    // Start is called before the first frame update
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
        if(i != null && !isOriginal){
            timerDetector.objectEntered = other.gameObject;
            timerDetector.timeEntered = Time.time;
            timerDetector.waitingForObject = false;
            timerDetector.moveObject = false;
        }
        else if(i!= null && isOriginal){
            timerDetector.objectEntered = other.gameObject;
            timerDetector.timeEntered = Time.time;
            timerDetector.waitingForObject = false;
            timerDetector.moveObject = false;
            
            StartCoroutine(timerDetector.ResetObjectExited());
        }
    } 
}
