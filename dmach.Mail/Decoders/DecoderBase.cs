using System;
using System.IO;
using System.Text;

namespace InSolve.dmach.Mail.Decoders
{
	/// <summary>
	/// ������� ����� ��� ���������, �������� ���������� ��������� �������.
	/// </summary>
	public abstract class DecoderBase
	{
		#region Fields

		/// <summary>
		/// ����� ���������� ��������������� ������.
		/// </summary>
        readonly MemoryStream _stream = new MemoryStream(4096);
        /// <summary>
        /// ������ ��� �������������
        /// </summary>
		readonly object _syncRoot = new object();

		#endregion

		#region Abstract methods

		/// <summary>
		/// ���������� ������.
		/// </summary>
		/// <param name="b">������ ����.</param>
		/// <param name="start">������� ������.</param>
		/// <param name="length">�����</param>
		/// <returns>���������� ���-�� �������������� ����.</returns>
		public abstract int Add (byte[] b, int start, int length);

		#endregion

		#region Methods

		/// <summary>
		/// ���������� ������ �� ������. (����������� ��� ������������� ����������)
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public int Add(string str)
		{
            return Add(Encoding.Default.GetBytes(str), 0, str.Length);
		}

		public byte[] ToArray()
		{
			lock(_syncRoot)
			{
				byte[] bytes = _stream.ToArray();
				_stream.SetLength(0);
				return bytes;
			}
		}

		public int WriteTo(Stream stream)
		{
			if (_stream.Length == 0)
				return 0;

			if (stream == null)
				throw new ArgumentNullException("stream");

			if (!stream.CanWrite)
				throw new NotSupportedException("stream cannot writable");

			lock(_syncRoot)
			{
				long length = _stream.Length;
				_stream.Position = 0;
				_stream.WriteTo(stream);
				_stream.SetLength(0);
				return (int)length;
			}
		}

		public int ReadToEnd(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			ParserStreamPos pos = new ParserStreamPos(4096);
			int count = 0;

			for(;;)
			{
				if (pos.IsFillNeed)
				{
					pos.Read(stream);
					if (pos.EndOfStream)
						break;
				}

				count += Add(
					pos.Buffer,
					pos.CurrentPosition,
					pos.GetLengthToEnd()
					);

				pos.CurrentPosition += pos.GetLengthToEnd();
			}

			return count;
		}

		#endregion

		#region Protected properties

		/// <summary>
		/// Singleton buffer ;-)
		/// </summary>
		protected MemoryStream InnerBuffer
		{
			get
			{
				return _stream;
			}
		}

		protected object StreamSyncRoot
		{
			get
			{
				return _syncRoot;
			}
		}

		#endregion
	}
}
