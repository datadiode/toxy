//������� 14.06.2004

using System;
using System.IO;
using System.Text;

namespace InSolve.dmach.Mail
{
	/// <summary>
	/// ����� ��� �������� �����.
	/// </summary>
	public abstract class Parser
	{
		#region Static fields and buffers
		
		/// <summary>
		/// ��������� ����������� ��������� � ������ � �.�.
		/// </summary>
		[ThreadStatic]
		private static ParserStreamPos	_pos;
		
		#endregion

		#region Stream read methods

		/// <summary>
		/// ���������� ������ �� ������. ������ �� ������ �� ������� \r\n _������������_
		/// </summary>
		/// <param name="stream">�����</param>
		/// <param name="buff">�����</param>
		/// <returns></returns>
		private static string ReadStreamLine(Stream stream)
		{
			//�������� ��������, �������� �������!
			//(�� �����������, ������������ � ������� ��������� ����� ������)
			if (_pos.EndOfStream)
				return string.Empty;

			StreamStringBuilder buffer = new StreamStringBuilder();

			for(;;)
			{
				if (_pos.IsFillNeed)
				{
					_pos.Read(stream);
					if (_pos.EndOfStream)
						break;
				}

				int indexOfEnter = _pos.GetIndexOf((int)'\n');

				_pos.CurrentPosition += buffer.Add(
					_pos.Buffer,
					_pos.CurrentPosition,
					(indexOfEnter == -1)? _pos.GetLengthToEnd() : _pos.GetLengthToIndex(indexOfEnter) + 1
					);

				if (indexOfEnter != -1)
					break;
			}

			return buffer.ToString();
		}

		/// <summary>
		/// ������ ������ �� ���������� ����������� �� ������� ��� � ���������.
		/// </summary>
		/// <param name="outStream"></param>
		/// <param name="stream"></param>
		/// <param name="letterError"></param>
		/// <param name="currentBound"></param>
		/// <param name="finalPart"></param>
		private static void ReadStreamToBound
			(
			Stream outStream,
			Stream stream,
            Action<ParserError, string> errorHandler,
			string currentBound,
			out bool finalPart
			)
		{
			//�������� ��������, �������� �������!
			//(�� �����������, ������������ � ������� ��������� ����� ������)
			//�� �������, ����� � ������ ��� �����������
			//�� ��������� ��� ����������� "bound": bbbound
			//��������� ����: ������ ���� --a � ��������� ���� ---a ��� �� ������ �����. ��� ���� :-)
			byte[] bound = ParserTools.GetBytes(currentBound);

			for(int progress = 0;;)
			{
				if (bound != null && bound.Length == progress)
					break;

				if (_pos.IsFillNeed)
				{
					_pos.Read(stream);
					if (_pos.EndOfStream)
					{
						if (progress > 0)
							if (outStream != null)
								outStream.Write(bound, 0, progress);
						break;
					}
				}

				int count = _pos.GetLengthToIndex(
					(bound != null)? _pos.GetIndexOf(bound[progress]) : -1
					);

				if (count == 0)
				{
					progress++;
					_pos.CurrentPosition++;
					continue;
				}
				if (progress > 0)
				{
					if (outStream != null)
						outStream.Write(bound, 0, progress);
					progress = 0;
					continue;
				}
				if (count < 0)
					count = _pos.GetLengthToEnd();
				
				if (outStream != null)
					outStream.Write(_pos.Buffer, _pos.CurrentPosition, count);
				_pos.CurrentPosition += count;
			}

			if (_pos.EndOfStream)
			{
                if (currentBound != null && errorHandler != null)
                    errorHandler(ParserError.NoCorrectBoundInMessageEnd, null);
                
                finalPart = true;                
                return;
			}

			string str = Parser.ReadStreamLine(stream);
			finalPart = (str.IndexOf("--") != -1);

            return;
		}

		#endregion

