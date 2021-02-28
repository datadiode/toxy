using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Toxy.Parsers
{
    public class EMLEmailParser:IEmailParser
    {
        public EMLEmailParser(ParserContext context)
        {
            this.Context = context;
        }

        public ToxyEmail Parse()
        {
            if (!File.Exists(Context.Path))
                throw new FileNotFoundException("File " + Context.Path + " is not found");

            ToxyEmail email = new ToxyEmail();
            using (FileStream stream = File.OpenRead(Context.Path))
            {
                var message = MsgReader.Mime.Message.Load(stream);
                email.From = message.Headers.From.Raw;
                email.To = new List<string>(String.Join(";", message.Headers.To).Split(';'));
                if (message.Headers.Cc.Count != 0)
                    email.Cc = new List<string>(String.Join(";", message.Headers.Cc).Split(';'));
                if (message.TextBody != null)
                    email.TextBody = message.TextBody.GetBodyAsText();
                if (message.HtmlBody != null)
                    email.HtmlBody = message.HtmlBody.GetBodyAsText();
                email.Subject = message.Headers.Subject;
                email.ArrivalTime = message.Headers.DateSent; // well, not quite correct
            }

            return email;
        }

        public ParserContext Context
        {
            get;
            set;
        }
    }
}
