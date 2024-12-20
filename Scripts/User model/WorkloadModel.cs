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
        
        taskScore = new List<Tuple<TaskType,TaskDifficulty, float>>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void ProcessResults(){
        float minWork=1,maxWork=0,avgWork=0;
        List<float> allworks = new List<float>();
        foreach(WorkloadTaskResult wtr in workloadTaskResults){
            allworks.AddRange(wtr.workloads);
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
            wtr.overallMedianWorkload = Median(allworks);
            workloadTaskResults[i]=wtr;
            Tuple<TaskType,TaskDifficulty,float> tp = new Tuple<TaskType, TaskDifficulty, float>(wtr.taskType, wtr.taskDifficulty,wtr.score);
            taskScore.Add(tp);
        }
    }
    public override void ProcessTask(TaskResults tr)
    {
        base.ProcessTask(tr);
        WorkloadTaskResult wtr = new WorkloadTaskResult();
        wtr.taskType = tr.taskType;
        wtr.conditionType = tr.conditionType;
        wtr.taskDifficulty = tr.taskDifficulty;
        wtr.workloads = tr.workloads;
        wtr.workloads = TrimFivePercent(tr.workloads);
        wtr.minWorkload = wtr.workloads.Min();
        wtr.maxWorkload = wtr.workloads.Max();
        wtr.meanWorkload = Median(wtr.workloads);
        float score = ComputeLocalScore(wtr);
        wtr.localScore = score;
        workloadTaskResults.Add(wtr);
    }

    public float ComputeLocalScore(WorkloadTaskResult wtr){
        float workMean = wtr.meanWorkload;
        float workMax = wtr.maxWorkload;
        float workMin = wtr.minWorkload;
        float median = wtr.overallMedianWorkload;
        //MAD Score
        /*List<float> madScore= new List<float>();
        for(int i = 0; i < wtr.workloads.Count; i++){
            madScore.Add(Math.Abs(median-wtr.workloads[i]));
        }
        return Median(madScore);*/
        return 1-Math.Abs((workMean - ((workMax+workMin)/2))/(workMax-workMin));
    }
    public float ComputeScore(WorkloadTaskResult wtr,float avgWork, float minWork, float maxWork){
        float workMean = wtr.meanWorkload;
        return 1-Math.Abs((workMean - avgWork)/(maxWork-minWork));
    }
    public static float Median(List<float> numbers)
    {
        if (numbers == null || numbers.Count == 0)
        {
            throw new ArgumentException("The list cannot be null or empty.");
        }

        // Sort the list
        var sortedNumbers = numbers.OrderBy(n => n).ToList();
        int count = sortedNumbers.Count;

        // Find the median
        if (count % 2 == 0)
        {
            // If even, average the two middle values
            return (sortedNumbers[count / 2 - 1] + sortedNumbers[count / 2]) / 2f;
        }
        else
        {
            // If odd, return the middle value
            return sortedNumbers[count / 2];
        }
    }
    public static List<T> TrimFivePercent<T>(List<T> items)
    {
        if (items == null || items.Count < 20)
        {
            return items;
        }

        int removeCount = (int)(items.Count * 0.05); // Calculate 5% of the list size

        // Return the trimmed list, excluding the first and last 5% of items
        return items.GetRange(removeCount, items.Count - 2 * removeCount);
    }
}
[Serializable]
public struct WorkloadTaskResult{
    public ConditionType conditionType;
    public TaskType taskType;
    public TaskDifficulty taskDifficulty;
    public float minWorkload;
    public float maxWorkload;
    public float meanWorkload;
    public float overallMedianWorkload;
    public List<float> workloads;

    public float localScore;

    public float overallMinWorkload,overallMaxWorkload,overallMeanWorkload;
    public float score;
}
