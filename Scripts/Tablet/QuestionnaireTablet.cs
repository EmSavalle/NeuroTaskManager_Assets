using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionnaireTablet : Tablet
{
    public TMP_Text question,tmin,tmax;
    public TMP_Text pannelText;
    public List<QuestionnaireButton> answer;
    public List<GameObject> buttonRelated;
    public List<GameObject> SliderRelated;
    public VRSlider vRSlider;
    public int currentAnswer;
    public QuestionnaireButton validation;
    public List<int> answers = new List<int>();
    public bool validated;

    public bool isButton = true;
    public override IEnumerator StartTablet(){
        string filePath = Application.dataPath+"Logs"+".txt";
        using (StreamWriter writer = new StreamWriter(filePath, append: true)) // 'append: true' to append if file exists
        {
            writer.WriteLine("Starting questionnaire tablet"+startPosition.ToString()+"/"+endPosition.ToString());
        }
        pannelText.text = "Questionnaire time!";
        question.text = "Wait for questions";
        tmax.text="";
        tmin.text ="";
        yield return StartCoroutine(base.StartTablet());
    }public override IEnumerator EndTablet(){
        question.text = "Bye";
        tmax.text="";
        tmin.text ="";
        yield return StartCoroutine(base.EndTablet());
    }
    public IEnumerator LaunchQuestionnaire(Questionnaire q){
        Reset();
        validated = false;
        answers = new List<int>();
        for(int i = 0; i < q.questions.Count; i++){
            Reset();
            if(q.withSlider[i] && isButton){
                isButton = false;
                foreach (GameObject go in buttonRelated){
                    go.SetActive(false);
                }
                
                foreach (GameObject go in SliderRelated){
                    go.SetActive(true);
                }
            }
            else if(!q.withSlider[i] && !isButton){
                isButton = true;
                foreach (GameObject go in buttonRelated){
                    go.SetActive(true);
                }
                
                foreach (GameObject go in SliderRelated){
                    go.SetActive(false);
                }
            }
            question.text = q.questions[i];
            tmin.text= q.tmin[i];
            tmax.text = q.tmax[i];
            while(!validated){
                yield return new WaitForSeconds(Time.deltaTime);
            }
            Reset();
            answers.Add(currentAnswer);
        }
    }

    public void Reset(){
        for(int i = 0; i < answer.Count ; i++){
            answer[i].Unselect();
        }
        validation.Unselect();
        validated = false;
    }
    public void Select(int value){
        if(value != 0){
            for(int i = 0; i < answer.Count ; i++){
                if(answer[i].isTriggered && i+1 != value){
                    answer[i].Unselect();
                }
            }
            currentAnswer = value;
        }
        else{
            //Questionnaire validated
            if(!isButton){
                currentAnswer = (int)vRSlider.sliderValue;
            }
            bool anySelection = false;
            for(int i = 0; i < answer.Count ; i++){
                if(answer[i].isTriggered){
                    anySelection=true;
                }
            }
            if(anySelection){
                validated = true;
            }
            else{
                validation.Unselect();
            }
            
        }
    }
}
