// ToxyExtract - Command line tool to extract text from various kinds of files
// SPDX-License-Identifier: AGPL-3.0-only

using System;
using System.Collections;
using System.IO;
using System.Text;
using Toxy;

namespace ToxyExtract
{
    class Program
    {
        const string Usage = "Usage: ToxyExtract [/encoding ...] [/extension ...] <inputfile>";

        static string ReadDocument(string filepath, string encoding, string extension)
        {
            ParserContext context = new ParserContext(filepath);
            context.Encoding = Encoding.GetEncoding(encoding);
            var tparser = ParserFactory.CreateText(context);
            return tparser.Parse();
        }

        static int Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;

                string encoding = "UTF-8";
                string extension = null;

                var arguments = new ArrayList(args);
                int i;
                if ((i = arguments.IndexOf("/encoding")) != -1)
                {
                    arguments.RemoveAt(i);
                    if (i < arguments.Count)
                    {
                        encoding = (string)arguments[i];
                        arguments.RemoveAt(i);
                    }
                }
                if ((i = arguments.IndexOf("/extension")) != -1)
                {
                    arguments.RemoveAt(i);
                    if (i < arguments.Count)
                    {
                        extension = (string)arguments[i];
                        arguments.RemoveAt(i);
                    }
                }

                if (arguments.Count != 1)
                {
                    Console.WriteLine(Usage);
                    return arguments.Count;
                }

                string filepath = (string)arguments[0];

                if (extension == null)
                {
                    FileInfo fi = new FileInfo(filepath);
                    extension = fi.Extension.ToLower();
                }

                var text = ReadDocument(filepath, encoding, extension);
                if (text.EndsWith("\r")) // as seems to happen with .doc files
                {
                    text = text.Replace('\r', '\n');
                }

                Console.Write(text);
                return 0;
            }
            catch (Exception e)
            {
                Console.Write(e);
                return 9;
            }
        }
    }
}
