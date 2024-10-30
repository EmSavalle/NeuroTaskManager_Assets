using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class CountingTablet : Tablet
{
    public TMP_Text countDisplay;
    public List<CountingButton> answer;
    public List<int> currentAnswer = new List<int>();
    public int validatedAnswer;
    public CountingButton validation;
    public CountingButton correct;
    public List<int> answers = new List<int>();
    public bool validated;
    public override IEnumerator StartTablet(){
        yield return StartCoroutine(base.StartTablet());
    }
    public override IEnumerator EndTablet(){
        yield return StartCoroutine(base.EndTablet());
    }
    public IEnumerator LaunchCounting(){
        currentAnswer = new List<int>();
        while(!validated){
            yield return new WaitForSeconds(Time.deltaTime);
        }   
    }

    public void Reset(){
        currentAnswer = new List<int>();
        validatedAnswer=0;  
        validated=false;      
        updateTest();
    }
    public void Select(int value){
        if(value == -1){
            currentAnswer = new List<int>();
        }
        else if(value == 10){
            validated = true;   
            updateTest();
            validatedAnswer = Int32.Parse(countDisplay.text);
        }
        else{
            currentAnswer.Add(value);
        }
        updateTest();
    }
    public void updateTest(){
        string txt = "";
        foreach (int v in currentAnswer){
            if(txt.Length<5){
                txt = txt+((uint)v);
            }
            
        }
        countDisplay.text = txt;
        if(txt == ""){
            validatedAnswer = 0;    
        }
        else{
            validatedAnswer = Int32.Parse(countDisplay.text);    
        }        
    }
    public void OnTriggerEnter(Collider c){
        if(c.gameObject.name == "Interactor"){
            c.gameObject.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    public void OnTriggerExit(Collider c){
        if(c.gameObject.name == "Interactor"){
            c.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
