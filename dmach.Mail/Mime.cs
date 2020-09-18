using System;
using System.Text.RegularExpressions;

namespace InSolve.dmach.Mail
{
	public static class Mime
	{

		/// <summary>
		/// ���������� ������� ���������, ���� �� Transfer � ������ ��� ���������. ������:
		/// =?koi8-r?B?0sXawM3F?= ����������� � ���
		/// [enc]		koi8-r
		/// [transfer]	B
		/// [data]		0sXawM3F
		/// </summary>
		public static Regex RegexMime = new Regex(@"=\?(?<enc>\S+?)\?(?<transfer>[a-z]+)\?(?<data>\S+?)\?=", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		#region Mime headers name

		public const string Content_Type				= "content-type";
		public const string Content_Transfer_Encoding	= "content-transfer-encoding";
		public const string Content_Disposition			= "content-disposition";
		public const string Content_ID					= "content-id";
		public const string Content_Description			= "content-description";

		#endregion
	}

}
