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

public class TheOfficeDemo : MonoBehaviour {







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
    private RolePlayCharacterAsset _player;
    private int counter;
    private Name chosenTarget;
    private bool initiated;

    private WorldModel.WorldModelAsset _wm;

    
    public bool skipCharacterSelection;

    public bool skipScenarioSelection;


    // Use this for initialization
    private IEnumerator Start()
    {
        waitingforReply = false;
        Initialized = false;
        _finalScore = GameObject.FindGameObjectWithTag("FinalScore");
        _finalScore.SetActive(false);
        AssetManager.Instance.Bridge = new AssetManagerBridge();
        ;
      //  m_dialogController.AddDialogLine("Loading...");

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
        LoadScenarioMenu();
    }

    private void LoadScenarioMenu()
    {
        ClearButtons();

        if(skipScenarioSelection)
            ChooseCharacterMenu(m_scenarios.FirstOrDefault());
        else foreach (var s in m_scenarios)
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

        if(skipCharacterSelection){

              var rpc = RolePlayCharacterAsset.LoadFromFile(data.IAT.GetAllCharacterSources().FirstOrDefault().Source);
            _player = rpc;
             LoadScenario(data);
           
        }

        else
        foreach (var agent in data.IAT.GetAllCharacterSources())
        {
            var rpc = RolePlayCharacterAsset.LoadFromFile(agent.Source);
            AddButton(rpc.CharacterName.ToString(), () =>
            {
                _player = rpc;
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

         if(_iat.m_worldModelSource != null)
            if(_iat.m_worldModelSource.Source != "" && _iat.m_worldModelSource.Source != null)
        _wm = WorldModel.WorldModelAsset.LoadFromFile(_iat.GetWorldModelSource().Source);


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
            _agentControllers.Add(_agentController);
            

            if(rpc.CharacterName == _player.CharacterName) _player = rpc;

            CharacterCount++;

        }

        foreach(var agent in _agentControllers)
        {
            if(agent.RPC.CharacterName != _player.CharacterName)
            agent.StartBehaviour(this, VersionMenu);
        }


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
        SetCamera();
        PlayerDecide();
    }

    public void SetCamera()
    {
       var camera =  GameObject.FindGameObjectWithTag("MainCamera");

      var rpc = GameObject.FindGameObjectWithTag(_player.CharacterName.ToString());

        camera.transform.position = rpc.transform.position;
        camera.transform.rotation = rpc.transform.rotation;

        camera.GetComponent<Camera>().fieldOfView = 40;

        camera.transform.Translate(new Vector3(-0.02f, 1.375f, 0));

         camera.transform.Rotate(new Vector3(10.0f,0.0f, 0));
   
  //      var MouseLook = camera.GetComponent<MouseLookController>();

    //    MouseLook.target = GameObject.FindGameObjectWithTag(rpcList.Find(x=>x.CharacterName != _player.CharacterName).CharacterName.ToString()).transform.position;
//        MouseLook.Online(true);




    }
    public void SaveState()
    {
        _agentController.SaveOutput();
    }

    private void UpdateButtonTexts(bool hide, List<IAction> decisions)
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
            var maxUtility = decisions.Max(x=>x.Utility);
            var playerDecisions = decisions.Where(x=>x.Utility == maxUtility);
            int i = 0;
            foreach(var a in playerDecisions){
           
           
              var dialogueOptions =   _iat.GetDialogueActions(a.Parameters.ElementAt(0), a.Parameters.ElementAt(1), a.Parameters.ElementAt(2), a.Parameters.ElementAt(3));
            
             
            foreach (var d in dialogueOptions)
            {
                if (isInButtonList(d.Utterance)) continue;
                var b = Instantiate(m_dialogButtonArchetype);
                var t = b.transform;
                i += 1;
                t.SetParent(m_dialogButtonZone, false);
            
                b.GetComponentInChildren<Text>().text = i + ": " + "[Towards " + a.Target + "] - " + d.Utterance;
                var id = d.Id;
                b.onClick.AddListener((() => Reply(id, a.Target)));
                m_buttonList.Add(b);

            }

            }

        }
    }


  

