using System.IO;

namespace Assets.Scripts
{
	public static class Util
	{
		public static void CopyTo(this Stream input, Stream output)
		{
			byte[] buffer = new byte[32768];
			int read;
			while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				output.Write(buffer, 0, read);
			}
		}
	}
}