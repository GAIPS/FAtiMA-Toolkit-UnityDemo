#if !(UNITY_WEBGL || UNITY_ANDROID) || UNITY_EDITOR

using System.IO;
using Assets.Plugins;
using UnityEngine;

namespace Assets.Scripts
{
	public class DefaultStreamingAssetsStorageProvider : IStorageProvider
	{
		public Stream LoadFile(string absoluteFilePath, FileMode mode, FileAccess access)
		{
			return File.Open(RootPath(absoluteFilePath), mode, access);
		}

		public bool FileExists(string absoluteFilePath)
		{
			var rootPath = RootPath(absoluteFilePath);
			return File.Exists(rootPath);
		}

		private static string RootPath(string path)
		{
			if (Path.IsPathRooted(path))
				return Application.streamingAssetsPath + path;
			return Path.Combine(Application.streamingAssetsPath, path);
		}
	}
}

#endif