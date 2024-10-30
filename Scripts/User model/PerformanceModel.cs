using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceModel : UserModel
{
    public List<PerformanceTaskResult> performanceTaskResults;
    public Dictionary<TaskType,int> taskMaxPerformances = new Dictionary<TaskType, int>();
    // Start is called before the first frame update
    void Start()
    {
        
        taskMaxPerformances[TaskType.MATCHING]=12*tm.task1stDuration;
        taskMaxPerformances[TaskType.BOXING]=12*tm.task1stDuration;
        taskMaxPerformances[TaskType.COUNTING]=12*tm.task1stDuration;
        taskScore = new List<Tuple<TaskType, float>>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void ProcessResults(){
        
        
    }
    public override void ProcessTask(TaskResults tr)
    {
        base.ProcessTask(tr);
        PerformanceTaskResult ptr = new PerformanceTaskResult();
        ptr.taskType = tr.taskType;
        ptr.taskDifficulty = tr.taskDifficulty;
        ptr.numberOfError = tr.numberOfError;
        ptr.numberOfMissed = tr.numberOfMissed;
        ptr.numberOfSuccess = tr.numberOfSuccess;
        ptr = ComputeScore(ptr);
        performanceTaskResults.Add(ptr);
        Tuple<TaskType,float> tp = new Tuple<TaskType, float>(ptr.taskType,ptr.score);
        taskScore.Add(tp);
    }

    public PerformanceTaskResult ComputeScore(PerformanceTaskResult ptr){
        float performance,errorRate,score;
        if(taskMaxPerformances.ContainsKey(ptr.taskType)){
            performance = (ptr.numberOfError+ptr.numberOfSuccess)/taskMaxPerformances[ptr.taskType];
            if((ptr.numberOfError+ptr.numberOfSuccess) != 0){
                errorRate = ptr.numberOfError/(ptr.numberOfError+ptr.numberOfSuccess);
            }
            else{
                errorRate = 1;
            }
            score = (ptr.numberOfSuccess-ptr.numberOfError)/taskMaxPerformances[ptr.taskType];
        }
        else{
            performance = (ptr.numberOfError+ptr.numberOfSuccess)/(ptr.numberOfError+ptr.numberOfSuccess+ptr.numberOfMissed);
            if((ptr.numberOfError+ptr.numberOfSuccess) != 0){
                errorRate = ptr.numberOfError/(ptr.numberOfError+ptr.numberOfSuccess);
            }
            else{
                errorRate = 1;
            }
            score = (ptr.numberOfSuccess-ptr.numberOfError)/(ptr.numberOfError+ptr.numberOfSuccess+ptr.numberOfMissed);
        }
        ptr.performance = performance;
        ptr.errorRate = errorRate;
        ptr.score = score;
        return ptr;
    }
}

[Serializable]
public struct PerformanceTaskResult{
    public ConditionType conditionType;
    public TaskType taskType;
    
    public TaskDifficulty taskDifficulty;
    public int numberOfError;
    public int numberOfMissed;
    public int numberOfSuccess;
    public float performance;
    public float errorRate;
    public float score;
    public int taskDuration;
}
