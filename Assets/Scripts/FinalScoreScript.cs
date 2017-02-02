using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalScoreScript : MonoBehaviour
{
    private GameObject score;
    private float mood;
    public bool PJ;
	// Use this for initialization
	void Start ()
	{
	 
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void FinalScore(float mood)
    {

        score = GameObject.FindGameObjectWithTag("Score");

        string ret = "";


        if (PJ)
        {
            int A = score.GetComponent<ScoreManager>().getAggression();
            int I = score.GetComponent<ScoreManager>().getInformation();
            int T = score.GetComponent<ScoreManager>().getTruth();
        
            ret += "Suspect Aggression level: " + A + "\n";
            ret += "Information obtained: " + I + "\n";
            ret += "Truth: " + T + "\n";

        }
        else
        {
            int C = score.GetComponent<ScoreManager>().getC();
            int E = score.GetComponent<ScoreManager>().getE();
            int F = score.GetComponent<ScoreManager>().getF();
            int I = score.GetComponent<ScoreManager>().getI();
            int P = score.GetComponent<ScoreManager>().getP();

            ret += "Empathy: " + E + "\n";
            ret += "Closure: " + C + "\n";
            ret += "FAQ usage: " + F + "\n";
            ret += "Inquire: " + I + "\n";
            ret += "Politeness: " + P + "\n";
        }





        ret += "Mood: " + Math.Round(mood, 2) + "\n";

        this.GetComponent<Text>().text = ret;
    }
}
