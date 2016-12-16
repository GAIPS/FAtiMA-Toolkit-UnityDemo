using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
   
    private int C_score;
    private int E_score;
    private int F_score;
    private int I_score;
    private int P_score;

    // Use this for initialization
    void Start ()
    {
       this.GetComponent<Text>().text = "";

	    C_score = 0;
        E_score = 0;
        F_score = 0;
        I_score = 0;
        P_score = 0;
        Refresh();
    }
	
	// Update is called once per frame
	void Update ()
	{

	 
	}

    public void Refresh()
    {
        string print = "";

       
            print += "  C:" + C_score;
       
            print += "  E:" + E_score;
        
            print += "  F:" + F_score;
      
            print += "  I:" + I_score;

        print += "  P:" + P_score;

        this.GetComponent<Text>().text = print;

    }
    public void AddC(int add)
    {
        Debug.Log("Added C");
        C_score += add;
        Refresh();
    }

    public void AddE(int add)
    {
        Debug.Log("Added E" + add);
        E_score += add;
        Refresh();
    }

    public void AddF(int add)
    {
        Debug.Log("Added F" + add);
        F_score += add;
        Refresh();
    }
    public void AddI(int add)
    {
        Debug.Log("Added I" + add);
        I_score += add;
        Refresh();
    }

    public void AddP(int add)
    {
        Debug.Log("Added P" + add);
        P_score += add;
        Refresh();
    }

    public void ResetScore()
    {

        E_score = 0;
        I_score = 0;
        F_score = 0;
        C_score = 0;
        P_score = 0;
    }

    public int getE()
    {
        return E_score;
    }

    public int getI()
    {
        return I_score;
    }


    public int getF()
    {
        return F_score;
    }


    public int getC()
    {
        return C_score;
    }

    public int getP()
    {
        return P_score;
    }




}
