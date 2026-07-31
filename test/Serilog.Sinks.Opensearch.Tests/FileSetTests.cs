using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Serilog.Sinks.OpenSearch.Durable;

namespace Serilog.Sinks.OpenSearch.Tests;

public class FileSetTests
{
    private const string BufferFilesKey = nameof(BufferFilesKey);
    private readonly string _fileNameBase;
    private readonly string _tempFileFullPathTemplate;

    public FileSetTests()
    {
        _fileNameBase = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        _tempFileFullPathTemplate = _fileNameBase + "-{0}.json";
    }

    [After(Test)]
    public void Cleanup()
    {
        if (TestContext.Current?.StateBag.TryRemove(BufferFilesKey, out var value) != true || value is not string[] bufferFileNames)
        {
            return;
        }

        foreach (var bufferFileName in bufferFileNames)
        {
            System.IO.File.Delete(bufferFileName);
        }
    }

    [Test]
    [Arguments(RollingInterval.Day)]
    [Arguments(RollingInterval.Hour)]
    [Arguments(RollingInterval.Infinite)]
    [Arguments(RollingInterval.Minute)]
    [Arguments(RollingInterval.Month)]
    [Arguments(RollingInterval.Year)]
    // Ensures that from all presented files FileSet gets only files with specified rolling interval and not the others.  
    public async Task GetBufferFiles_ReturnsOnlySpecifiedTypeOfRollingFile(RollingInterval rollingInterval)
    {
        // Arrange
        var format = rollingInterval.GetFormat();
        var bufferFileNames = GenerateFilesUsingFormat(format);
        TestContext.Current!.StateBag[BufferFilesKey] = bufferFileNames.Values.ToArray();
        var fileSet = new FileSet(_fileNameBase, rollingInterval);
        var bufferFileForInterval = bufferFileNames[rollingInterval];

        // Act
        var bufferFiles = fileSet.GetBufferFiles();

        // Assert
        await Assert.That(bufferFiles).IsEquivalentTo(new[] { bufferFileForInterval });
    }

    /// <summary>
    /// Generates buffer files for all RollingIntervals and returns dictionary of {rollingInterval, fileName} pairs.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    private Dictionary<RollingInterval, string> GenerateFilesUsingFormat(string format)
    {
        var result = new Dictionary<RollingInterval, string>();
        foreach (var rollingInterval in Enum.GetValues(typeof(RollingInterval)))
        {
            var bufferFileName = string.Format(_tempFileFullPathTemplate,
                string.IsNullOrEmpty(format) ? string.Empty : new DateTime(2000, 1, 1).ToString(format));
            var lines = new[] {rollingInterval.ToString()};
            // Important to use UTF8 with BOM if we are starting from 0 position 
            System.IO.File.WriteAllLines(bufferFileName, lines, new UTF8Encoding(true));
            result.Add((RollingInterval) rollingInterval, bufferFileName);
        }

        return result;
    }
}
