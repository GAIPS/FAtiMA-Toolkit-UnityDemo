using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ActionLibrary;
using IntegratedAuthoringTool;
using IntegratedAuthoringTool.DTOs;
using RolePlayCharacter;
using UnityEngine;
using Assets.Plugins;
using Utilities;
using WellFormedNames;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
	public class AgentControler
	{
		private RolePlayCharacterAsset m_rpc;
		private DialogController m_dialogController;
		private IntegratedAuthoringToolAsset m_iat;
		public UnityBodyImplement _body;

		private List<Name> _events = new List<Name>();
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
        

		public RolePlayCharacterAsset RPC { get { return m_rpc; } }

		public AgentControler(SingleCharacterDemo.ScenarioData scenarioData, RolePlayCharacterAsset rpc,
			IntegratedAuthoringToolAsset iat, UnityBodyImplement archetype, Transform anchor, DialogController dialogCrt)
		{
			m_scenarioData = scenarioData;
            m_iat = iat;
            m_rpc = rpc;
            m_dialogController = dialogCrt;
			_body = GameObject.Instantiate(archetype);
		    just_talked = false;
            

            var t = _body.transform;
			t.SetParent(anchor, false);
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
			m_dialogController.SetCharacterLabel(m_rpc.CharacterName.ToString());

            m_rpc.Perceive(new Name[] { EventHelper.PropertyChange("DialogueState(Player)", "Start", "world")});
		}

		public void AddEvent(string eventName)
		{
			_events.Add((Name)eventName);
		}

		public void SetExpression(string emotion, float amount)
		{
			_body.SetExpression(emotion, amount);
		}

		public void SaveOutput()
		{
			const string datePattern = "dd-MM-yyyy-H-mm-ss";
			m_rpc.SaveToFile(Application.streamingAssetsPath + "\\Output\\" + m_rpc.CharacterName + "-" + DateTime.Now.ToString(datePattern) + ".ea");
		}

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
			m_dialogController.UpdateFields(m_rpc);
		}

		public void UpdateEmotionExpression()
		{
			var emotion = m_rpc.GetStrongestActiveEmotion();
			if (emotion == null)
				return;

			//_body.SetExpression(emotion.EmotionType, emotion.Intensity/10f);
		}

		private IEnumerator UpdateCoroutine()
		{
            //	_events.Clear();
           // Debug.Log(m_rpc.GetBeliefValue("HasFloor(" + "SELF" + ")"));

            while (m_rpc.GetBeliefValue(string.Format(IATConsts.DIALOGUE_STATE_PROPERTY, IATConsts.PLAYER)) != IATConsts.TERMINAL_DIALOGUE_STATE) 
			{
				yield return new WaitForSeconds(1);
               
              if(  _body._speechController.IsPlaying)
                    continue;

                var action = m_rpc.Decide().Shuffle().FirstOrDefault();
                _events.Clear(); 
				m_rpc.Update();

				if (action == null)
					continue;

			//	Debug.Log("Action Key: " + action.Key);

				switch (action.Key.ToString())
				{
					case "Speak":
						m_activeController.StartCoroutine(HandleSpeak(action));

						break;
					case "Disconnect":
                        m_activeController.StartCoroutine(newHandleDisconnect());
                        m_dialogController.AddDialogLine(string.Format("- {0} disconnects -", m_rpc.CharacterName));

                        _currentCoroutine = null;
                        Object.Destroy(_body.Body);
                        GameObject.FindObjectOfType<SingleCharacterDemo>().Restart();
                        break;
					default:
						Debug.LogWarning("Unknown action: " + action.Key);
						break;
				}
			}
		}
     
		private IEnumerator HandleSpeak(IAction speakAction)
		{
            //m_rpc.Perceive(new [] { EventHelper.ActionStart(m_rpc.CharacterName.ToString(), speakAction.Name.ToString(), IATConsts.PLAYER) });


            Name currentState = speakAction.Parameters[0];
            Name nextState = speakAction.Parameters[1];
            Name meaning = speakAction.Parameters[2];
            Name style = speakAction.Parameters[3];

            var dialogs = m_iat.GetDialogueActions(currentState, nextState, meaning, style);


		    var dialog = dialogs.Shuffle().FirstOrDefault();

   //         Debug.Log("Going to say: " + dialog.Utterance);

		    if (dialog == null)
			{
				Debug.LogWarning("Unknown dialog action.");
				m_dialogController.AddDialogLine("... (unkown dialogue) ...");
			}
			else
			{
                string subFolder = m_scenarioData.TTSFolder;
				if (subFolder != "<none>")
				{
					var path = string.Format("/TTS-Dialogs/{0}/{1}/{2}", subFolder, m_rpc.VoiceName, dialog.UtteranceId);
					var absolutePath = Application.streamingAssetsPath;
#if UNITY_EDITOR || UNITY_STANDALONE
					absolutePath = "file://" + absolutePath;
#endif
					string audioUrl = absolutePath +path+ ".wav";
					string xmlUrl = absolutePath +path+ ".xml";

					var audio = new WWW(audioUrl);
					var xml = new WWW(xmlUrl);

					yield return audio;
					yield return xml;

					var xmlError = !string.IsNullOrEmpty(xml.error);
					var audioError = !string.IsNullOrEmpty(audio.error);

					if(xmlError)
						Debug.LogError(xml.error);
					if(audioError)
						Debug.LogError(audio.error);

					m_dialogController.AddDialogLine(dialog.Utterance);

					if (xmlError || audioError)
					{
						yield return new WaitForSeconds(2);
					}
					else
					{
						var clip = audio.GetAudioClip(false);
						yield return _body.PlaySpeech(clip, xml.text);
						clip.UnloadAudioData();
					}

				
				
				}
				else
				{
					m_dialogController.AddDialogLine(dialog.Utterance);
					yield return new WaitForSeconds(2);

				
				}

                  Debug.Log("Just talked");
                    reply = dialog;
                    just_talked = true;
                
            }

			
		    if (nextState.ToString() == "Disconnect")
		    {
               this.End();
		    }

            AddEvent(EventHelper.ActionEnd(m_rpc.CharacterName.ToString(), speakAction.Name.ToString(), IATConsts.PLAYER).ToString());

		}

		private IEnumerator HandleDisconnectAction(IAction actionRpc)
		{
			yield return null;
			m_rpc.Perceive(new Name[] { EventHelper.ActionEnd(m_rpc.CharacterName.ToString(), actionRpc.Name.ToString(), IATConsts.PLAYER) });
            AddEvent(EventHelper.PropertyChange(string.Format(IATConsts.DIALOGUE_STATE_PROPERTY,IATConsts.PLAYER),"Disconnected", "SELF").ToString());
			if(_body)
				_body.Hide();
			yield return new WaitForSeconds(2);
		    GameObject.Destroy(GameObject.FindGameObjectWithTag("Score"));
		    if (m_scenarioData.IAT.ScenarioName.Contains("PJ"))
		    {
		        _finalScore.SetActive(true);
		        GameObject.FindGameObjectWithTag("FinalScoreText").GetComponent<FinalScoreScript>().FinalScore(RPC.Mood);
		    }
		}


	    private IEnumerator newHandleDisconnect()
	    {
            if (_body)
                _body.Hide();
            yield return new WaitForSeconds(2);
            GameObject.Destroy(GameObject.FindGameObjectWithTag("Score"));
	        if (m_scenarioData.IAT.ScenarioName.Contains("PJ") || m_scenarioData.IAT.ScenarioName.Contains("Space"))
	        {
	            _finalScore.SetActive(true);
	            GameObject.FindGameObjectWithTag("FinalScoreText").GetComponent<FinalScoreScript>().FinalScore(RPC.Mood);
	        }
	        m_dialogController.Clear();
        }

	    public void End()
	    {
            if (_body)
                _body.Hide();
          //  yield return new WaitForSeconds(2);
            GameObject.Destroy(GameObject.FindGameObjectWithTag("Score"));
            _finalScore.SetActive(true);
            GameObject.FindGameObjectWithTag("FinalScoreText").GetComponent<FinalScoreScript>().FinalScore(RPC.Mood);
            m_dialogController.Clear();

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


        public void WaitingToSpeak()
        {

        }
	}
}