        /// <summary>
        /// ���������� ������������� �������, ������������ �� ������ IMessageConstructor
        /// </summary>
        /// <param name="constructor">��������� IMessageConstructor</param>
        /// <param name="stream">����� ��� ������ ������</param>
        /// <param name="sizeOfInnerBuffer">������ ������, �������� 4096</param>
        /// <param name="upperLevel">������� ������� ��������, ��� ������ ������� ������ ���� true</param>
        /// <param name="currentBound">�������� ��� ���������� ��������, ������ ���� null ��� ������ �������</param>
        /// <returns>finalPart</returns>
		static bool Parse
			(
			IMessageConstructor constructor,
			Stream stream,
			int sizeOfInnerBuffer,
			bool upperLevel,
			string currentBound
			)
		{
			#region Check parameters

			if (constructor == null)
				throw new ArgumentNullException("constructor");
			if (stream == null)
				throw new ArgumentNullException("stream");

			if (upperLevel)
			{
				//������� ����� ��������� �������� ���������� ��� ����, ��� �� �������� ������ ����������� ������� � ���� ������
				_pos = new ParserStreamPos(sizeOfInnerBuffer);
				_pos.Read(stream);
			}

            Action<ParserError, string> errorHandler = (err, comment) => constructor.ErrorHandler(err, comment);

			#endregion

			#region Create variable

			HeaderCollection	headers		= new HeaderCollection();
			bool				finalPart	;
			Encoding			encoding	= Encoding.Default;
			string				nextBound	= null;

			#endregion

			#region Headers parse
			
			StringBuilder strBuffer = new StringBuilder(76*3);	//76 - ����� ������ � ������ �� RFC822
			
			for(;;)
			{
				string str = Parser.ReadStreamLine(stream).Trim();

				if (_pos.EndOfStream)
				{
                    errorHandler(ParserError.NoContainsBodyContent, null);
					
					finalPart = true;
					return finalPart;
				}

				if (str.Length == 0)
				{
					//���� ���������� � ������� ��������� ������
					if (strBuffer.Length > 0)
                        ParserTools.ParseHeader(strBuffer.ToString(), headers, constructor, errorHandler, upperLevel);
					break;
				}

				//��������� ������ � ��� ��� ���������
				bool correctHeader;
                str = ParserTools.ConvertMimeHeader(str, out correctHeader, errorHandler);
				
				if (correctHeader)
				{
					if (strBuffer.Length > 0)
                        ParserTools.ParseHeader(strBuffer.ToString(), headers, constructor, errorHandler, upperLevel);

					strBuffer.Length = 0;
				}

				strBuffer.Append(str);
			}

			#endregion

			#region Before content parse

			nextBound = ParserTools.GetNextBound(headers);
			//���� ����������� ����� ������������
			if (nextBound != null)
			{
				//���������� ������ ����� ������ ���������� � ������� �����
				bool ignoreIt;
                Parser.ReadStreamToBound(null, stream, errorHandler, nextBound, out ignoreIt);
			}

			#endregion

			#region Content parse

			if (nextBound != null)
			{
                while (!Parser.Parse(constructor, stream, sizeOfInnerBuffer, false, nextBound)) ;

				if (_pos.EndOfStream)
					finalPart = true;
				else
                    Parser.ReadStreamToBound(null, stream, errorHandler, currentBound, out finalPart);
			}
			else
			{
				//��������� ����������� ������� ������� �� ���������
				if (currentBound != null)
					currentBound = Environment.NewLine + currentBound;

				IAttachmentConstructor attachment = constructor.GetAttachmentConstructor(headers);
				
				Parser.ReadStreamToBound(
					(attachment != null)? attachment.Stream : null,
					stream,
                    errorHandler,
					currentBound,
					out finalPart
					);

                if (attachment != null)
                {
                    attachment.CompleteConstruction();
                    constructor.CompleteAttachment(attachment);
                }
			}

			#endregion

			#region After content parse

			if (upperLevel)
			{
				constructor.CompleteConstruction();
				_pos.Close();
			}

			#endregion

			return finalPart;
		}

        public static void Parse(IMessageConstructor constructor, Stream stream)
        {
            Parse(constructor, stream, 4096);
        }

        public static void Parse(IMessageConstructor constructor, Stream stream, int sizeOfInnerBuffer)
        {
            Parse(constructor, stream, sizeOfInnerBuffer, true, null);
        }

	}
}
