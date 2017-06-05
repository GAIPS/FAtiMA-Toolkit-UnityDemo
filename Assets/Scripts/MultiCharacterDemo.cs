using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using ActionLibrary;
using AssetManagerPackage;
using Assets.Scripts;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;
using RolePlayCharacter;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;
using WellFormedNames;

public class MultiCharacterDemo : MonoBehaviour {

  

    [Serializable]
    private struct BodyType
    {
        public string BodyName;
        public UnityBodyImplement CharaterArchtype;
    }

    [SerializeField]
    private List<Transform> m_characterAnchors;

    [SerializeField]
    private DialogController m_dialogController;

    [SerializeField]
    private BodyType[] m_bodies;





    [Space]
    [SerializeField]
    private Button m_dialogButtonArchetype = null;
    [SerializeField]
    private Transform m_dialogButtonZone = null;


    [SerializeField]
    private Transform m_scoreZone = null;

    private GameObject score;

    [Space]
    [SerializeField]
    [Range(1, 60)]
    private float m_agentProblemReminderRepeatTime = 3;

    [Space]
    [SerializeField]
    private RectTransform m_menuButtonHolder = null;
    [SerializeField]
    private Button m_menuButtonArchetype = null;

    public GameObject VersionMenu;
    public GameObject ScoreTextPrefab;
    private bool PJScenario;
    private bool SpaceModulesScenario;

    [Header("Intro")]
    [SerializeField]
    private GameObject _introPanel;
    [SerializeField]
    private Text _introText;

    private SingleCharacterDemo.ScenarioData[] m_scenarios;
    private List<Button> m_currentMenuButtons = new List<Button>();
    private List<Button> m_buttonList = new List<Button>();
    private IntegratedAuthoringToolAsset _iat;
    private MultiCharacterAgentController _agentController;
    private List<MultiCharacterAgentController> _agentControllers;
    private GameObject _finalScore;
    public Dictionary<string, string> alreadyUsedDialogs;
    private bool Initialized;
    private bool waitingforReply;
    private List<RolePlayCharacterAsset> rpcList;

    private List<Name> _events;
    private List<IAction> _actions;
    string currentSocialMoveAction;
    string currentSocialMoveResult;
    private MultiCharacterAgentController justSpokeAgent;
    private float TIME_LEFT_CONST = 5.0f;
    private float Timeleft = 15.0f;
    private bool stopTime = false;
    private Name _chosenCharacter;
    private int counter;
    private Name chosenTarget;
    private bool initiated;


    // Use this for initialization
    private IEnumerator Start()
    {
        waitingforReply = false;
        Initialized = false;
        _finalScore = GameObject.FindGameObjectWithTag("FinalScore");
        _finalScore.SetActive(false);
        AssetManager.Instance.Bridge = new AssetManagerBridge();
        ;
        m_dialogController.AddDialogLine("Loading...");

        alreadyUsedDialogs = new Dictionary<string, string>();

        var streamingAssetsPath = Application.streamingAssetsPath;
        counter = 0;
#if UNITY_EDITOR || UNITY_STANDALONE
        streamingAssetsPath = "file://" + streamingAssetsPath;
#endif
        var www = new WWW(streamingAssetsPath + "/MultiCharacterScenarioList.txt");
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            m_dialogController.AddDialogLine("Error: " + www.error);
            yield break;
        }
        rpcList = new List<RolePlayCharacterAsset>();
        m_characterAnchors = new List<Transform>();
        foreach (var anc in GameObject.FindGameObjectsWithTag("Anchor"))
        {
            m_characterAnchors.Add(anc.transform);
        }
       
        _agentControllers = new List<MultiCharacterAgentController>();
        justSpokeAgent = null;

        var entries = www.text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
        if ((entries.Length % 2) != 0)
        {
            m_dialogController.AddDialogLine("Error: Scenario entries must in groups of 2, to identify the scenario file, and TTS directory");
            yield break;
        }

        {
            List<SingleCharacterDemo.ScenarioData> data = new List<SingleCharacterDemo.ScenarioData>();

            for (int i = 0; i < entries.Length; i += 2)
            {
                var path = entries[i].Trim();
                var tts = entries[i + 1].Trim();
                //    Debug.Log(path  + " e " + tts);
                data.Add(new SingleCharacterDemo.ScenarioData(path, tts));
            }

            m_scenarios = data.ToArray();
        }

