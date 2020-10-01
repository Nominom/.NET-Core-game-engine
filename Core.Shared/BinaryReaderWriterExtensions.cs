using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Shared
{
	public static class BinaryReaderWriterExtensions
	{
		public static unsafe void WriteStruct<T>(this BinaryWriter bw, T t) where T : unmanaged
		{
			int size = Unsafe.SizeOf<T>();
			byte* bytes = stackalloc byte[size];
			Unsafe.Write(bytes, t);
			Span<byte> bSpan = new Span<byte>(bytes, size);
			bw.Write(bSpan);
		}

		public static unsafe T ReadStruct<T>(this BinaryReader br) where T : unmanaged
		{
			int size = Unsafe.SizeOf<T>();
			byte* bytes = stackalloc byte[size];
			Span<byte> bSpan = new Span<byte>(bytes, size);
			br.Read(bSpan);
			return Unsafe.Read<T>(bytes);
		}

		public static void AddPadding(this BinaryWriter bw, uint padding)
		{
			for (uint i = 0; i < padding; i++)
			{
				bw.Write((byte)0);
			}
		}

		public static void SkipPadding(this BinaryReader br, uint padding)
		{
			for (uint i = 0; i < padding; i++)
			{
				br.ReadByte();
			}
		}

		public static void WriteUtf8String(this BinaryWriter writer, string input) {
			var encoding = Encoding.UTF8;

			var inputSpan = input.AsSpan();
			int inputByteCount = encoding.GetByteCount(input);
			Span<byte> inputBytes = stackalloc byte[inputByteCount];

			encoding.GetBytes(inputSpan, inputBytes);

			writer.Write(inputByteCount);
			writer.Write(inputBytes);
		}

		public static string ReadUtf8String(this BinaryReader reader) {
			var encoding = Encoding.UTF8;

			int byteCount = reader.ReadInt32();
			Span<byte> bytes = stackalloc byte[byteCount];
			reader.Read(bytes);

			return encoding.GetString(bytes);
		}
	}
}
