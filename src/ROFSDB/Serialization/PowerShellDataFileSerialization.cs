using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Language;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

namespace TIKSN.ROFSDB.Serialization;

public class PowerShellDataFileSerialization : ISerialization
{
    private static readonly IEnumerable<string> fileExtensions = [".psd1"];
    public IEnumerable<string> FileExtensions => fileExtensions;

    public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where T : class, new()
    {
        using var streamReader = new StreamReader(stream);
        var content = await streamReader.ReadToEndAsync(cancellationToken);
        var psd1Data = ParsePowerShellData(content);
        var json = JsonSerializer.Serialize(psd1Data);
        var model = JsonSerializer.Deserialize<T>(json);
        if (model != null)
        {
            yield return model;
        }
    }

    private static object ParsePowerShellData(string content)
    {
        var ast = Parser.ParseInput(content, out var _, out var errors);
        if (errors.Length > 0)
        {
            throw new FormatException("Could not parse as a PowerShellData file format");
        }
        else
        {
            var data = ast.Find(static a => a is HashtableAst, false);
            if (data != null)
            {
                return data.SafeGetValue();
            }
            else
            {
                throw new FormatException("Could not parse as a PowerShellData file format, No Hashtable found");
            }
        }
    }
}