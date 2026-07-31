using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using Serilog.Sinks.OpenSearch.Durable;

namespace Serilog.Sinks.OpenSearch.Tests;

public class OpenSearchPayloadReaderTests
{
    private const string BufferFileKey = nameof(BufferFileKey);
    private readonly string _tempFileFullPathTemplate;

    public OpenSearchPayloadReaderTests()
    {
        _tempFileFullPathTemplate = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")) + "-{0}.json";
    }

    [After(Test)]
    public void Cleanup()
    {
        if (TestContext.Current?.StateBag.TryRemove(BufferFileKey, out var value) == true && value is string bufferFileName)
        {
            System.IO.File.Delete(bufferFileName);
        }
    }

    [Test]
    [Arguments(RollingInterval.Day)]
    [Arguments(RollingInterval.Hour)]
    [Arguments(RollingInterval.Minute)]
    public async Task ReadPayload_ShouldReadSpecifiedTypesOfRollingFile(RollingInterval rollingInterval)
    {
        // Arrange
        var format = rollingInterval.GetFormat();
        var payloadReader = new OpenSearchPayloadReader("testPipelineName",
            "TestTypeName",
            null,
            (_, _) => "TestIndex",
            OpenOpType.Index,
            rollingInterval);
        var lines = new[]
        {
            rollingInterval.ToString()
        };
        var bufferFileName = string.Format(_tempFileFullPathTemplate,
            string.IsNullOrEmpty(format) ? string.Empty : new DateTime(2000, 1, 1).ToString(format));
        TestContext.Current!.StateBag[BufferFileKey] = bufferFileName;
        // Important to use UTF8 with BOM if we are starting from 0 position 
        System.IO.File.WriteAllLines(bufferFileName, lines, new UTF8Encoding(true));

        // Act
        var fileSetPosition = new FileSetPosition(0, bufferFileName);
        var count = 0;
        var payload = payloadReader.ReadPayload(int.MaxValue,
            null,
            ref fileSetPosition,
            ref count,
            bufferFileName);

        // Assert
        // Thus we ensure that file was properly handled by PayloadReader  
        await Assert.That(payload.Count).IsEqualTo(lines.Length * 2);
        await Assert.That(payload[1]).IsEqualTo(lines[0]);
    }

    [Test]
    [Arguments(RollingInterval.Infinite)]
    [Arguments(RollingInterval.Year)]
    [Arguments(RollingInterval.Month)]
    public async Task OpenSearchPayloadReader_CannotUseRollingIntervalLessFrequentThanDay(RollingInterval rollingInterval)
    {
        // Arrange

        // Act
        Action act = () => new OpenSearchPayloadReader("testPipelineName",
            "TestTypeName",
            null,
            (_, _) => "TestIndex",
            OpenOpType.Index,
            rollingInterval);

        // Assert
        await Assert.That(act)
            .Throws<ArgumentException>()
            .WithMessage("Rolling intervals less frequent than RollingInterval.Day are not supported");
    }
}
