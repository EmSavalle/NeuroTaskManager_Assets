using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ParticipantInfos : MonoBehaviour
{
    public string participantId;
    public int participantNumber;

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
        string filePath = "ParticipantNumber.txt";
        string content ="";
        try
        {
            // Read all text from the file
            content = File.ReadAllText(filePath);
            Console.WriteLine("Content of the file:");
            Console.WriteLine(content);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("File not found. Please check the file path.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        participantId = content;
        int number = 0;
        int.TryParse(participantId, out number);
        participantNumber = number;
        participantId += System.DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss");
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
    public void SaveQuestionnaireResultsToFile()
    {
        string filePath = Application.dataPath + "/Logs/" + "Questionnaires"+participantId+".txt";
        using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
        {
            foreach (var result in questionnaireResults)
            {
                writer.WriteLine("Questionnaire Type: " + result.questionnaireType);
                writer.WriteLine("Condition Type: " + result.conditionType);
                writer.WriteLine("Task Type: " + result.taskType);
                writer.WriteLine("Task Difficulty: " + result.taskDifficulty);
                writer.WriteLine("Questionnaire Answers: " + string.Join(", ", result.questionnaireAnswers));
                writer.WriteLine("-----"); // Separate entries with a line
            }
        }
    }
    public void SaveTaskResultsToFile()
    {
        string filePath = Application.dataPath + "/Logs/" + "Results"+participantId+".txt";
        using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
        {
            foreach (var result in taskResults)
            {
                writer.WriteLine("Task Type: " + result.taskType);
                writer.WriteLine("Condition Type: " + result.conditionType);
                writer.WriteLine("Task Difficulty: " + result.taskDifficulty);
                writer.WriteLine("Duration: " + result.duration + " seconds");

                // Write workloads list as a comma-separated string
                writer.WriteLine("Workloads: " + string.Join(", ", result.workloads));
                
                // Write error, success, and missed counts
                writer.WriteLine("Number of Errors: " + result.numberOfError);
                writer.WriteLine("Number of Successes: " + result.numberOfSuccess);
                writer.WriteLine("Number of Missed: " + result.numberOfMissed);


                writer.WriteLine("-----"); // Separate entries with a line
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
        //Debug.Log("Task Process - Success !");
        TaskResults tr = taskResults[^1];
        tr.numberOfSuccess+=1;
        taskResults[^1]=tr;
    }
    public void TaskError(){
        //Debug.Log("Task Process - Error !");
        TaskResults tr = taskResults[^1];
        tr.numberOfError+=1;
        taskResults[^1]=tr;
    }
    public void TaskMissed(){
        //Debug.Log("Task Process - Missed !");
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
    public List<float> questionnaireAnswers;
}

public enum QuestionnaireType {NASA,STFA,COMP}
