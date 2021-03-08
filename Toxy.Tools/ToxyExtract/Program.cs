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
        const string Usage = "Usage: ToxyExtract [/encoding ...] [/text] [/metadata] <inputfile>";

        [Flags]
        enum Flags
        {
            None = 0,
            Text = 1,
            Metadata = 2,
        }

        static int Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;

                var encoding = "UTF-8";
                var flags = Flags.None;
                var caught = Flags.None;

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
                if ((i = arguments.IndexOf("/text")) != -1)
                {
                    arguments.RemoveAt(i);
                    flags |= Flags.Text;
                }
                if ((i = arguments.IndexOf("/metadata")) != -1)
                {
                    arguments.RemoveAt(i);
                    flags |= Flags.Metadata;
                }

                if (arguments.Count != 1)
                {
                    Console.WriteLine(Usage);
                    return arguments.Count;
                }

                var filepath = (string)arguments[0];

                ParserContext context = new ParserContext(filepath);
                context.Encoding = Encoding.GetEncoding(encoding);

                ITextParser tparser = null;
                if (flags == Flags.None || (flags & Flags.Text) != 0)
                {
                    try
                    {
                        tparser = ParserFactory.CreateText(context);
                    }
                    catch (Exception e)
                    {
                        if (flags == Flags.None)
                        {
                            flags = Flags.Metadata;
                        }
                        else
                        {
                            caught |= Flags.Text;
                            Console.WriteLine(e);
                        }
                    }
                }

                if ((flags & Flags.Metadata) != 0)
                {
                    try
                    {
                        var parser = ParserFactory.CreateMetadata(context);
                        Console.WriteLine(string.Format("[{0}]", parser.GetType().Name));
                        var metadatas = parser.Parse();
                        foreach (var data in metadatas)
                        {
                            Console.WriteLine(string.Format("{0} = {1}", data.Name.PadRight(23), data.Value.ToString()));
                        }
                        Console.WriteLine();
                    }
                    catch (Exception e)
                    {
                        caught |= Flags.Metadata;
                        Console.WriteLine(e);
                    }
                }

                if (tparser != null)
                {
                    try
                    {
                        if ((flags & Flags.Text) != 0)
                            Console.WriteLine(string.Format("[{0}]", tparser.GetType().Name));
                        var text = tparser.Parse();
                        if (text.EndsWith("\r")) // as seems to happen with .doc files
                        {
                            text = text.Replace('\r', '\n');
                        }
                        Console.Write(text);
                    }
                    catch (Exception e)
                    {
                        caught |= Flags.Text;
                        Console.WriteLine(e);
                    }
                }

                return (int)caught;
            }
            catch (Exception e)
            {
                Console.Write(e);
                return -1;
            }
        }
    }
}
