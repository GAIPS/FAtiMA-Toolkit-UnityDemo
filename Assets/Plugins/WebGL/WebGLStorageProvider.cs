#if UNITY_WEBGL

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Assets.Plugins;
using UnityEngine;
using Utilities;

namespace Assets.Scripts
{
	public class WebGLStorageProvider : IStorageProvider
	{
		private const int MAX_BUFFER_SIZE = 1024;

		[DllImport("__Internal")]
		private static extern void WebGl_Storage_Initialize();

		[DllImport("__Internal")]
		private static extern bool WebGl_Storage_IsInitialized();

		[DllImport("__Internal")]
		private static extern int LoadRemoteFile(string path);

		[DllImport("__Internal")]
		private static extern string HasErrors(int id);

		[DllImport("__Internal")]
		private static extern int WebGl_Storage_ReadBytes(int id, byte[] buffer, int index, int count);

		[DllImport("__Internal")]
		private static extern bool WebGl_Storage_IsEndOfFile(int id, int index);

		private static Dictionary<string,byte[]> m_retrievedFiles=new Dictionary<string, byte[]>();

		public WebGLStorageProvider()
		{
			if (!WebGl_Storage_IsInitialized())
				WebGl_Storage_Initialize();
		}

		public bool FileExists(string absoluteFilePath)
		{
			if (m_retrievedFiles.ContainsKey(absoluteFilePath))
				return true;

			try
			{
				PullFile(absoluteFilePath);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				return false;
			}

			return true;
		}

		public Stream LoadFile(string absoluteFilePath, FileMode mode, FileAccess access)
		{
			if (mode != FileMode.Open)
				throw new System.NotImplementedException("Only open mode is implemented at the moment.");

			if (access != FileAccess.Read)
				throw new System.NotImplementedException("Only read access is implemented at the moment.");

			byte[] data;
			if (!m_retrievedFiles.TryGetValue(absoluteFilePath, out data))
			{
				PullFile(absoluteFilePath);
				data = m_retrievedFiles[absoluteFilePath];
			}
			return new MemoryStream(data,false);
		}

		private static void PullFile(string absoluteFilePath)
		{
			var rootedPath = RootPath(absoluteFilePath);
			var id = LoadRemoteFile(rootedPath);
			var errorMsg = HasErrors(id);
			if (errorMsg != null)
				throw new IOException(errorMsg);

			byte[] buffer = new byte[MAX_BUFFER_SIZE];
			int index = 0;

			using (var m = new MemoryStream())
			{
				do
				{
					var readed = WebGl_Storage_ReadBytes(id, buffer, index, MAX_BUFFER_SIZE);
					m.Write(buffer, 0, readed);
					index += readed;
				} while (!WebGl_Storage_IsEndOfFile(id, index));

				m_retrievedFiles[absoluteFilePath] = m.GetBuffer();
			}
		}

		private static string RootPath(string path)
		{
			path = path.Split('/', '\\').Select(s => WWW.EscapeURL(s)).AggregateToString("/");
			Debug.Log(path);
			if (Path.IsPathRooted(path))
				return Application.streamingAssetsPath + path;
			return Path.Combine(Application.streamingAssetsPath, path);
		}
	}
}

#endif