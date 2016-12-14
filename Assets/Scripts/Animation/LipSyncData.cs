using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

namespace Assets.Scripts.Animation
{
	public class LipSyncData : ScriptableObject
	{
		public static LipSyncData CreateFromFile(string xmlString, AudioClip audioClip)
		{
			var data = CreateInstance<LipSyncData>();

			List<VisemeData> list = new List<VisemeData>();
			if (!string.IsNullOrEmpty(xmlString))
			{
				try
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(xmlString);

					if (doc.DocumentElement.Name != "LipSyncVisemes")
						throw new Exception("Document root element mismatch");

					var root = doc.DocumentElement;
					foreach (XmlNode node in root.ChildNodes)
					{
						if (node.Name != "viseme")
							throw new Exception("Invalid element \"" + node.Name + "\"");

						if (node.ChildNodes.Count > 0)
							throw new Exception("viseme nodes cannot contain children");

						var v = node.Attributes["type"].Value;
						var t = node.Attributes["time"].Value;
						var d = node.Attributes["duration"].Value;

						var viseme = (Visemes)Enum.Parse(typeof(Visemes), v);
						var time = float.Parse(t, CultureInfo.InvariantCulture);
						var duration = float.Parse(d, CultureInfo.InvariantCulture);

						list.Add(new VisemeData(viseme, time, duration));
					}

				}
				catch (Exception e)
				{
					Debug.Log("Error when loading XML file: " + e.Message);
					return null;
				}
			}
			
			data._visemes = list.ToArray();
			data._audioClip = audioClip;

			return data;
		}

		[SerializeField]
		private VisemeData[] _visemes;
		[SerializeField]
		private AudioClip _audioClip;

		public IList<VisemeData> Visemes {
			get { return _visemes; }
		}

		public AudioClip AudioClip {
			get { return _audioClip; }
		}
	}

	[Serializable]
	public class VisemeData
	{
		[SerializeField]
		private Visemes _viseme;
		[SerializeField]
		private float _time;
		[SerializeField]
		private float _duration;

		public VisemeData(Visemes viseme, float time, float duration)
		{
			_viseme = viseme;
			_time = time;
			_duration = duration;
		}

		public Visemes Viseme {
			get { return _viseme; }
		}

		public float Time {
			get { return _time; }
		}

		public float Duration {
			get { return _duration; }
		}
	}

	public enum Visemes : byte
	{
		Silence = 0,
		AeAxAh = 1,
		Aa = 2,
		Ao = 3,
		EyEhUh = 4,
		Er = 5,
		YIyIhIx = 6,
		WUw = 7,
		Ow = 8,
		Aw = 9,
		Oy = 10,
		Ay = 11,
		H = 12,
		R = 13,
		L = 14,
		SZ = 15,
		ShChJhZh = 16,
		ThDh = 17,
		FV = 18,
		DTN = 19,
		KGNg = 20,
		PBM = 21
	}
}