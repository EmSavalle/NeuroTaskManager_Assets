using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public class UserModel : MonoBehaviour
{
    public ModelType modelType;
    public TaskManager tm;
    public ParticipantInfos participantInfos;
    public string modelName;
    public List<Tuple<TaskType,TaskDifficulty,float>> taskScore;
    public int taskScoreNumber;

    public Solution solution;
    
    void Start()
    {
        taskScore = new List<Tuple<TaskType,TaskDifficulty, float>>();
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
        DetermineSolution();
    }
    public float findScore(TaskType tt, TaskDifficulty td){
        for (int i = 0; i < taskScore.Count; i++){
            if(taskScore[i].Item1 == tt && taskScore[i].Item2 == td){
                return taskScore[i].Item3;
            }
        }
        return 0;
    }
    public virtual void DetermineSolution(){
        List<TaskType> availableType = new List<TaskType>{TaskType.GONOGO,TaskType.NBACK,TaskType.COLORSHAPE};
        List<TaskDifficulty> availableDifficulty= new List<TaskDifficulty>{TaskDifficulty.LOW,TaskDifficulty.MEDIUM,TaskDifficulty.HIGH};
        List<Tuple<TaskType,TaskType,TaskType,float>> solutionScore = new List<Tuple<TaskType, TaskType, TaskType, float>>();
        for (int i = 0; i < availableType.Count; i++)
        {
            for (int j = 0; j < availableType.Count; j++)
            {
                if(i!=j){
                    for (int k = 0; k < availableType.Count; k++)
                    {
                        if(i != k && j != k){
                            Console.WriteLine($"Combination: {availableType[i]}, {availableType[j]}, {availableType[k]}");
                            float scoreLow = findScore(availableType[i],TaskDifficulty.LOW);
                            float scoreMedium = findScore(availableType[j],TaskDifficulty.MEDIUM);
                            float scoreHigh = findScore(availableType[k],TaskDifficulty.HIGH);
                            float totalScore = scoreHigh+scoreMedium+scoreLow;
                            solutionScore.Add(new Tuple<TaskType, TaskType, TaskType, float>(availableType[i],availableType[j],availableType[k],totalScore));
                        }                        
                    }
                }
                
            }
        }
        // Find the tuple with the highest Item4
        Tuple<TaskType,TaskType,TaskType,float> highestScoreTuple = solutionScore.OrderByDescending(tuple => tuple.Item4).FirstOrDefault();
        solution = new Solution();
        solution.modelType = modelType;
        solution.taskTypeLow = highestScoreTuple.Item1;
        solution.taskTypeMedium = highestScoreTuple.Item2;
        solution.taskTypeHigh = highestScoreTuple.Item3;
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
    public void SaveSolution()
    {
        string filePath = Application.dataPath+"Solution"+participantInfos.participantId+".txt";
        using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
        {
            writer.WriteLine("Model type : "+solution.modelType.ToString());
            writer.WriteLine("Task Low : "+solution.taskTypeLow.ToString());
            writer.WriteLine("Task Medium : "+solution.taskTypeMedium.ToString());
            writer.WriteLine("Task High : "+solution.taskTypeHigh.ToString());            
        }
    }
}
public struct Solution{
    public ModelType modelType;
    public TaskType taskTypeLow;
    public TaskType taskTypeMedium;
    public TaskType taskTypeHigh;
}
public enum ModelType{WORKLOAD,PERFORMANCE};
