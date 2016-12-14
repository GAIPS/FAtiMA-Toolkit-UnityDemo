#if UNITY_ANDROID

using System.Collections.Generic;
using System.IO;
using Assets.Plugins;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace Assets.Scripts
{
	public sealed class AndroidStreamingAssetStorageProvider : IStorageProvider
	{
		private static Dictionary<string, byte[]> m_retrievedFiles = new Dictionary<string, byte[]>();

		public Stream LoadFile(string absoluteFilePath, FileMode mode, FileAccess access)
		{
			using (var file = File.OpenRead(Application.dataPath))
			{
				using (var zip = new ZipFile(file))
				{
					var entryIndex = zip.FindEntry("assets" + absoluteFilePath.Replace('\\', '/'), false);
					if(entryIndex<0)
						throw new FileNotFoundException();

					var stream = zip.GetInputStream(entryIndex);
					var m = new MemoryStream();
					stream.CopyTo(m);
					m.Position = 0;
					return m;
				}
			}
		}

		public bool FileExists(string absoluteFilePath)
		{
			using (var file = File.OpenRead(Application.dataPath))
			{
				using (var zip = new ZipFile(file))
				{
					var entryIndex = zip.FindEntry("assets" + absoluteFilePath.Replace('\\', '/'), false);
					return entryIndex >= 0;
				}
			}
		}
	}
}

#endif