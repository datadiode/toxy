using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toxy.Test
{
    [TestFixture]
    public class EmlEmailParserTest
    {

        [Test]
        public void ReadEmlTest()
        {
            string path = TestDataSample.GetEmailPath("test.eml");
            ParserContext context = new ParserContext(path);
            IEmailParser parser = ParserFactory.CreateEmail(context);
            ToxyEmail email = parser.Parse();
            Assert.IsNotNull(email.From);
            Assert.IsNotEmpty(email.From);
            Assert.AreEqual(1, email.To.Count);
            Assert.AreEqual(email.From, "\u62C9\u52FE\u7F51 <service@email.lagou.com>");
            Assert.AreEqual(email.To[0], "tonyqus@163.com");

            Assert.AreEqual(email.Subject, "\u4E0A\u6D77\u5206\u4F17\u5FB7\u5CF0\u5E7F\u544A\u4F20\u64AD\u6709\u9650\u516C\u53F8-\u9AD8\u7EA7.NET\u8F6F\u4EF6\u5DE5\u7A0B\u5E08\u62DB\u8058\u53CD\u9988\u901A\u77E5");
            Assert.IsTrue(email.HtmlBody.StartsWith("<html>\r\n"));
            Assert.IsNull(email.TextBody);

        }
    }
}
