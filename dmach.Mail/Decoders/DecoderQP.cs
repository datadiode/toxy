using System;
using System.IO;
using System.Collections;

namespace InSolve.dmach.Mail.Decoders
{
	/// <summary>
	/// ������� ��� Quoted-Printable (?)
	/// </summary>
	public class DecoderQP : 
		DecoderBase
	{
		#region Fields

		private Queue	_queue;
        /// <summary>
        /// ����������, ��������� �� ������� ������� ������ ����� =xx ��� ���.
        /// </summary>
		private bool	present;

		#endregion
		
		#region Constructors

		public DecoderQP()
			:base()
		{
			_queue		= new Queue(4);
			present		= false;
		}

		#endregion

		#region DecoderBase Members

		public override int Add(byte[] b, int start, int length)
		{
			lock(StreamSyncRoot)
			{
				int r = 0;
				for(int i = 0; i < length; i++)
				{
					if (present)
					{
						_queue.Enqueue(b[start+i]);
					
						//���� ��� ������� � ������� ��� ����.
						if (_queue.Count == 2)
						{
							byte b1 = (byte)_queue.Dequeue();
							byte b2 = (byte)_queue.Dequeue();
                            if (b1 == (byte)'\n' || b2 == (byte)'\n')
                            {
                                present = false;
                                continue;
                            }
                            else
                            {
                                InnerBuffer.WriteByte(HexToByte(b1, b2));
                                present = false;
                                r++;
                            }
						}
					}
					else
					{
						if ( b[start+i] == (byte)'=' )
						{
							present = true;
							continue;
						}
						else
						{
							InnerBuffer.WriteByte(b[start+i]);
						}
					}
					//������� � ��������
				}
				return r;
			}
		}

		#endregion

		#region Tools Methods

		/// <summary>
		/// ���������� byte �� 2-� ������������ 16-������ ���������.
		/// </summary>
		/// <param name="h1"></param>
		/// <param name="h2"></param>
		/// <returns></returns>
		protected static byte HexToByte(byte h1, byte h2)
		{
			unchecked
			{
				return (byte)((Hex(h1)<<4) | Hex(h2));
			}
		}
		/// <summary>
		/// ������������� ����� ��������� ����� � �������� ���� �� ����� � 16-������
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		protected static byte Hex(byte b)
		{
			unchecked
			{
				//�������� ������ ������� � �������� (�� 'a' �� 'f' ������������)
				if (b > 96 && b < 103)
					b -= 32;

				b -= 48;
				if ( b < 10 )
					return b;
				b -= 7;
				if ( b < 16 )
					return b;
				else
					return 0;
			}
		}

		#endregion
	}
}
