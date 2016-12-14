using System;
using System.IO;
using AssetPackage;
using Assets.Plugins;
using UnityEngine;

namespace Assets.Scripts
{
	public class AssetManagerBridge : IBridge, ILog, IDataStorage
	{
		public readonly IStorageProvider _provider;

		public AssetManagerBridge()
		{

#if !UNITY_EDITOR
#if UNITY_WEBGL
			_provider = new WebGLStorageProvider();
#elif UNITY_ANDROID
			_provider = new AndroidStreamingAssetStorageProvider();
#else
			_provider = new DefaultStreamingAssetsStorageProvider();
#endif
#else
			_provider = new DefaultStreamingAssetsStorageProvider();
#endif
		}

		public void Log(Severity severity, string msg)
		{
			switch (severity)
			{
				case Severity.Critical:
				case Severity.Error:
					Debug.LogError(msg);
					break;
				case Severity.Warning:
					Debug.LogWarning(msg);
					break;
				case Severity.Information:
				case Severity.Verbose:
					Debug.Log(msg);
					break;
				default:
					throw new ArgumentOutOfRangeException("severity", severity, null);
			}
		}

		public bool Delete(string fileId)
		{
			throw new InvalidOperationException();
		}

		public bool Exists(string fileId)
		{
			return _provider.FileExists(fileId);
		}

		public string[] Files()
		{
			throw new InvalidOperationException();
		}

		public string Load(string fileId)
		{
			using (var reader = new StreamReader(_provider.LoadFile(fileId, FileMode.Open, FileAccess.Read)))
			{
				return reader.ReadToEnd();
			}
		}

		public void Save(string fileId, string fileData)
		{
			throw new InvalidOperationException();
		}
	}
}