using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticipantInfos : MonoBehaviour
{
    public string participantId;

    public List<QuestionnaireResults> questionnaireResults;
    public List<TaskResults> taskResults;
    public TaskType currentType;
    public ConditionType conditionType;

    public UserModel workloadModel;
    public UserModel performanceModel;

    [Header("Workload management")]
    public WorkloadManager workloadManager;
    public float workloadRefresh = 1;
    private float lastWorkload = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(taskResults.Count != 0){
            
            if(taskResults[^1].ongoingTask && lastWorkload+workloadRefresh < Time.time){
                ProcessWorkload(workloadManager.GenerateWorkload());
                lastWorkload = Time.time;
            }
        }
    }

    public void StartNewTask(Task t, TaskType taskType, ConditionType conditionType, TaskDifficulty taskDifficulty){
        TaskResults tr = new TaskResults();
        tr.ended = false;
        tr.taskType = taskType;
        tr.taskDifficulty = taskDifficulty;
        tr.conditionType = conditionType;
        tr.duration=t.duration;
        tr.workloads = new List<float>();
        taskResults.Add(tr);
    }
    public void StartProcess(){
        TaskResults tr = taskResults[^1];
        tr.ongoingTask=true;
        taskResults[^1]=tr;
    }
    public void EndProcess(){
        TaskResults tr = taskResults[^1];
        tr.ongoingTask=false;
        taskResults[^1]=tr;
    }

    public void EndTask(){
        TaskResults tr = taskResults[^1];
        tr.ended=true;
        taskResults[^1]=tr;
    }
    public void ComputeLastNBack(){
        if(taskResults[^1].taskType == TaskType.NBACK){
            TaskResults tr = taskResults[^1];
            tr.numberOfError = tr.numberOfMissed;
            tr.numberOfSuccess = tr.numberOfSuccess-tr.numberOfError;
        }
    }
    public void TaskSuccess(){
        TaskResults tr = taskResults[^1];
        tr.numberOfSuccess+=1;
        taskResults[^1]=tr;
    }
    public void TaskError(){
        TaskResults tr = taskResults[^1];
        tr.numberOfError+=1;
        taskResults[^1]=tr;
    }
    public void TaskMissed(){
        TaskResults tr = taskResults[^1];
        tr.numberOfMissed+=1;
        taskResults[^1]=tr;
    }
    public void ProcessWorkload(float workload){
        TaskResults tr = taskResults[^1];
        tr.workloads.Add(workload);
        taskResults[^1]=tr;
    }

}
[Serializable]
public struct TaskResults{
    public TaskType taskType;
    public ConditionType conditionType;
    public TaskDifficulty taskDifficulty;
    public int duration;
    //Metrics 
    public List<float> workloads;
    public int numberOfError;
    public int numberOfSuccess;
    public int numberOfMissed;

    public bool ongoingTask;
    public bool ended;
}

[Serializable]
public struct QuestionnaireResults{
    public QuestionnaireType questionnaireType;
    public ConditionType conditionType;
    public TaskType taskType;
    public TaskDifficulty taskDifficulty;
    public List<int> questionnaireAnswers;
}

public enum QuestionnaireType {NASA,STFA,COMP}
