using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkloadSimulator : WorkloadManager
{
    public ParticipantInfos participantInfos;
    public float refreshTime = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override float GenerateWorkload(){
        return Random.Range(0f,1f);
    }
}