   public void Reply(Guid dialogId, Name target)
    {

        var reply = _iat.GetDialogActionById(dialogId);
        var actionFormat = string.Format("Speak({0},{1},{2},{3})", reply.CurrentState, reply.NextState, reply.Meaning, reply.Style);


        StartCoroutine(PlayerReplyAction(actionFormat, target));

        alreadyUsedDialogs.Add(reply.Utterance, reply.UtteranceId);


          foreach (var b in m_buttonList)
            {
                Destroy(b.gameObject);
            }
            m_buttonList.Clear(); 
    }

    private IEnumerator PlayerReplyAction(string replyActionName, Name target)
    {
       
       HandleEffects(new List<Name>{ EventHelper.ActionEnd(IATConsts.PLAYER, replyActionName, target.ToString())});

          yield return new WaitForSeconds(0.5f);
    }

    // Update is called once per frame
   void FixedUpdate()
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
            //   Timeleft -= Time.deltaTime;

            //    if (Timeleft < 0)
            //       RandomizeNext();
        }

        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!m_buttonList.IsEmpty())
            {
                if (m_buttonList.ElementAt(0))
                    m_buttonList.First().onClick.Invoke();
            }
        }
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (!m_buttonList.IsEmpty())
            {
                if (m_buttonList.ElementAt(1))
                    m_buttonList.ElementAt(1).onClick.Invoke();
            }
        }
        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (!m_buttonList.IsEmpty())
            {
                if (m_buttonList.ElementAt(2))
                    m_buttonList.ElementAt(2).onClick.Invoke();
            }
        }
        if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (!m_buttonList.IsEmpty())
            {
                if (m_buttonList.ElementAt(3))
                    m_buttonList.ElementAt(3).onClick.Invoke();
            }
        }
        if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (!m_buttonList.IsEmpty())
            {
                if (m_buttonList.ElementAt(4))
                    m_buttonList.ElementAt(4).onClick.Invoke();
            }
        }

        if (rpcList != null && _agentControllers != null)
        {
            
            foreach (var agent in _agentControllers)
                if (agent.getJustReplied())
                {
                   // Debug.Log("we got a reply!");
                     var reply = agent.getReply();
                    var lastAction = agent.getLastAction();
            
                     HandleEffects(new List<Name>{ EventHelper.ActionEnd(agent.RPC.CharacterName, (Name)("Speak(" + reply.CurrentState.ToString() + "," + reply.NextState.ToString() + "," + reply.Meaning.ToString() + "," + reply.Style.ToString() + ")"), lastAction.Target)});


            // will probably need to launch a courotine
                     waitingforReply = false;

                      PlayerDecide();

                    break;
                }
        }

      
    }
    

    public void Restart()
    {
        SceneManager.LoadScene(1);
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


    public void PlayerDecide()
    {

       
         if (_player != null)
        {
          
            var decisions = _player.Decide();

            if(decisions.IsEmpty() || waitingforReply)
             return;

              UpdateButtonTexts(false, decisions.ToList());

        }
    }


     public void HandleEffects(List<Name> _events)
    {
        foreach(var ev in _events){
         _player.Perceive(ev);
         _agentControllers.ForEach(x=>x.RPC.Perceive(ev));
        }

        if(_wm != null){
            var effects = _wm.Simulate(_events.ToArray());

        
        foreach(var eff in effects)
        {
            Debug.Log("Effect: " + eff.PropertyName + " " + eff.NewValue + " " + eff.ObserverAgent);
         
            if(eff.ObserverAgent.ToString() == _player.CharacterName.ToString())
            {
                _player.Perceive(EventHelper.PropertyChange(eff.PropertyName, eff.NewValue, (Name)"World"));

            }
            else if(eff.ObserverAgent.IsUniversal)
            {
                _player.Perceive(EventHelper.PropertyChange(eff.PropertyName, eff.NewValue, (Name)"World"));
                _agentControllers.ForEach(x=>x.RPC.Perceive(EventHelper.PropertyChange(eff.PropertyName, eff.NewValue, (Name)"World")));
            }
             else {
            {
                 _agentControllers.Find(x=>x.RPC.CharacterName == eff.ObserverAgent).RPC.Perceive(EventHelper.PropertyChange(eff.PropertyName, eff.NewValue, (Name)"World"));
            }

            }
    }
        }
    }

}