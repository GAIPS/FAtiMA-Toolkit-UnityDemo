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

            _iat = IntegratedAuthoringToolAsset.LoadFromFile(ScenarioPath);
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

    // Use this for initialization
    private IEnumerator Start()
    {
        _finalScore = GameObject.FindGameObjectWithTag("FinalScore");
        _finalScore.SetActive(false);
        AssetManager.Instance.Bridge = new AssetManagerBridge();
        ;
        m_dialogController.AddDialogLine("Loading...");

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
            m_dialogController.AddDialogLine("Error: Scenario entries must in groups of 2, to identify the scenario file, and TTS directory");
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
    }

    private void LoadScenario(ScenarioData data)
    {
        ClearButtons();

        _iat = data.IAT;

        _introPanel.SetActive(true);
        _introText.text = string.Format("<b>{0}</b>\n\n\n{1}", _iat.ScenarioName, _iat.ScenarioDescription);

        var characterSources = _iat.GetAllCharacterSources().ToList();
        foreach (var source in characterSources)
        {
            var rpc = RolePlayCharacterAsset.LoadFromFile(source.Source);
            rpc.LoadAssociatedAssets();
            _iat.BindToRegistry(rpc.DynamicPropertiesRegistry);
            AddButton(characterSources.Count == 1 ? "Start" : rpc.CharacterName.ToString(), 
                () =>
                {
                   var body = m_bodies.FirstOrDefault(b => b.BodyName == rpc.BodyName);
                   _agentController = new AgentControler(data, rpc, _iat, body.CharaterArchtype, m_characterAnchor, m_dialogController);
                StopAllCoroutines();
                _agentController.storeFinalScore(_finalScore);
                _agentController.Start(this, VersionMenu);
                InstantiateScore();
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
            if (m_buttonList.Count == dialogOptions.Count())
                return;

            foreach (var d in dialogOptions)
            {
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
        var reply = _iat.GetDialogActionById(IATConsts.PLAYER, dialogId);
        var actionFormat = string.Format("Speak({0},{1},{2},{3})", reply.CurrentState, reply.NextState, reply.GetMeaningName(), reply.GetStylesName());


        StartCoroutine(PlayerReplyAction(actionFormat, reply.NextState));
        UpdateScore(reply);

    }

    private IEnumerator PlayerReplyAction(string replyActionName, string nextState)
    {
        const float WAIT_TIME = 0.5f;
        _agentController.AddEvent(EventHelper.ActionStart(IATConsts.PLAYER,replyActionName,_agentController.RPC.CharacterName.ToString()).ToString());
        yield return new WaitForSeconds(WAIT_TIME);
        _agentController.AddEvent(EventHelper.ActionEnd(IATConsts.PLAYER, replyActionName, _agentController.RPC.CharacterName.ToString()).ToString());
        _agentController.AddEvent(EventHelper.PropertyChanged(string.Format(IATConsts.DIALOGUE_STATE_PROPERTY, IATConsts.PLAYER), nextState, "SELF").ToString()); 
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

            UpdateScore(_agentController.getReply());
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Time.timeScale > 0)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            this.SaveState();
        }

        _agentController.UpdateEmotionExpression();

        var state = (Name)_agentController.RPC.GetBeliefValue(string.Format(IATConsts.DIALOGUE_STATE_PROPERTY, IATConsts.PLAYER));
        var possibleOptions = _iat.GetDialogueActionsByState(IATConsts.PLAYER, state.ToString());
        if (!possibleOptions.Any())
        {
            UpdateButtonTexts(true, null);
        }
        else
        {
            UpdateButtonTexts(false, possibleOptions);
        }
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

    public void End()
    {

        _agentController.End();
    }

}