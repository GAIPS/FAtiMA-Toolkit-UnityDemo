using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
	public sealed class WavStreamReader
	{
		public ushort NumOfChannels { get; private set; }
		public uint SampleRate { get; private set; }
		public ushort BitsPerSample { get; private set; }
		public uint SamplesLength { get; private set; }

		private float[] _samples;

		public WavStreamReader(Stream byteStream)
		{
			using (var reader = new BinaryReader(byteStream,Encoding.ASCII))
			{
				ChunkLoader(reader,"RIFF", (r,totalSize) =>
				{
					var format = Encoding.ASCII.GetString(r.ReadBytes(4));
					if (format != "WAVE")
						throw new FormatException("Invalid Wav format");

					ChunkLoader(r,"fmt ",(r2,s)=>
					{
						var audioFormat = r2.ReadUInt16();
						if (audioFormat != 1)
							throw new FormatException("Only PCM Wav formats are supported");

						NumOfChannels = r2.ReadUInt16();
						SampleRate = r2.ReadUInt32();
						var byteRate = r2.ReadUInt32();
						var blockAllign = r2.ReadUInt16();
						BitsPerSample = r2.ReadUInt16();

						var calculatedBlockAllign = NumOfChannels * (BitsPerSample / 8);
						if (calculatedBlockAllign != blockAllign)
							throw new FormatException("Mismatch in the block allign bytes");

						var calculatedByteRate = SampleRate * calculatedBlockAllign;
						if (calculatedByteRate != byteRate)
							throw new FormatException("Mismatch in the byte rate specification bytes");
					});

					ReadChunks(r,totalSize - r.BaseStream.Position,
						new Dictionary<string, Action<BinaryReader, uint>>()
						{
							{"data",ReadDataChunk}
						}
					);
				});
			}
		}

		private static void ReadChunks(BinaryReader reader,long maxBytes, IDictionary<string, Action<BinaryReader, uint>> chunkParsers)
		{
			var initialPos = reader.BaseStream.Position;
			while ((reader.BaseStream.Position - initialPos)<maxBytes)
			{
				var header = Encoding.ASCII.GetString(reader.ReadBytes(4));
				var size = reader.ReadUInt32();

				var pos = reader.BaseStream.Position;

				Action<BinaryReader, uint> loader;
				if (chunkParsers.TryGetValue(header, out loader))
				{
					loader(reader, size);
				}

				reader.BaseStream.Position = pos + size;
			}
		}

		private static void ChunkLoader(BinaryReader reader, string chunkHeader, Action<BinaryReader,uint> loader)
		{
			var header = Encoding.ASCII.GetString(reader.ReadBytes(4));
			if(header != chunkHeader)
				throw new FormatException("Invalid "+chunkHeader+" chunk header");

			var size = reader.ReadUInt32();
			var totalSize = reader.BaseStream.Length - reader.BaseStream.Position;
			if (size > totalSize)
				throw new FormatException("Invalid stream size. Expecting " + size + " bytes, but only " + totalSize + " are available.");

			var pos = reader.BaseStream.Position;
			loader(reader,size);
			reader.BaseStream.Position = pos + size;
		}

		private void ReadDataChunk(BinaryReader reader, uint chunkSize)
		{
			ushort blockAllign = (ushort)(NumOfChannels * (BitsPerSample / 8));
			SamplesLength = chunkSize / blockAllign;

			var totalSamplesToRead = SamplesLength*NumOfChannels;
			_samples = new float[totalSamplesToRead];
			for (uint i = 0; i < totalSamplesToRead; i++)
				_samples[i] = ReadSample(reader, BitsPerSample);
		}

		public float[] GetRawSamples()
		{
			return _samples;
		}

		private static float ReadSample(BinaryReader reader, ushort bitsPerSample)
		{
			switch (bitsPerSample)
			{
				case 8:
				{
					var b = reader.ReadSByte();
					var r = b/(float) sbyte.MaxValue;
					return r;
				}
				case 16:
				{
					var s = reader.ReadInt16();
					var r = s/(float) short.MaxValue;
					return r;
				}
			}

			throw new NotSupportedException();
		}
	}
}