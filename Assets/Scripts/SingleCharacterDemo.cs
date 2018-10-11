using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetManagerPackage;
using Assets.Scripts;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WellFormedNames;
using RolePlayCharacter;
using UnityEngine.SceneManagement;
using Utilities;

public class SingleCharacterDemo : MonoBehaviour
{
    public struct ScenarioData
    {
        public readonly string ScenarioPath;
        public readonly string TTSFolder;

        private IntegratedAuthoringToolAsset _iat;

        public IntegratedAuthoringToolAsset IAT { get { return _iat; } }

        public ScenarioData(string path, string tts)
        {
            ScenarioPath = path;
            TTSFolder = tts;
            var auxPath = "";
            if (Application.platform.ToString().Contains("OS"))
                auxPath = path.Replace('\\', '/');
            else auxPath = path;
            _iat = IntegratedAuthoringToolAsset.LoadFromFile(auxPath);
        }
    }


    [Serializable]
    private struct BodyType
    {
        public string BodyName;
        public UnityBodyImplement CharaterArchtype;
    }

    [SerializeField]
    private Transform m_characterAnchor;

    [SerializeField]
    private DialogController m_dialogController;

    [SerializeField]
    private BodyType[] m_bodies;

    [Space]
    [SerializeField]
    private Button m_dialogButtonArchetype = null;
    [SerializeField]
    private Transform m_dialogButtonZone = null;

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

    [Header("Intro")]
    [SerializeField]
    private GameObject _introPanel;
    [SerializeField]
    private Text _introText;

    private ScenarioData[] m_scenarios;
    private List<Button> m_currentMenuButtons = new List<Button>();
    private List<Button> m_buttonList = new List<Button>();
    private IntegratedAuthoringToolAsset _iat;
    private AgentControler _agentController;


    public Dictionary<string, string> alreadyUsedDialogs;


    private bool Initialized;
    private bool waitingforReply;
    private RolePlayCharacterAsset Player;
    private WorldModel.WorldModelAsset _wm;


    public GameObject _background;
    public Material activeBackgroundMaterial;

    // Use this for initialization
    private IEnumerator Start()
    {
        waitingforReply = false;
        Initialized = false;
        AssetManager.Instance.Bridge = new AssetManagerBridge();

        m_dialogController.AddDialogLine("Loading...");

        alreadyUsedDialogs = new Dictionary<string, string>();

        var streamingAssetsPath = Application.streamingAssetsPath;

#if UNITY_EDITOR || UNITY_STANDALONE
        streamingAssetsPath = "file://" + streamingAssetsPath;
#endif

        var www = new WWW(streamingAssetsPath + "/scenarioList.txt");
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            m_dialogController.AddDialogLine("Error: " + www.error);
            yield break;
        }

        var entries = www.text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
        if ((entries.Length % 2) != 0)
        {
            m_dialogController.AddDialogLine("Error: Scenario entries must in groups of 2 to identify the scenario file, and TTS directory");
            yield break;
        }

        {
            List<ScenarioData> data = new List<ScenarioData>();

            for (int i = 0; i < entries.Length; i += 2)
            {
                var path = entries[i].Trim();
                var tts = entries[i + 1].Trim();
                data.Add(new ScenarioData(path, tts));
            }

            m_scenarios = data.ToArray();
        }

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

        foreach (var m in m_buttonList)
            Destroy(m.gameObject);

