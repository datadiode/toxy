using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toxy.Test
{
    [TestFixture]
    public class DateTimeParserTest
    {
        [Test]
        public void TestParseDatetimeWithTimezone()
        {
            Assert.AreEqual(DateTime.Parse("2008-10-25 03:09:06 +8"), DateTimeParser.Parse("24-oct-08 21:09:06 CEST"));
            Assert.AreEqual(DateTime.Parse("2012-04-20 16:10:00 +8"), DateTimeParser.Parse("2012 -04-20 10:10:00+0200"));
            Assert.AreEqual(DateTime.Parse("2014-12-12 12:13:30 +8"), DateTimeParser.Parse("Fri, 12 Dec 2014 12:13:30 +0800 (CST)"));

        }

        [Test]
        public void TestParseDatetimeWithoutTimezone()
        {
            Assert.AreEqual(new DateTime(2014, 12, 12, 12, 13, 30), DateTimeParser.Parse("Fri, 12 Dec 2014 12:13:30"));
        }
    }
}
