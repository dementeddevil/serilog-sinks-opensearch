using Serilog.Sinks.OpenSearch.Tests.Stubs;

namespace Serilog.Sinks.OpenSearch.Tests
{
    public class OpenSearchLogShipperTests : OpenSearchSinkTestsBase
    {
        //To enable tests :
        // * Create public key token for Testproject, make Serilog.Sinks.OpenSearch internals visible to testproject & scope OpenSearchLogShipper to internal.
        // * or make OpenSearchLogShipper (temporarily) public.

        //[Fact]
        //public void OpenSearchLogShipper_TryReadLineShouldReadWithBOM()
        //{
        //    bool withBom = true;

        //    string testLine1 = "test 1";
        //    string testLine2 = "test 2";

        //    using (MemoryStream s = new MemoryStream())
        //    using (StreamWriter sw = new StreamWriter(s, new UTF8Encoding(withBom)))
        //    {
        //        sw.WriteLine(testLine1);
        //        sw.WriteLine(testLine2);
        //        sw.Flush();

        //        string nextLine;
        //        long nextStart = 0;

        //        OpenSearchLogShipper.TryReadLine(s, ref nextStart, out nextLine);

        //        Assert.Equal(Encoding.UTF8.GetByteCount(testLine1) + +Encoding.UTF8.GetByteCount(Environment.NewLine) + 3, nextStart);
        //        Assert.Equal(testLine1, nextLine);

        //        OpenSearchLogShipper.TryReadLine(s, ref nextStart, out nextLine);
        //        Assert.Equal(testLine2, nextLine);
        //    }
        //}

        //[Fact]
        //public void OpenSearchLogShipper_TryReadLineShouldReadWithoutBOM()
        //{
        //    bool withBom = false;

        //    string testLine1 = "test 1";
        //    string testLine2 = "test 2";
        //    Encoding.UTF8.GetBytes(testLine1);

        //    using (MemoryStream s = new MemoryStream())
        //    using (StreamWriter sw = new StreamWriter(s, new UTF8Encoding(withBom)))
        //    {
        //        sw.WriteLine(testLine1);
        //        sw.WriteLine(testLine2);
        //        sw.Flush();

        //        string nextLine;
        //        long nextStart = 0;

        //        OpenSearchLogShipper.TryReadLine(s, ref nextStart, out nextLine);

        //        Assert.Equal(Encoding.UTF8.GetByteCount(testLine1) + Encoding.UTF8.GetByteCount(Environment.NewLine), nextStart);
        //        Assert.Equal(testLine1, nextLine);

        //        OpenSearchLogShipper.TryReadLine(s, ref nextStart, out nextLine);
        //        Assert.Equal(testLine2, nextLine);

        //    }
        //}

        //[Fact]
        //public void OpenSearchLogShipper_CreatePayLoad_ShouldSkipOversizedEvent()
        //{
        //    var selfLogMessages = new StringBuilder();
        //    SelfLog.Enable(new StringWriter(selfLogMessages));
        //    string testLine1 = "test 1";
        //    string testLine2 = "test 2";
        //    string testLine3 = "1234567";
        //    var startPosition = 0;
        //    _options.BatchPostingLimit = 50;
        //    _options.SingleEventSizePostingLimit = 6;

        //    var payLoad = new List<string>();
        //    using (MemoryStream s = new MemoryStream())
        //    using (StreamWriter sw = new StreamWriter(s, new UTF8Encoding(false)))
        //    {
        //        sw.WriteLine(testLine1);
        //        sw.WriteLine(testLine2);
        //        sw.WriteLine(testLine3);
        //        sw.Flush();

        //        (new OpenSearchLogShipper(_options)).CreatePayLoad(s, payLoad, "TestIndex", startPosition, "D:\\TestFile");

        //        var expectedNumberOfLines = 2;
        //        Assert.Equal(expectedNumberOfLines*2, payLoad.Count());
        //        Assert.Contains("Skip sending to OpenSearch", selfLogMessages.ToString());
        //    }
        //}
    }
}
