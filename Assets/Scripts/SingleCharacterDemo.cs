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

    private ScenarioData[] m_scenarios;
    private List<Button> m_currentMenuButtons = new List<Button>();
    private List<Button> m_buttonList = new List<Button>();
    private IntegratedAuthoringToolAsset _iat;
    private AgentControler _agentController;
    private GameObject _finalScore;
    public Dictionary<string, string> alreadyUsedDialogs;
    private bool Initialized;
    private bool waitingforReply;
    private RolePlayCharacterAsset Player;

    // Use this for initialization
    private IEnumerator Start()
    {
        waitingforReply = false;
        Initialized = false;
        _finalScore = GameObject.FindGameObjectWithTag("FinalScore");
        _finalScore.SetActive(false);
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
                //   Debug.Log(path  + " e " + tts);
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

        if (_iat.ScenarioName.Contains("PJ"))
        {
            PJScenario = true;
        }
        else
        {
            PJScenario = false;
        }

        if (_iat.ScenarioName.Contains("Space"))
        {
            SpaceModulesScenario = true;
        }
        else
        {
            SpaceModulesScenario = false;
        }

        var characterSources = _iat.GetAllCharacterSources().ToList();
        foreach (var source in characterSources)
        {
            var rpc = RolePlayCharacterAsset.LoadFromFile(source.Source);
            rpc.LoadAssociatedAssets();
            _iat.BindToRegistry(rpc.DynamicPropertiesRegistry);

            if (rpc.CharacterName.ToString().Contains("Player"))
            {
                Debug.Log("we have a player!");
                Player = rpc;
                return;
            }
            AddButton(characterSources.Count == 1 ? "Start" : rpc.CharacterName.ToString(),
                () =>
                {
                    //  Debug.Log("Body " + rpc.BodyName);
                    var body = m_bodies.FirstOrDefault(b => b.BodyName == rpc.BodyName);
                    _agentController = new AgentControler(data, rpc, _iat, body.CharaterArchtype, m_characterAnchor, m_dialogController);
                    StopAllCoroutines();
                    _agentController.storeFinalScore(_finalScore);
                    _agentController.Start(this, VersionMenu);
                    if (PJScenario || SpaceModulesScenario) InstantiateScore();

                    StartCoroutine(AddDialoguePlayerOptions());
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
            var dif = false;
            if (m_buttonList.Count() == dialogOptions.Count())
                foreach (var b in m_buttonList)
                {
                    foreach (var d in dialogOptions)
                    {
                        if (b.GetComponentInChildren<Text>().text != d.Utterance)
                        {
                            dif = true;
                            break;
                        }
                    }
                }
            else dif = true;

            if (!dif)
                return;


            foreach (var d in dialogOptions)
            {


                if (isInButtonList(d.Utterance)) continue;
                var b = Instantiate(m_dialogButtonArchetype);
                var t = b.transform;
                t.SetParent(m_dialogButtonZone, false);
                b.GetComponentInChildren<Text>().text = d.Utterance;
                var id = d.Id;
                //  Debug.Log("I want to put this here: " + d.Utterance);
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
        if (PJScenario || SpaceModulesScenario) UpdateScore(reply);

        alreadyUsedDialogs.Add(reply.Utterance, reply.UtteranceId);


    }

    private IEnumerator PlayerReplyAction(string replyActionName, string nextState)
    {
        ClearButtons();
        const float WAIT_TIME = 0.1f;
        _agentController.AddEvent(EventHelper.ActionStart(IATConsts.PLAYER, replyActionName, _agentController.RPC.CharacterName.ToString()).ToString());
        yield return new WaitForSeconds(WAIT_TIME);
        _agentController.AddEvent(EventHelper.ActionEnd(IATConsts.PLAYER, replyActionName, _agentController.RPC.CharacterName.ToString()).ToString());
        _agentController.AddEvent(EventHelper.PropertyChange(string.Format(IATConsts.DIALOGUE_STATE_PROPERTY, IATConsts.PLAYER), nextState, "Player").ToString());
        _agentController.AddEvent(EventHelper.PropertyChange("HasFloor(" + _agentController.RPC.CharacterName + ")", "True", _agentController.RPC.CharacterName.ToString()).ToString());

    }

    // Update is called once per frame
    void Update()
    {
        if (_agentController == null)
            return;

        if (!_agentController.IsRunning)
            return;

        if (_agentController.getJustReplied())
        {

            var reply = _agentController.getReply();
            UpdateScore(reply);

            if (_iat.ScenarioName.ToString().Contains("Intro"))
                GiveFloor();
            // will probably need to launch a courotine
            if (Initialized) waitingforReply = false;
            if (_agentController.RPC.GetBeliefValue("HasFloor(SELF)", "SELF") != "True")
            {
                Debug.Log("Starting coroutine");
                StartCoroutine(AddDialoguePlayerOptions());
            }
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



    }

    private void LateUpdate()
    {
        if (_agentController != null)
            _agentController.UpdateFields();
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

            HandleKeywords(meaning.ToString());
        }

        foreach (var style in reply.Style)
        {

            HandleKeywords(style.ToString());
        }
    }

    /* private IEnumerable<DialogueStateActionDTO> HandleContext(string s)
     {
         IEnumerable<DialogueStateActionDTO> ret =

     }*/


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


    private IEnumerator AddDialogueOptions()
    {

        yield return new WaitForSeconds(0.6f);
        var state = (Name)_agentController.RPC.GetBeliefValue(string.Format(IATConsts.DIALOGUE_STATE_PROPERTY, IATConsts.PLAYER));
        //   Debug.Log("CurrentState: " + state.ToString());
        var possibleOptions = _iat.GetDialogueActionsByState(state.ToString());


        var originalPossibleActions = possibleOptions;

        if (!possibleOptions.Any())
        {
            UpdateButtonTexts(true, null);
            Initialized = true;


        }
        else
        {

            if (_iat.ScenarioName.Contains("Intro"))
            {
                GiveFloor();
            }

            else if (PJScenario)
            {
                if (waitingforReply) yield break;
                if (!Initialized)
                {

                    var newOptions =
                        possibleOptions.Where(x => x.CurrentState == IATConsts.INITIAL_DIALOGUE_STATE)
                            .Take(3)
                            .Shuffle()
                            .ToList();

                    newOptions.AddRange(_iat.GetDialogueActionsByState("Introduction"));
                    possibleOptions = newOptions;
                    waitingforReply = true;
                    UpdateButtonTexts(false, possibleOptions);
                }
                else
                {


                    var newOptions =
                        possibleOptions.Where(x => !alreadyUsedDialogs.ContainsKey(x.Utterance))
                            .Shuffle()
                            .Take(3)
                            .ToList();
                    //if(newOptions.Count > 2)    Debug.Log("NEW OPTIOns: " + newOptions.ElementAt(0).Utterance + newOptions.ElementAt(1).Utterance + newOptions.ElementAt(2).Utterance);
                    var additionalOptions = _iat.GetDialogueActionsByState("Start")
                        .Where(x => !alreadyUsedDialogs.ContainsKey(x.Utterance) && !newOptions.Contains(x))
                        .Shuffle()
                        .Take(2);


                    possibleOptions = newOptions.Concat(additionalOptions).Shuffle().ToList();

                    if (alreadyUsedDialogs.Count() > 12 && possibleOptions.Count() < 6)
                    {
                        var ClosureOptions =
                            _iat.GetDialogueActionsByState("Closure").Take(1).ToList();

                        possibleOptions = newOptions.Concat(additionalOptions).Concat(ClosureOptions).Shuffle().ToList();
                    }

                    waitingforReply = true;
                    UpdateButtonTexts(false, possibleOptions);
                }
            }


            else UpdateButtonTexts(false, possibleOptions);

        }
    }


    private IEnumerator AddDialoguePlayerOptions()
    {
        if (Player != null)
        {
            yield return new WaitForSeconds(0.6f);
            //   Debug.Log("CurrentState: " + state.ToString());
            var decision = Player.Decide().FirstOrDefault();
            foreach(var d in Player.Decide())
                Debug.Log(" Decision: " + decision.Name);


            /*     Debug.Log(" Decision" + decision.Name);
                 Debug.Log(" Uhm 1" + decision.Parameters.ElementAt(1).ToString() + " 2 " + decision.Parameters.ElementAt(2));
                 Debug.Log(" 3 " + decision.Parameters.ElementAt(3) + " 4 " + decision.Parameters.ElementAt(4));*/

            var dialogActions = _iat.GetDialogueActions(decision.Parameters.ElementAt(0), decision.Parameters.ElementAt(1), Name.BuildName("*"), Name.BuildName("*"));

            var generalOptions = _iat.GetDialogueActionsByState("*");
            List<DialogueStateActionDTO> possibleOptions = new List<DialogueStateActionDTO>(dialogActions);

            var originalPossibleActions = possibleOptions;

            if (!possibleOptions.Any())
            {
                UpdateButtonTexts(true, null);
                Initialized = true;


            }
            else
            {

            
              if (PJScenario)
                {
                    if (waitingforReply) yield break;
                    if (!Initialized)
                    {

                        var newOptions =
                            possibleOptions.Where(x => x.CurrentState == IATConsts.INITIAL_DIALOGUE_STATE)
                                .Take(3)
                                .Shuffle()
                                .ToList();

                        newOptions.AddRange(_iat.GetDialogueActionsByState("Introduction"));
                        possibleOptions = newOptions;
                        waitingforReply = true;
                        UpdateButtonTexts(false, possibleOptions);
                    }
                    else
                    {


                        var newOptions =
                            possibleOptions.Where(x => !alreadyUsedDialogs.ContainsKey(x.Utterance))
                                .Shuffle()
                                .Take(3)
                                .ToList();
                        //if(newOptions.Count > 2)    Debug.Log("NEW OPTIOns: " + newOptions.ElementAt(0).Utterance + newOptions.ElementAt(1).Utterance + newOptions.ElementAt(2).Utterance);
                        var additionalOptions = _iat.GetDialogueActionsByState("Start")
                            .Where(x => !alreadyUsedDialogs.ContainsKey(x.Utterance) && !newOptions.Contains(x))
                            .Shuffle()
                            .Take(2);


                        possibleOptions = newOptions.Concat(additionalOptions).Shuffle().ToList();

                        if (alreadyUsedDialogs.Count() > 12 && possibleOptions.Count() < 6)
                        {
                            var ClosureOptions =
                                _iat.GetDialogueActionsByState("Closure").Take(1).ToList();

                            possibleOptions = newOptions.Concat(additionalOptions).Concat(ClosureOptions).Shuffle().ToList();
                        }

                        waitingforReply = true;
                        UpdateButtonTexts(false, possibleOptions);
                    }
                }


                else UpdateButtonTexts(false, possibleOptions);

            }
        }
        else AddDialogueOptions();
    }



    public void GiveFloor()
    {
        if(Player != null)
          Player.Perceive(EventHelper.PropertyChange("HasFloor(Player)", "False","Player"));

        _agentController.AddEvent(EventHelper.PropertyChange("HasFloor(" + _agentController.RPC.CharacterName + ")", "True", _agentController.RPC.CharacterName.ToString()).ToString());

    }

}
