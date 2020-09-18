using System;

namespace InSolve.dmach.Mail
{
	public interface IMessageConstructor
	{
		void AddHeader (string name, string value);
		IAttachmentConstructor GetAttachmentConstructor (HeaderCollection headers);
        void CompleteAttachment(IAttachmentConstructor att);
        void CompleteConstruction();
        
        // ����� ���� ����� ����������� ��� Action<ParserError, string>
        void ErrorHandler(ParserError err, string comment);
	}
}