using System.IO;

namespace Assets.Plugins
{
	public interface IStorageProvider
	{
		Stream LoadFile(string absoluteFilePath, FileMode mode, FileAccess access);
		bool FileExists(string absoluteFilePath);
	}
}