using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ActionLibrary;
using AssetManagerPackage;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;
using RolePlayCharacter;
using UnityEngine;
using Utilities;
using WellFormedNames;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
	public class AgentControler
	{
		private RolePlayCharacterAsset _rpc;
		private DialogController m_dialogController;
		private IntegratedAuthoringToolAsset m_iat;
		private UnityBodyImplement _body;

		private List<string> _events = new List<string>();
		private string lastEmotionRPC;
		private float _previousMood;

		private float _moodThresold = 0.001f;
	    private GameObject _finalScore;
		private SingleCharacterDemo.ScenarioData m_scenarioData;
		private MonoBehaviour m_activeController;
		private GameObject m_versionMenu;
		private Coroutine _currentCoroutine = null;
	    private DialogueStateActionDTO reply;
	    private bool just_talked;
        

		public RolePlayCharacterAsset RPC { get { return _rpc; } }

		public AgentControler(SingleCharacterDemo.ScenarioData scenarioData, RolePlayCharacterAsset rpc,
			IntegratedAuthoringToolAsset iat, UnityBodyImplement archetype, Transform anchor, DialogController dialogCrt)
		{
			m_scenarioData = scenarioData;
			_rpc = rpc;
			m_iat = iat;
			m_dialogController = dialogCrt;
			_body = GameObject.Instantiate(archetype);
		    just_talked = false;
            var t = _body.transform;
			t.SetParent(anchor, false);
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;

			m_dialogController.SetCharacterLabel(rpc.Perspective.ToString());
		}

		public void AddEvent(string eventName)
		{
			_events.Add(eventName);
		}

		public void SetExpression(string emotion, float amount)
		{
			_body.SetExpression(emotion, amount);
		}

		//public void SaveOutput()
		//{
		//	const string datePattern = "dd-MM-yyyy-H-mm-ss";

		//	_rpc.SaveOutput(Application.streamingAssetsPath + "\\Output\\", _rpc.CharacterName + "-" + DateTime.Now.ToString(datePattern) + ".ea");
		//}

		public bool IsRunning
		{
			get { return _currentCoroutine != null; }
		}

		public void Start(MonoBehaviour controller, GameObject versionMenu)
		{
			m_activeController = controller;
			m_versionMenu = versionMenu;
			m_versionMenu.SetActive(false);
			_currentCoroutine = controller.StartCoroutine(UpdateCoroutine());
		}

		public void UpdateFields()
		{
			m_dialogController.UpdateFields(_rpc);
		}

		public void UpdateEmotionExpression()
		{
			var emotion = _rpc.GetStrongestActiveEmotion();
			if (emotion == null)
				return;

			_body.SetExpression(emotion.EmotionType, emotion.Intensity/10f);
		}

		private IEnumerator UpdateCoroutine()
		{
			_events.Clear();
			var enterEventRpcOne = string.Format("Event(Property-Change,{0},Front(Self),Computer)", _rpc.Perspective);
			AddEvent(enterEventRpcOne);
			AddEvent(string.Format("Event(Property-change,Self,DialogueState(Player),{0})", IntegratedAuthoringToolAsset.INITIAL_DIALOGUE_STATE));

			while (_rpc.GetBeliefValue("DialogueState(Player)") != "Disconnected")
			{
				yield return new WaitForSeconds(1);

				var actionRpc = _rpc.PerceptionActionLoop(_events);
				_events.Clear();
				_rpc.Update();

				if (actionRpc == null)
					continue;

				string actionKey = actionRpc.ActionName.ToString();
				Debug.Log("Action Key: " + actionKey);

				switch (actionKey)
				{
					case "Speak":
						m_activeController.StartCoroutine(HandleSpeak(actionRpc));
						break;
					case "Fix":
						m_activeController.StartCoroutine(HandleFix(actionRpc));
						break;
					case "Disconnect":
						m_activeController.StartCoroutine(HandleDisconnectAction(actionRpc));
						break;
					default:
						Debug.LogWarning("Unknown action: " + actionKey);
						break;
				}
			}

			m_dialogController.AddDialogLine(string.Format("- {0} disconnects -", _rpc.Perspective));
			_currentCoroutine = null;
			Object.Destroy(_body.Body);
		}

		private static string JoinStrings(string[] strs)
		{
			if (strs.Length == 0)
				return "-";
			if (strs.Length == 1)
				return strs[0];

			return strs.Aggregate((s, s1) => s + "," + s1);
		}

		private IEnumerator HandleSpeak(IAction speakAction)
		{
			Name nextState = speakAction.Parameters[1];
			var dialog =
				m_iat.GetDialogueActions(IntegratedAuthoringToolAsset.AGENT, speakAction.Parameters[0], nextState,
					speakAction.Parameters[2], speakAction.Parameters[3]).Shuffle().FirstOrDefault();

			if (dialog == null)
			{
				Debug.LogWarning("Unknown dialog action.");
				m_dialogController.AddDialogLine("... (unkown dialogue) ...");
			}
			else
			{
				m_dialogController.AddDialogLine(dialog.Utterance);
                reply = dialog;
                just_talked = true;


                string subFolder = m_scenarioData.TTSFolder;
				if (subFolder != "<none>")
				{
					var id = DialogUtilities.GenerateFileKey(dialog);
					var provider = (AssetManager.Instance.Bridge as AssetManagerBridge)._provider;

					var path = string.Format("/TTS-Dialogs/{0}/{1}/{2}", subFolder, id,
						DialogUtilities.UtteranceHash(dialog.Utterance));

					AudioClip clip = null; //Resources.Load<AudioClip>(path);
					string xml = null; //Resources.Load<TextAsset>(path);
                    
                    var xmlPath = path + ".xml";
					if (provider.FileExists(xmlPath))
					{
						try
						{
							using (var xmlStream = provider.LoadFile(xmlPath, FileMode.Open, FileAccess.Read))
							{
								using (var reader = new StreamReader(xmlStream))
								{
									xml = reader.ReadToEnd();
								}
							}
						}
						catch (Exception e)
						{
							Debug.LogException(e);
						}

						if (!string.IsNullOrEmpty(xml))
						{
							var wavPath = path + ".wav";
							if (provider.FileExists(wavPath))
							{
								try
								{
									using (var wavStream = provider.LoadFile(wavPath, FileMode.Open, FileAccess.Read))
									{
										var wav = new WavStreamReader(wavStream);

										clip = AudioClip.Create("tmp", (int)wav.SamplesLength, wav.NumOfChannels, (int)wav.SampleRate, false);
										clip.SetData(wav.GetRawSamples(), 0);
									}
								}
								catch (Exception e)
								{
									Debug.LogException(e);
									if (clip != null)
									{
										clip.UnloadAudioData();
										clip = null;
									}
								}
							}
						}
					}
                  
                    if (clip != null && xml != null)
					{
						yield return _body.PlaySpeech(clip, xml);
						clip.UnloadAudioData();
						//Resources.UnloadAsset(clip);
						//Resources.UnloadAsset(text);
					}
					else
					{
						Debug.LogWarning("Could not found speech assets for dialog id \"" + id + "\"");
						yield return new WaitForSeconds(2);
					}
				}
				else
					yield return new WaitForSeconds(2);

				if (nextState.ToString() != "-") //todo: replace with a constant
					AddEvent(string.Format("Event(Property-change,self,DialogueState(Player),{0})", nextState));
			}

			if (speakAction.Parameters[1].ToString() != "-") //todo: replace with a constant
			{
				var dialogueStateUpdateEvent = string.Format("Event(Property-Change,SELF,DialogueState({0}),{1})", speakAction.Target, speakAction.Parameters[1]);
				AddEvent(dialogueStateUpdateEvent);
			}
			_rpc.ActionFinished(speakAction);
		}

		private IEnumerator HandleFix(IAction actionRpc)
		{
			var leaveEvt = string.Format("Event(Property-change,{0},Front(Self),Socket)", _rpc.Perspective);
			_events.Add(leaveEvt);

			yield return new WaitForSeconds(1.5f);

			var fixedEvt = string.Format("Event(Property-change,{0},IsBroken({1}),false)", _rpc.Perspective, actionRpc.Target);
			_events.Add(fixedEvt);
			var enterEvt = string.Format("Event(Property-change,{0},Front(Self),Computer)", _rpc.Perspective);
			_events.Add(enterEvt);
			_rpc.ActionFinished(actionRpc);
		}

		private IEnumerator HandleDisconnectAction(IAction actionRpc)
		{
			var exitEvtOne = string.Format("Event(Property-change,{0},Front(Self),-)", _rpc.Perspective);
			_events.Add(exitEvtOne);
			_rpc.PerceptionActionLoop(_events);
			yield return null;
			_rpc.ActionFinished(actionRpc);
			AddEvent(string.Format("Event(Property-change,SELF,DialogueState(Player),{0})", "Disconnected"));
			if(_body)
				_body.Hide();
			yield return new WaitForSeconds(2);
		    GameObject.Destroy(GameObject.FindGameObjectWithTag("Score"));

            _finalScore.SetActive(true);
            GameObject.FindGameObjectWithTag("FinalScoreText").GetComponent<FinalScoreScript>().FinalScore(RPC.Mood);



        }

	    public void End()
	    {
           _finalScore.SetActive(false);

            m_dialogController.Clear();
            m_versionMenu.SetActive(true);

        }

	    public void storeFinalScore(GameObject g)
	    {

	        _finalScore = g;
         
	    }
        public DialogueStateActionDTO getReply()
        {
            just_talked = false;
            return reply;
        }
        public bool getJustReplied()
        {
            return just_talked;
        }
	}
}