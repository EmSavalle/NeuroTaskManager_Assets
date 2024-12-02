using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkloadListener : WorkloadManager
{
    public ParticipantInfos participantInfos;
    public LSLManager lSLManager;
    public float refreshTime = 1;
    public int streamIndex = -1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override float GenerateWorkload(){
        Debug.Log("Generate workload");
        float workload = 0f;
        if(streamIndex == -1 && lSLManager!=null){
            for (int i = 0; i < lSLManager.lSLStreams.Count; i++){
                if(lSLManager.lSLStreams[i].letType == LetType.WORKLOAD){
                    streamIndex = i;
                }
            }
        }
        if(streamIndex != -1 && lSLManager!=null){
            LSLStream ls = lSLManager.lSLStreams[streamIndex];
            
            if(ls.receivedData.Count>0){
                workload = ls.receivedData.Average();
                lSLManager.ResetListWorkload();
            }
            
        }
       
        return workload;
    }
    public float Median(List<float> numbers)
    {
        if (numbers.Count == 0)
            return 0;

        numbers = numbers.OrderBy(n=>n).ToList(); 

        var halfIndex = numbers.Count()/2; 

        if (numbers.Count() % 2 == 0)
            return (numbers[halfIndex] + numbers[halfIndex - 1]) / 2;

        return numbers[halfIndex];
    }
}
