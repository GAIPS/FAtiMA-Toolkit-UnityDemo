#if UNITY_WEBGL

using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;
using GAIPS.Rage;

namespace Assets.Scripts
{
	public class WebGLStorageProvider : BaseStorageProvider
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

		public WebGLStorageProvider() : base(Application.streamingAssetsPath)
		{
			if (!WebGl_Storage_IsInitialized())
				WebGl_Storage_Initialize();
		}

		protected override Stream LoadFile(string filePath, FileMode mode, FileAccess access)
		{
			if (mode != FileMode.Open)
				throw new System.NotImplementedException("Only open mode is implemented at the moment.");

			if (access != FileAccess.Read)
				throw new System.NotImplementedException("Only read access is implemented at the moment.");

			var id = LoadRemoteFile(filePath);
			var errorMsg = HasErrors(id);
			if (errorMsg != null)
				throw new IOException(errorMsg);

			byte[] buffer = new byte[MAX_BUFFER_SIZE];
			int index = 0;

			var m = new MemoryStream();
			do
			{
				var readed = WebGl_Storage_ReadBytes(id, buffer, index, MAX_BUFFER_SIZE);
				m.Write(buffer, 0, readed);
				index += readed;
			} while (!WebGl_Storage_IsEndOfFile(id, index));

			m.Position = 0;
			return m;
		}

		protected override bool IsDirectory(string path)
		{
			return !Path.HasExtension(path);
		}
	}
}

#endif