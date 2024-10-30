using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserModel : MonoBehaviour
{
    public TaskManager tm;
    public string modelName;
    public List<Tuple<TaskType,float>> taskScore;
    public int taskScoreNumber;
    
    void Start()
    {
        taskScore = new List<Tuple<TaskType, float>>();
    }

    // Update is called once per frame
    void Update()
    {
        taskScoreNumber = taskScore.Count();
    }

    public virtual void ComputeSolution(List<TaskResults> taskResults)
    {
        foreach(TaskResults tr in taskResults){
            ProcessTask(tr);
        }
        ProcessResults();
    }

    public virtual void ProcessTask(TaskResults tr){

    }
    public virtual void ProcessResults(){

    }

    public void CalibrateModel() 
    { 

    }
    public  List<TaskType> SortTasksByScore()
    {
        // Use LINQ to order the list by the second item (int) in descending order and select the TaskType
        var sortedTaskTypes = taskScore
                              .OrderByDescending(t => t.Item2)  // Sort by score (int)
                              .Select(t => t.Item1)             // Select TaskType
                              .ToList();                        // Convert to List<TaskType>

        return sortedTaskTypes;
    }

    public List<TaskType> ProvideTasks(int tasknumber){
        List<TaskType> taskTypes = new List<TaskType>();
        List<TaskType> sortedTask = SortTasksByScore();
        for (int i = 0; i < tasknumber; i++){
            taskTypes.Add(sortedTask[i]);
        }
        return taskTypes;
    }
}
