using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Toxy.Parsers
{
    public class EMLTextParser:PlainTextParser
    {
        public EMLTextParser(ParserContext context):base(context)
        {
            this.Context = context;
        }

        public override string Parse()
        {
            if (!File.Exists(Context.Path))
                throw new FileNotFoundException("File " + Context.Path + " is not found");

            StringBuilder sb = new StringBuilder();
            using (FileStream stream = File.OpenRead(Context.Path))
            {
                var message = MsgReader.Mime.Message.Load(stream);
                sb.Append("[From] ");
                sb.AppendLine(message.Headers.From.Raw);
                sb.Append("[To] ");
                sb.AppendLine(String.Join(";", message.Headers.To));
                if (message.Headers.Cc.Count != 0)
                {
                    sb.Append("[CC] ");
                    sb.AppendLine(String.Join(";", message.Headers.Cc));
                }
                sb.Append("[Date] ");
                sb.AppendLine(message.Headers.Date);
                sb.Append("[Subject] ");
                sb.AppendLine(message.Headers.Subject);
                if (message.TextBody != null)
                {
                    sb.AppendLine();
                    sb.Append(message.TextBody.GetBodyAsText());
                }
                if (message.HtmlBody != null)
                {
                    sb.AppendLine();
                    sb.Append(message.HtmlBody.GetBodyAsText());
                }
            }
            return sb.ToString();
        }
    }
}