        m_buttonList = new List<Button>();
    }

    private void LoadScenario(ScenarioData data)
    {
        ClearButtons();

        _iat = data.IAT;

        _introPanel.SetActive(true);
        _introText.text = string.Format("<b>{0}</b>\n\n\n{1}", _iat.ScenarioName, _iat.ScenarioDescription);



        if(_iat.m_worldModelSource != null)
            if(!string.IsNullOrEmpty(_iat.m_worldModelSource.Source))
        _wm = WorldModel.WorldModelAsset.LoadFromFile(_iat.GetWorldModelSource().Source);

        var characterSources = _iat.GetAllCharacterSources().ToList();
        foreach (var source in characterSources)
        {
           
            var rpc = RolePlayCharacterAsset.LoadFromFile(source.Source);
            rpc.LoadAssociatedAssets();
            _iat.BindToRegistry(rpc.DynamicPropertiesRegistry);

            if (rpc.CharacterName.ToString().Contains("Player"))
            {
                Player = rpc;
                continue;
            }
            AddButton(characterSources.Count == 1 ? "Start" : rpc.CharacterName.ToString(),
                () =>
                {
                  
                    var body = m_bodies.FirstOrDefault(b => b.BodyName == rpc.BodyName);
                    _agentController = new AgentControler(data, rpc, _iat, body.CharaterArchtype, m_characterAnchor, m_dialogController);
                    StopAllCoroutines();
                    _agentController.Start(this, VersionMenu);

                     _background.SetActive(true);
                     if(activeBackgroundMaterial != null)
                     _background.GetComponent<Renderer>().material = activeBackgroundMaterial;

                });
        }
        AddButton("Back to Scenario Selection Menu", () =>
        {
            _iat = null;
            LoadScenarioMenu();
        });


       
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

    public void Reply(Guid dialogId)
    {
        var state = _agentController.RPC.GetBeliefValue("DialogState(Player)");
        if (state == IATConsts.TERMINAL_DIALOGUE_STATE)
        {
            return;
        }
        var reply = _iat.GetDialogActionById(dialogId);
        var actionFormat = string.Format("Speak({0},{1},{2},{3})", reply.CurrentState, reply.NextState, reply.Meaning, reply.Style);


        StartCoroutine(PlayerReplyAction(actionFormat, reply.NextState));

        alreadyUsedDialogs.Add(reply.Utterance, reply.UtteranceId);


          foreach (var b in m_buttonList)
            {
                Destroy(b.gameObject);
            }
            m_buttonList.Clear(); 
    }

    private IEnumerator PlayerReplyAction(string replyActionName, string nextState)
    {
        ClearButtons();
        const float WAIT_TIME = 0.1f;
        _agentController.RPC.Perceive(EventHelper.ActionStart(IATConsts.PLAYER, replyActionName, _agentController.RPC.CharacterName.ToString()));
        yield return new WaitForSeconds(WAIT_TIME);
        _agentController.RPC.Perceive(EventHelper.ActionEnd(IATConsts.PLAYER, replyActionName, _agentController.RPC.CharacterName.ToString()));

       HandleEffects(new List<Name>{ EventHelper.ActionEnd(IATConsts.PLAYER, replyActionName, _agentController.RPC.CharacterName.ToString())});

    }

    // Update is called once per frame
    void Update()
    {
        if (_agentController == null)
            return;

        if (!_agentController.IsRunning)
            return;

        
        if (  _agentController._body._speechController.IsPlaying)
            return;

        if (_agentController.getJustReplied())
        {
           
            var reply = _agentController.getReply();

            
       HandleEffects(new List<Name>{ EventHelper.ActionEnd(_agentController.RPC.CharacterName, (Name)("Speak(" + reply.CurrentState.ToString() + "," + reply.NextState.ToString() + "," + reply.Meaning.ToString() + "," + reply.Style.ToString() + ")"), (Name)"Player")});


          
          waitingforReply = false;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale > 0)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            this.Restart();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            this.SaveState();
        }

        _agentController.UpdateEmotionExpression();

         if (Player != null)
        {
           
            var decision = Player.Decide().FirstOrDefault();

            if(decision == null || waitingforReply)
             return;
            
            if (decision.Target != _agentController.RPC.CharacterName)
                return;
            var dialogActions = _iat.GetDialogueActions(decision.Parameters.ElementAt(0), Name.BuildName("*"), Name.BuildName("*"), Name.BuildName("*"));

            UpdateButtonTexts(false, dialogActions);

            waitingforReply = true;

        }
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


    public void HandleEffects(List<Name> _events)
    {
        Player.Perceive(_events);
        _agentController.RPC.Perceive(_events);
        _agentController.UpdateEmotionExpression();

        if(_wm != null){
            var effects = _wm.Simulate(_events.ToArray());

        foreach(var eff in effects)
        {
       //     Debug.Log("Effect: " + eff.PropertyName + " " + eff.NewValue + " " + eff.ObserverAgent);
            if(eff.ObserverAgent.ToString() == "Player")
            {
                Player.Perceive(EventHelper.PropertyChange(eff.PropertyName, eff.NewValue, (Name)"World"));
            } else if(eff.ObserverAgent.ToString() == _agentController.RPC.CharacterName.ToString())
            {
                 _agentController.RPC.Perceive(EventHelper.PropertyChange(eff.PropertyName, eff.NewValue, (Name)"World"));
                
            }

            else if(eff.ObserverAgent.IsUniversal)
            {
                Player.Perceive(EventHelper.PropertyChange(eff.PropertyName, eff.NewValue, (Name)"World"));
                _agentController.RPC.Perceive(EventHelper.PropertyChange(eff.PropertyName, eff.NewValue, (Name)"World"));
            }

         
            }
    }
    }
}
