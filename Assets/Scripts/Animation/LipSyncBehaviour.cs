using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Animation
{
	[RequireComponent(typeof (FaceController), typeof (AudioSource))]
	public class LipSyncBehaviour : MonoBehaviour
	{
		[SerializeField] private string _AI_expression = "AI_phoneme";
		[SerializeField] private string _E_expression = "E_phoneme";
		[SerializeField] private string _U_expression = "U_phoneme";
		[SerializeField] private string _O_expression = "O_phoneme";
		[SerializeField] private string _CDGKNRSThYZ_expression = "CDGKNRSThYZ_phoneme";
		[SerializeField] private string _FV_expression = "FV_phoneme";
		[SerializeField] private string _L_expression = "L_phoneme";
		[SerializeField] private string _MBP_expression = "MBP_phoneme";
		[SerializeField] private string _WQ_expression = "WQ_phoneme";

		[SerializeField] private bool _disableVisemes = false;

		[SerializeField]
		[Range(0.0001f,float.MaxValue)]
		private float _phonemeTime = 0.1f;

		private AudioSource _audio;
		private AudioSource audioPlayer
		{
			get { return _audio ?? (_audio = GetComponent<AudioSource>()); }
		}
		
		private Dictionary<Visemes, FaceController.IExpressionController> _expressionCache = null;
		private FaceController.IExpressionController _currentExpression = null;
		private List<VisemeData> _cachedVisemes = new List<VisemeData>();
		private Coroutine _playCoroutine = null;
		private bool _isPaused = false;

		public bool IsPlaying {
			get { return _playCoroutine != null; }
		}

		public bool IsPaused {
			get { return _isPaused; }
		}

		private void BuildExpressionCache()
		{
			if(_expressionCache!=null)
				return;

			_expressionCache=new Dictionary<Visemes, FaceController.IExpressionController>();

			var faceController = GetComponent<FaceController>();

			foreach (Visemes viseme in Enum.GetValues(typeof (Visemes)))
			{
				var id = VisemeEnumToExpressionId(viseme);
				if(id==null)
					continue;
				try
				{
					_expressionCache.Add(viseme, faceController.GetExpressionController(id));
				}
				catch (Exception)
				{
					Debug.LogWarning("Unable to find expression for viseme \""+id+"\"");
				}
			}
		}

		private string VisemeEnumToExpressionId(Visemes v)
		{
			switch (v)
			{
				case Visemes.Silence:
					return null;
				case Visemes.AeAxAh:
				case Visemes.Aa:
				case Visemes.Ao:
				case Visemes.Ay:
				case Visemes.H:
					return _AI_expression;
				case Visemes.EyEhUh:
				case Visemes.Er:
				case Visemes.YIyIhIx:
					return _E_expression;
				case Visemes.WUw:
				case Visemes.Ow:
					return _U_expression;
				case Visemes.Oy:
					return _O_expression;
				case Visemes.Aw:
					return _WQ_expression;
				case Visemes.L:
					return _L_expression;
				case Visemes.FV:
					return _FV_expression;
				case Visemes.PBM:
					return _MBP_expression;
				case Visemes.R:
				case Visemes.SZ:
				case Visemes.ShChJhZh:
				case Visemes.ThDh:
				case Visemes.DTN:
				case Visemes.KGNg:
					return _CDGKNRSThYZ_expression;	
			}

			throw new ArgumentOutOfRangeException("v", v, null);
		}

		public void Play(LipSyncData dataFile, bool force=false)
		{
			if (IsPlaying)
			{
				if(force)
					Stop();
				else
					throw new Exception("Lip sync is already playing.");
			}

			//Load data from file and execute coroutine

			_cachedVisemes.AddRange(dataFile.Visemes);
			// Phonemes are stored out of sequence in the file, for depth sorting in the editor
			// Sort them by timestamp to make finding the current one faster
			_cachedVisemes.Sort((p1, p2) => p1.Time.CompareTo(p2.Time));
			audioPlayer.clip = dataFile.AudioClip;

			_playCoroutine = StartCoroutine(PlayCoroutine(!_disableVisemes));
		}

		public void Pause()
		{
			if (!IsPlaying)
				throw new Exception("No lip sync data currently playing.");

			if (_isPaused)
				return;

			_isPaused = true;
			audioPlayer.Pause();
			if (_currentExpression != null)
			{
				if (_currentExpression != null)
					_currentExpression.TargetAmount = 0;
				_currentExpression = null;
			}
		}

		public void Resume()
		{
			if (!IsPlaying)
				throw new Exception("No lip sync data currently playing.");

			if (!_isPaused)
				return;

			_isPaused = false;
			audioPlayer.UnPause();
		}

		public void Stop()
		{
			if (!IsPlaying)
				throw new Exception("No lip sync data currently playing.");

			_isPaused = false;
			StopCoroutine(_playCoroutine);
			StopCurrentPlay();
		}

		private IEnumerator PlayCoroutine(bool useVisemes)
		{
			int index = 0;
			var audio = audioPlayer;
			float time = 0;
			audio.loop = false;
			audio.Play();

			if (useVisemes)
			{
				BuildExpressionCache();
				while (index < _cachedVisemes.Count)
				{
					if (_isPaused)
						yield return new WaitWhile(() => _isPaused);

					var currentViseme = _cachedVisemes[index];

					float startTime = currentViseme.Time;
					float endTime = currentViseme.Time + currentViseme.Duration;
					if (time >= startTime)
					{
						if (_currentExpression == null)
						{
							//Debug.Log(currentViseme.Viseme);
							_currentExpression = _expressionCache[currentViseme.Viseme];
						}

						//const float PEAK = 0.25f;
						//float p = Mathf.Clamp01((audio.time - startTime) / (endTime - startTime));
						//float blendAmount = p >= PEAK ? 1f- BlendEase((p - PEAK)/ (1f-PEAK)) : BlendEase(p/PEAK);
						//_currentExpression.Amount = blendAmount;
						_currentExpression.TargetAmount = 1;
					}
					yield return null;
					time += Time.deltaTime;
					if (endTime < time)
					{
						if (_currentExpression != null)
							_currentExpression.TargetAmount = 0;
						_currentExpression = null;
						index++;
					}
				}
			}

			yield return new WaitWhile(() => audio.isPlaying);
			StopCurrentPlay();
		}

		private void StopCurrentPlay()
		{
			if (_currentExpression != null)
				_currentExpression.TargetAmount = 0;
			_currentExpression = null;

			_playCoroutine = null;
			audioPlayer.Stop();
			_cachedVisemes.Clear();
		}

		private static float BlendEase(float t)
		{
			return -0.5f*(Mathf.Cos(Mathf.PI*t) - 1);
		}
	}
}
