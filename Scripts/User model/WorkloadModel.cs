using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkloadModel : UserModel
{
    public List<WorkloadTaskResult> workloadTaskResults;
    // Start is called before the first frame update
    void Start()
    {
        
        taskScore = new List<Tuple<TaskType, float>>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void ProcessResults(){
        float minWork=1,maxWork=0,avgWork=0;
        foreach(WorkloadTaskResult wtr in workloadTaskResults){
            minWork = Mathf.Min(wtr.minWorkload, minWork);
            maxWork = Mathf.Max(wtr.maxWorkload, maxWork);
        }
        avgWork = (minWork+maxWork)/2;
        for (int i = 0 ; i < workloadTaskResults.Count; i++){
            WorkloadTaskResult wtr = workloadTaskResults[i];
            float score = ComputeScore(wtr,avgWork,minWork,maxWork);
            wtr.score = score;
            wtr.overallMaxWorkload = maxWork;
            wtr.overallMeanWorkload = avgWork;
            wtr.overallMinWorkload = minWork;
            workloadTaskResults[i]=wtr;
            Tuple<TaskType,float> tp = new Tuple<TaskType, float>(wtr.taskType,wtr.score);
            taskScore.Add(tp);
        }
    }
    public override void ProcessTask(TaskResults tr)
    {
        base.ProcessTask(tr);
        WorkloadTaskResult wtr = new WorkloadTaskResult();
        wtr.taskType = tr.taskType;
        wtr.conditionType = tr.conditionType;
        wtr.workloads = tr.workloads;
        wtr.minWorkload = tr.workloads.Min();
        wtr.maxWorkload = tr.workloads.Max();
        wtr.meanWorkload = tr.workloads.Average();
        float score = ComputeLocalScore(wtr);
        wtr.localScore = score;
        workloadTaskResults.Add(wtr);
    }

    public float ComputeLocalScore(WorkloadTaskResult wtr){
        float workMean = wtr.meanWorkload;
        float workMax = wtr.maxWorkload;
        float workMin = wtr.minWorkload;
        return (workMean - ((workMax+workMin)/2))/(workMax-workMin);
    }
    public float ComputeScore(WorkloadTaskResult wtr,float avgWork, float minWork, float maxWork){
        float workMean = wtr.meanWorkload;
        return (workMean - avgWork)/(maxWork-minWork);
    }
}
[Serializable]
public struct WorkloadTaskResult{
    public ConditionType conditionType;
    public TaskType taskType;
    public float minWorkload;
    public float maxWorkload;
    public float meanWorkload;
    public List<float> workloads;

    public float localScore;

    public float overallMinWorkload,overallMaxWorkload,overallMeanWorkload;
    public float score;
}