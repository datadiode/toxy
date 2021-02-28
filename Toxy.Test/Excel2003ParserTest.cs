using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Globalization;
using NUnit.Framework;

namespace Toxy.Test
{
    /// <summary>
    /// Summary description for ExcelParserTest
    /// </summary>
    [TestFixture]
    public class Excel2003ParserTest:ExcelParserBaseTest
    {
        public Excel2003ParserTest()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }
        [Test]
        public void TestExtractFooter()
        {
            BaseTestExtractSheetFooter("45538_classic_Footer.xls");
        }
        [Test]
        public void TestExtractHeader()
        {
            BaseTestExtractSheetHeader("45538_classic_Header.xls");
        }
        [Test]
        public void TestFillBlankCells()
        {
            base.BaseTestFillBlankCells("Employee.xls");
        }

        [Test]
        public void TestExcelWithFormats()
        {
            BaseTestExcelFormatedString("Formatting.xls");
        }

        [Test]
        public void TestExcelParserSimple()
        {
            base.BaseTestExcelContent("Employee.xls");
        }
        [Test]
        public void TestExcelWithComments()
        {
            base.BaseTestExcelComment("comments.xls");
        }
    }
}
