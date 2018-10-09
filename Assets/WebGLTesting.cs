using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ActionLibrary;
using AssetManagerPackage;
using Assets.Scripts;
using UnityEngine;
using IntegratedAuthoringTool;
using RolePlayCharacter;
using WellFormedNames;
using WorldModel;
using IntegratedAuthoringTool.DTOs;

public class WebGLTesting : MonoBehaviour
{


    // Store the iat file
    private IntegratedAuthoringToolAsset _iat;

    //Store the characters
    private List<RolePlayCharacterAsset> _rpcList;

    //Store the World Model
    private WorldModelAsset _worldModel;


	// Use this for initialization
	void Start ()
	{
	//    AssetManager.Instance.Bridge = new AssetManagerBridge();



	    _iat = IntegratedAuthoringToolAsset.LoadFromString("{\r\n\t\"root\":\r\n\t\t{\r\n\t\t\t\"classId\": 0,\r\n\t\t\t\"ScenarioName\": \"Pepe Silvia\",\r\n\t\t\t\"Description\": \"A short conversation between the Player and a Character named Charlie. Charlie discovers that there is a major conspiracy within the company he works in. \",\r\n\t\t\t\"WorldModelSource\": \"PepeSilviaWorldModel.wmo\",\r\n\t\t\t\"CharacterSources\": [\"Charlie.rpc\", \"Player.rpc\"],\r\n\t\t\t\"Dialogues\": [\r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"Start\",\r\n\t\t\t\t\t\"NextState\": \"S1\",\r\n\t\t\t\t\t\"Meaning\": \"-\",\r\n\t\t\t\t\t\"Style\": \"-\",\r\n\t\t\t\t\t\"Utterance\": \"Hi how are you?\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-51AADAEB0C97ED7BD252B14D88309F6B\"\r\n\t\t\t\t}, \r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"S2\",\r\n\t\t\t\t\t\"NextState\": \"S3\",\r\n\t\t\t\t\t\"Meaning\": \"-\",\r\n\t\t\t\t\t\"Style\": \"Neutral\",\r\n\t\t\t\t\t\"Utterance\": \"I\'m feeling okay-ish, I\'ve got something to tell you\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-58C7E04BE265D8CC2D9A51D169C0C41B\"\r\n\t\t\t\t}, \r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"S3\",\r\n\t\t\t\t\t\"NextState\": \"S4\",\r\n\t\t\t\t\t\"Meaning\": \"-\",\r\n\t\t\t\t\t\"Style\": \"-\",\r\n\t\t\t\t\t\"Utterance\": \"What happened?\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-9CBB8AB9BF567189876DC3F84F766DFD\"\r\n\t\t\t\t}, \r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"S4\",\r\n\t\t\t\t\t\"NextState\": \"S5\",\r\n\t\t\t\t\t\"Meaning\": \"-\",\r\n\t\t\t\t\t\"Style\": \"-\",\r\n\t\t\t\t\t\"Utterance\": \"I\'ve just stumbled unto a major conspiracy !\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-2C10E0C6CD86488733DC9FB56095A896\"\r\n\t\t\t\t}, \r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"FeelingGreat\",\r\n\t\t\t\t\t\"NextState\": \"S6\",\r\n\t\t\t\t\t\"Meaning\": \"-\",\r\n\t\t\t\t\t\"Style\": \"-\",\r\n\t\t\t\t\t\"Utterance\": \"Oh thank god!\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-A112F1A6A1C6F499EB83AC65CFBC61F2\"\r\n\t\t\t\t}, \r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"S2\",\r\n\t\t\t\t\t\"NextState\": \"S3\",\r\n\t\t\t\t\t\"Meaning\": \"-\",\r\n\t\t\t\t\t\"Style\": \"Depressed\",\r\n\t\t\t\t\t\"Utterance\": \"Not that great actually\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-4A616F5A524EE7AAA5F8A495F2A2548F\"\r\n\t\t\t\t}, \r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"S1\",\r\n\t\t\t\t\t\"NextState\": \"S2\",\r\n\t\t\t\t\t\"Meaning\": \"Neutral\",\r\n\t\t\t\t\t\"Style\": \"-\",\r\n\t\t\t\t\t\"Utterance\": \"I\'m okay, how about you\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-D45D0CAB5CB3A9C4EC782678AACC3846\"\r\n\t\t\t\t}, \r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"S1\",\r\n\t\t\t\t\t\"NextState\": \"S2\",\r\n\t\t\t\t\t\"Meaning\": \"Sad\",\r\n\t\t\t\t\t\"Style\": \"-\",\r\n\t\t\t\t\t\"Utterance\": \"I\'m really sad, how about you?\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-9B384E09650832C1BF05F35C0D11D1AF\"\r\n\t\t\t\t}, \r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"S1\",\r\n\t\t\t\t\t\"NextState\": \"S2\",\r\n\t\t\t\t\t\"Meaning\": \"Happy\",\r\n\t\t\t\t\t\"Style\": \"-\",\r\n\t\t\t\t\t\"Utterance\": \"I\'m doing amazingly, how about you?\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-D5D435EF2A8FE2F34D3DECD7BA9C7048\"\r\n\t\t\t\t}, \r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"Sad\",\r\n\t\t\t\t\t\"NextState\": \"S2\",\r\n\t\t\t\t\t\"Meaning\": \"-\",\r\n\t\t\t\t\t\"Style\": \"-\",\r\n\t\t\t\t\t\"Utterance\": \"I\'m feeling pretty bad aswell\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-1AE6C0BC6D52FA7B7918E32948E30326\"\r\n\t\t\t\t}, \r\n\t\t\t\t{\r\n\t\t\t\t\t\"CurrentState\": \"S2\",\r\n\t\t\t\t\t\"NextState\": \"S3\",\r\n\t\t\t\t\t\"Meaning\": \"-\",\r\n\t\t\t\t\t\"Style\": \"Positive\",\r\n\t\t\t\t\t\"Utterance\": \"I\'m feeling great ! I\'ve got something to tell you!\",\r\n\t\t\t\t\t\"UtteranceId\": \"TTS-37E04E7AA23C7104DF9C248324A8DA01\"\r\n\t\t\t\t}]\r\n\t\t},\r\n\t\"types\": [\r\n\t\t{\r\n\t\t\t\"TypeId\": 0,\r\n\t\t\t\"ClassName\": \"IntegratedAuthoringTool.IntegratedAuthoringToolAsset, IntegratedAuthoringTool, Version=1.7.0.0, Culture=neutral, PublicKeyToken=null\"\r\n\t\t}]\r\n}");

	  /*Initialize the List
        _rpcList = new List<RolePlayCharacterAsset>();

	    foreach (var characterSouce in _iat.GetAllCharacterSources())
	    {

	        var rpc = RolePlayCharacterAsset.LoadFromFile(characterSouce.Source);

            // RPC must load its "sub-assets"
	        rpc.LoadAssociatedAssets();

            // Iat lets the RPC know all the existing Meta-Beliefs / Dynamic Properties
	        _iat.BindToRegistry(rpc.DynamicPropertiesRegistry);


            // A debug message to make sure we are correctly loading the characters
            Debug.Log("Loaded Character " + rpc.CharacterName);
	        
            _rpcList.Add(rpc);
	    }


        //Loading the WorldModel
	    if(_iat.m_worldModelSource.Source != "" && _iat.m_worldModelSource.Source != null)
	        _worldModel = WorldModel.WorldModelAsset.LoadFromFile(_iat.GetWorldModelSource().Source);

	    UpdateFunction();
        */



	    foreach (var d in _iat.GetAllDialogueActions())
	    {
	     Debug.Log(" Dialogues " + d.Utterance);   
	    }
	}


	
	// Update is called once per frame
    void UpdateFunction()
    {
        IAction finalDecision = null;
        String agentName = "";

        // a simple cycle to go through all the agents and get their decision
        foreach (var rpc in _rpcList)
        {
            
            // From all the decisions the rpc wants to perform we want the first one
            var decision = rpc.Decide().FirstOrDefault();

            if (decision != null)
            {

                agentName = rpc.CharacterName.ToString();
                finalDecision = decision;
                break;
            }

        }

        //If there was a decision I want to print it
       
        if (finalDecision != null)
        
        {
            Debug.Log(" The agent " + agentName + " decided to perform " + finalDecision.Name);

            //                                          NTerm: 0     1     2     3     4
            // If it is a speaking action it is composed by Speak ( [ms], [ns] , [m}, [sty])
            var currentState = finalDecision.Name.GetNTerm(1);
            var nextState = finalDecision.Name.GetNTerm(2);
            var meaning = finalDecision.Name.GetNTerm(3);
            var style = finalDecision.Name.GetNTerm(4);


            // Returns a list of all the dialogues given the parameters
            var dialog = _iat.GetDialogueActions(currentState, nextState, meaning, style).FirstOrDefault();

            //Let's print all available dialogues:

          if(dialog!=null)
             Debug.Log(agentName + " says: " + dialog.Utterance + " towards " + finalDecision.Target);


            var actualCurrentState = dialog.CurrentState;
            var actualNextState = dialog.NextState;
            var actualMeaning = dialog.Meaning;
            var actualStyle = dialog.Style;

            var actualActionName = "Speak(" + actualCurrentState + ", " + actualNextState + ", " + actualMeaning +
                                   ", " + actualStyle + ")";

         
            
            var eventName = EventHelper.ActionEnd((Name)agentName, (Name)actualActionName, finalDecision.Target);

            var consequences = _worldModel.Simulate(new Name[] {eventName} );


            foreach (var eff in consequences)
            {
                Debug.Log("Effect: " + eff.PropertyName + " " + eff.NewValue + " " + eff.ObserverAgent);

                foreach (var rpc in _rpcList)
                {
                    if (eff.ObserverAgent == rpc.CharacterName || eff.ObserverAgent == (Name) "*")
                    {
                        rpc.Perceive(EventHelper.PropertyChange(eff.PropertyName, eff.NewValue, eff.ObserverAgent));
                        ;
                    }

                }
            }

        }

        // else there was no decision

    }





}