        _events = new List<Name>();
       _actions = new List<IAction>();
       currentSocialMoveAction = "";
        initiated = false;
       currentSocialMoveResult = "";
        chosenTarget = Name.BuildName("ay");
        m_dialogController.Clear();
        LoadScenarioMenu();
    }

    private void LoadScenarioMenu()
    {
        ClearButtons();
        foreach (var s in m_scenarios)
        {
            var data = s;
            AddButton(s.IAT.ScenarioName, () =>
            {
                ChooseCharacterMenu(data);

            });
        }
    }

    private void ChooseCharacterMenu(SingleCharacterDemo.ScenarioData data)
    {
        ClearButtons();
        foreach (var agent in data.IAT.GetAllCharacterSources())
        {
            var rpc = RolePlayCharacterAsset.LoadFromFile(agent.Source);
            AddButton(rpc.CharacterName.ToString(), () =>
            {
                _chosenCharacter = rpc.CharacterName;
                LoadScenario(data);
            });
        }
    }

    private void AddButton(string label, UnityAction action)
    {
        var button = Instantiate(m_menuButtonArchetype);
        var t = button.transform;
        t.SetParent(m_menuButtonHolder);
        t.localScale = Vector3.one;
        button.image.color = new Color(0, 0, 0, 0);
        button.image.color = new Color(200, 200, 200, 0);

        var buttonLabel = button.GetComponentInChildren<Text>();
        buttonLabel.text = label;
        buttonLabel.color = Color.white;
        button.onClick.AddListener(action);
        m_currentMenuButtons.Add(button);
    }

    private void ClearButtons()
    {
        foreach (var b in m_currentMenuButtons)
        {
            Destroy(b.gameObject);
        }
        m_currentMenuButtons.Clear();
    }

    private void LoadScenario(SingleCharacterDemo.ScenarioData data)
    {
        ClearButtons();
      
        _iat = data.IAT;

        _introPanel.SetActive(true);
        _introText.text = string.Format("<b>{0}</b>\n\n\n{1}", _iat.ScenarioName, _iat.ScenarioDescription);

      
        var characterSources = _iat.GetAllCharacterSources().ToList();
        int CharacterCount = 0;
        foreach (var source in characterSources)
        {
            var rpc = RolePlayCharacterAsset.LoadFromFile(source.Source);
            rpc.LoadAssociatedAssets();
            _iat.BindToRegistry(rpc.DynamicPropertiesRegistry);
            rpcList.Add(rpc);
            var body = m_bodies.FirstOrDefault(b => b.BodyName == rpc.BodyName);
            _agentController = new MultiCharacterAgentController(data, rpc, _iat, body.CharaterArchtype, m_characterAnchors[CharacterCount], m_dialogController);
                    StopAllCoroutines();
               
            //        if (PJScenario || SpaceModulesScenario) InstantiateScore();
              _agentControllers.Add(_agentController);
    //       Debug.Log("Character count " + CharacterCount + " size " + characterSources.Count);
            CharacterCount++;

        }

       
        AddButton("Back to Scenario Selection Menu", () =>
        {
            _iat = null;
            LoadScenarioMenu();
        });

        foreach (var actor in rpcList)
        {

            foreach (var anotherActor in rpcList)
            {
                if (actor != anotherActor)
                {

                    var changed = new[] { EventHelper.ActionEnd(anotherActor.CharacterName.ToString(), "Enters", "Room") };
                    actor.Perceive(changed);
                }

            }

          
        }

     RandomizeNext();
    }

    public void SaveState()
    {
        _agentController.SaveOutput();
    }

    private void UpdateButtonTexts(bool hide, IEnumerable<DialogueStateActionDTO> dialogOptions)
    {
        if (hide)
        {
            if (!m_buttonList.Any())
                return;
            foreach (var b in m_buttonList)
            {
                Destroy(b.gameObject);
            }
            m_buttonList.Clear();
        }
        else
        {

            if (m_buttonList.Count == dialogOptions.Count())
                return;

            foreach (var d in dialogOptions)
            {
                if (isInButtonList(d.Utterance)) continue;
                    var b = Instantiate(m_dialogButtonArchetype);
                    var t = b.transform;
                    t.SetParent(m_dialogButtonZone, false);
                    b.GetComponentInChildren<Text>().text = d.Utterance;
                    var id = d.Id;
                    b.onClick.AddListener((() => Reply(id)));
                    m_buttonList.Add(b);
                
            }

        }
    }


    private void TargetOptionsButton(bool hide, List<RolePlayCharacterAsset> targetOptions)
    {
        if (hide)
        {
            if (!m_buttonList.Any())
                return;
            foreach (var b in m_buttonList)
            {
                Destroy(b.gameObject);
             
            }
            m_buttonList.Clear();
            return;
        }


        foreach (var d in targetOptions)
            {
                
                var b = Instantiate(m_dialogButtonArchetype);
                var t = b.transform;
                t.SetParent(m_dialogButtonZone, false);
                b.GetComponentInChildren<Text>().text = d.CharacterName.ToString();
               
                b.onClick.AddListener((() => ChosenTarget(d.CharacterName.ToString())));
                m_buttonList.Add(b);

            }

        }
    

    public void Reply(Guid dialogId)
    {
   /*     var state = _agentController.RPC.GetBeliefValue("DialogState(Player)");
        if (state == IATConsts.TERMINAL_DIALOGUE_STATE)
        {
            return;
        }*/
        var reply = _iat.GetDialogActionById(IATConsts.AGENT, dialogId);
     
        Name actionName = Name.BuildName("Speak");
        Name actionCS = Name.BuildName(reply.CurrentState);      
        Name actionNS = Name.BuildName(reply.NextState);
        Name actionMean = Name.BuildName(reply.Meaning[0]);
        Name actionStyle = Name.BuildName(reply.Style[0]);
        Name utteranceID = Name.BuildName(reply.UtteranceId);

        var act =  new ActionLibrary.Action(new List<Name>() {actionName, actionCS, actionNS, actionMean, actionStyle}, chosenTarget);
      
        _agentControllers.Find(x => x.RPC.CharacterName == _chosenCharacter).Speak(this, act);
        UpdateButtonTexts(true, new DialogueStateActionDTO[1]);

        //  alreadyUsedDialogs.Add(reply.Utterance, reply.UtteranceId);


    }

    public void ChosenTarget(string charId)
    {
        chosenTarget = Name.BuildName(charId);
    
        var temp = new List<RolePlayCharacterAsset>();
        TargetOptionsButton(true, temp);
        PlayerInitiateTurn();
        

    }

    private IEnumerator PlayerReplyAction(string replyActionName, string nextState)
    {
        const float WAIT_TIME = 0.5f;
        _agentController.AddEvent(EventHelper.ActionStart(IATConsts.PLAYER, replyActionName, _agentController.RPC.CharacterName.ToString()).ToString());
        yield return new WaitForSeconds(WAIT_TIME);
        _agentController.AddEvent(EventHelper.ActionEnd(IATConsts.PLAYER, replyActionName, _agentController.RPC.CharacterName.ToString()).ToString());
        _agentController.AddEvent(EventHelper.PropertyChange(string.Format(IATConsts.DIALOGUE_STATE_PROPERTY, IATConsts.PLAYER), nextState, "SELF").ToString());


    }

    // Update is called once per frame
    void Update()
    {

            if (Input.GetKeyDown(KeyCode.P))
            {
                Time.timeScale = Time.timeScale > 0 ? 0 : 1;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.Restart();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                this.SaveState();
            }
        if (!stopTime)
        {
            Timeleft -= Time.deltaTime;

            if (Timeleft < 0)
                RandomizeNext();
        }

    }



    private void LateUpdate()
    {
        if (_agentController != null)
            _agentController.UpdateFields();

        List<Name> eventsToPerceive = new List<Name>();

        if (rpcList != null && _agentControllers != null)
        {

            foreach (var agent in _agentControllers)
            {
                if (agent.getJustReplied())
                {
                    var lastEvent = agent.GetLastEvent().FirstOrDefault();
                    Debug.Log(" to perceive " + lastEvent.ToString());
                    eventsToPerceive.Add(lastEvent);
                    var actionTarget = agent.getLastAction().Target;
                    var TargetRPC = _agentControllers.Find(x => x.RPC.CharacterName == actionTarget);
                    agent.setFloor(false);
                    justSpokeAgent = agent;
                    Debug.Log("Justspoke agent" + justSpokeAgent.RPC.CharacterName + lastEvent);
                    Debug.Log(justSpokeAgent.RPC.CharacterName + " property Friendship towards "  + actionTarget + justSpokeAgent.RPC.m_kb.AskProperty(Name.BuildName("Friendship(" + justSpokeAgent.RPC.CharacterName + "," + TargetRPC.RPC.CharacterName + ")" )));
                    Debug.Log(justSpokeAgent.RPC.CharacterName + " property Attraction towards " + actionTarget + justSpokeAgent.RPC.m_kb.AskProperty(Name.BuildName("Attraction(" + justSpokeAgent.RPC.CharacterName + "," + TargetRPC.RPC.CharacterName + ")")));
                    Debug.Log(TargetRPC.RPC.CharacterName + " property Friendship towards " + justSpokeAgent.RPC.CharacterName + TargetRPC.RPC.m_kb.AskProperty(Name.BuildName("Friendship(" + TargetRPC.RPC.CharacterName + "," + justSpokeAgent.RPC.CharacterName + ")")));
                    Debug.Log(TargetRPC.RPC.CharacterName + " property Attraction towards " + justSpokeAgent.RPC.CharacterName + TargetRPC.RPC.m_kb.AskProperty(Name.BuildName("Attraction(" + TargetRPC.RPC.CharacterName + "," + justSpokeAgent.RPC.CharacterName + ")")));

                    agent.ClearTempVariables();
                    m_dialogController.Clear();
                  
                    foreach (var aux in _agentControllers)
                    {
                        aux.RPC.Perceive(eventsToPerceive);
                        aux.UpdateEmotionExpression();
                       
                    }
                
         
                    if (lastEvent.ToString().Contains("Finalize"))
                    {
                      //  Debug.Log(" last event " + lastEvent.ToString());
                        RandomizeNext();
                    }
                                       
                    else
                    {
                        var go = _agentControllers.Find(x => x.RPC.CharacterName == actionTarget);

                        if (go.RPC != null)
                        {
                            if (go.RPC.CharacterName == _chosenCharacter)
                            {
                                chosenTarget = agent.RPC.CharacterName;
                                PlayerReplyTurn();
                              
                            }
                               
                            else
                            {
                                Debug.Log(" Next NPC: " + go.RPC.CharacterName);
                                go.setFloor(true);
                                Timeleft = TIME_LEFT_CONST;

                                go.StartBehaviour(this, VersionMenu);
                            }

                       
                        }
                    }
                    return;
                }               
            }         
        }      
    }

    

    private void InstantiateScore()
    {

        score = Instantiate(ScoreTextPrefab);

        var t = score.transform;
        t.SetParent(m_scoreZone, false);

        if (PJScenario)
        {
            var obj = GameObject.FindGameObjectWithTag("Score");
            obj.GetComponent<ScoreManager>().SetPJ(true);
            obj.GetComponent<ScoreManager>().Refresh();

        }
        else if (SpaceModulesScenario)
        {
            var obj = GameObject.FindGameObjectWithTag("Score");
            obj.GetComponent<ScoreManager>().SetPJ(false);
            obj.GetComponent<ScoreManager>().Refresh();
        }
    }

    public void UpdateScore(DialogueStateActionDTO reply)
    {


        foreach (var meaning in reply.Meaning)
        {

            HandleKeywords(meaning);
        }

        foreach (var style in reply.Style)
        {

            HandleKeywords(style);
        }
    }



    private void HandleKeywords(string s)
    {

        char[] delimitedChars = { '(', ')' };

        string[] result = s.Split(delimitedChars);



        if (result.Length > 1)

            if (PJScenario)
            {
                switch (result[0])
                {
                    case "Aggression":
                        score.GetComponent<ScoreManager>().addAggression(Int32.Parse(result[1]));
                        break;

                    case "Information":
                        score.GetComponent<ScoreManager>().addInformation(Int32.Parse(result[1]));
                        break;

                    case "Truth":
                        score.GetComponent<ScoreManager>().addTruth(Int32.Parse(result[1]));
                        break;

                }
            }
            else
                switch (result[0])
                {
                    case "Inquire":
                        score.GetComponent<ScoreManager>().AddI(Int32.Parse(result[1]));
                        break;

                    case "FAQ":
                        score.GetComponent<ScoreManager>().AddF(Int32.Parse(result[1]));
                        break;

                    case "Closure":
                        score.GetComponent<ScoreManager>().AddC(Int32.Parse(result[1]));
                        break;

                    case "Empathy":
                        score.GetComponent<ScoreManager>().AddE(Int32.Parse(result[1]));
                        break;

                    case "Polite":
                        score.GetComponent<ScoreManager>().AddP(Int32.Parse(result[1]));
                        break;
                }

    }


    public void ClearScore()
    { 
        Destroy(score);
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }


    public bool isInButtonList(string utterance)
    {

        foreach (var button in m_buttonList)
        {
            if (button.GetComponentInChildren<Text>().text == utterance)
                return true;
        }
        return false;
    }


    public void PlayerInitiateTurn()
    {
        var playerAgent = rpcList.Find(x => x.CharacterName == _chosenCharacter);

  
     
       var actionList =  _iat.GetDialogueActionsBySpeaker(IATConsts.AGENT).ToList();
        var newList = actionList.FindAll(x => x.Meaning.First().ToString().Contains("Initiate"));
        
        CommeillFaut.CommeillFautAsset cif = CommeillFaut.CommeillFautAsset.LoadFromFile(rpcList.First().CommeillFautAssetSource);

        List<DialogueStateActionDTO> dialogs = new List<DialogueStateActionDTO>();

        foreach (var social in cif.m_SocialExchanges)
        {
            Debug.Log(social.ActionName.ToString() + " and " + newList.FirstOrDefault().Meaning.First());
        var member = newList.FindAll(x => x.Meaning.First().ToString().Contains(social.ActionName.ToString())).Shuffle().FirstOrDefault();
            dialogs.Add(member);
        }

     //   newList = newList.FindAll(x => x.Parameters[2].ToString().Contains("Initiate"));

     //   Debug.Log(" decided size " + newList.Count);
      //  IEnumerable<DialogueStateActionDTO> availableDialogs = new DialogueStateActionDTO[1];
      
        stopTime = true;
/*
        foreach (var speakAction in newList)
        {
            Name currentState = speakAction.CurrentState;
            Name nextState = speakAction.NextState;
            Name meaning = speakAction.Meanings[0];
     //       Debug.Log(" meaning: " + meaning);
            Name style = Name.BuildName("*");
            dialogs = _iat.GetDialogueActions(IATConsts.AGENT, currentState, nextState, meaning, style);
    //        Debug.Log(" dialog: " + dialogs.Count +  " first: " + dialogs[0].Utterance);
            availableDialogs = dialogs;

        }*/
        UpdateButtonTexts(false, dialogs);
    }


    public void PlayerReplyTurn()
    {
        var playerAgent = rpcList.Find(x => x.CharacterName == _chosenCharacter);


        var decidedList = playerAgent.Decide();
        var action = decidedList.FirstOrDefault();
      
        IEnumerable<DialogueStateActionDTO> availableDialogs = new DialogueStateActionDTO[1];
        List<DialogueStateActionDTO> dialogs = new List<DialogueStateActionDTO>();
        stopTime = true;

      
            Name currentState = action.Parameters[0];
            Name nextState = action.Parameters[1];
            Name meaning = action.Parameters[2];
            Debug.Log(" meaning: " + meaning);
            Name style = Name.BuildName("*");
            dialogs = _iat.GetDialogueActions(IATConsts.AGENT, currentState, nextState, meaning, style);
            //Debug.Log(" dialog: " + dialogs.Count +  " first: " + dialogs[0].Utterance);
            availableDialogs = dialogs;

        
        UpdateButtonTexts(false, availableDialogs);
    }


    public void ChoseTarget()
    {
        var notPlayerAgents = rpcList.FindAll(x => x.CharacterName != _chosenCharacter);

        TargetOptionsButton(false, notPlayerAgents);


    }

    public void RandomizeNext()
    {
    var rand = new System.Random();
        //   int next = rand.Next(0, _agentControllers.Count);

        if (initiated)
        {
            if (counter < 2)
                counter++;
            else counter = 0;
        }
        else {
            if (_agentControllers[0].RPC.CharacterName != _chosenCharacter)
                counter = 0;
            else counter = 1;
            initiated = true;
          
        }

          //  Debug.Log(" randomizeNext: " + counter + " count : " + _agentControllers.Count);
        Timeleft = TIME_LEFT_CONST;
        var go = _agentControllers[counter];

        if (go.RPC != null)
        {

            if (go.RPC.CharacterName == _chosenCharacter)
                ChoseTarget();
            else
            {
                Debug.Log("Time's up! Next NPC: " + go.RPC.CharacterName);
                go.setFloor(true);

                go.StartBehaviour(this, VersionMenu);
            }
            // go.RPC.SaveToFile("rpc-output.rpc");
        }


    }

  
}