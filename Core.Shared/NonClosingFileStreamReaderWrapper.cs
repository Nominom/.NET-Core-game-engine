using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Shared
{
	public class NonClosingFileStreamReaderWrapper : Stream {
		private readonly Stream sourceStream;
		private readonly long startPosition;
		private long length;

		public NonClosingFileStreamReaderWrapper(Stream source) {
			sourceStream = source;
			startPosition = sourceStream.Position;
			length = -1;
		}

		public NonClosingFileStreamReaderWrapper(Stream source, long length) {
			sourceStream = source;
			startPosition = sourceStream.Position;
			this.length = length;
		}

		public override void Flush() {
			sourceStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count) {
			long length = Math.Min(count, this.length - Position);
			if (length <= 0) return 0;
			return sourceStream.Read(buffer, offset, (int)length);
		}

		public override long Seek(long offset, SeekOrigin origin) {
			if (origin == SeekOrigin.Begin) {
				return sourceStream.Seek(offset + startPosition, origin);
			}
			else if(origin == SeekOrigin.Current){
				return sourceStream.Seek(offset, origin);
			}
			else {
				throw new NotSupportedException();
			}
		}

		public override void SetLength(long value) {
			length = value;
		}

		public override void Write(byte[] buffer, int offset, int count) {
			throw new NotSupportedException();
		}

		public override void Write(ReadOnlySpan<byte> buffer) {
			throw new NotSupportedException();
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
			throw new NotSupportedException();
		}

		public override void EndWrite(IAsyncResult asyncResult) {
			throw new NotSupportedException();
		}

		public override int ReadByte() {
			if (Position >= length) {
				return -1;
			}
			return sourceStream.ReadByte();
		}

		public override int Read(Span<byte> buffer) {
			long length = Math.Min(buffer.Length, this.length - Position);
			if (length <= 0) return 0;
			return sourceStream.Read(buffer.Slice(0, (int)length));
		}

		public override Task FlushAsync(CancellationToken cancellationToken) {
			return sourceStream.FlushAsync(cancellationToken);
		}

		public override void WriteByte(byte value) {
			throw new NotSupportedException();
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
			throw new NotSupportedException();
		}

		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken()) {
			throw new NotSupportedException();
		}

		public override int ReadTimeout {
			get => sourceStream.ReadTimeout;
			set => sourceStream.ReadTimeout = value;
		}

		public override int WriteTimeout {
			get => sourceStream.WriteTimeout;
			set => sourceStream.WriteTimeout = value;
		}

		public override bool CanTimeout => sourceStream.CanTimeout;

		public override bool CanRead => sourceStream.CanRead;
		public override bool CanSeek => sourceStream.CanSeek;
		public override bool CanWrite => false;
		public override long Length => sourceStream.Length;
		public override long Position {
			get => sourceStream.Position - startPosition;
			set => sourceStream.Position = value + startPosition;
		}

		public override void Close() { }
	}
}
