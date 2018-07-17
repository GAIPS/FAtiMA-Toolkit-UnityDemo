using System.Collections;
using System.Xml;
using Assets.Scripts;
using Assets.Scripts.Animation;
using UnityEngine;

public class UnityBodyImplement : MonoBehaviour
{
	[SerializeField]
	private FaceController _faceController = null;
	[SerializeField]
	public LipSyncBehaviour _speechController = null;

	public GameObject Body {
		get { return gameObject; }
	}

	private object[] _previousParameters;

	private object[] _parameters;

	private FaceController.IExpressionController _currentEmotion;

	public Coroutine PlaySpeech(AudioClip clip, string xmlStr)
	{
		var data = LipSyncData.CreateFromFile(xmlStr, clip);
		return StartCoroutine(PlaySpeechCoroutine(data));
	}

	private IEnumerator PlaySpeechCoroutine(LipSyncData data)
	{
		_speechController.Play(data);
		yield return new WaitWhile(() => _speechController.IsPlaying);
	}

	public void SetExpression(string emotion, float amount)
	{
		var exp = _faceController.GetExpressionController(emotion);

		if (_currentEmotion != null && _currentEmotion != exp)
			_currentEmotion.TargetAmount = 0;

		_currentEmotion = exp;
		_currentEmotion.TargetAmount = amount;
	}

    public void Hide()
    {
		if(gameObject)
			Body.SetActive(false);
    }
}