using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Contracts.Files;
using Domain.Files;
using Domain.Resources;
using Microsoft.VisualBasic.FileIO;

namespace Application.Files;

public sealed class FileAnalyzer : IFileAnalyzer
{
    private const int PreviewRows = 200;
    private const int PreviewCharacters = 20_000;
    private const int PreviewJsonCharacters = 100_000;

    public async Task<FileResultResponse> AnalyzeAsync(Guid id, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".csv" or ".tsv" => await AnalyzeDelimitedAsync(id, content, extension, watch, cancellationToken),
            ".json" => await AnalyzeJsonAsync(id, content, watch, cancellationToken),
            ".txt" or ".log" or ".xml" or ".md" => await AnalyzeTextAsync(id, content, watch, cancellationToken),
            _ => throw new FileAnalysisException(FileFailureCode.UnsupportedFormat)
        };
    }

    private static async Task<FileResultResponse> AnalyzeDelimitedAsync(
        Guid id, Stream content, string extension, Stopwatch watch, CancellationToken cancellationToken)
    {
        string? firstLine;
        using (var reader = new StreamReader(content, Encoding.UTF8, true, 4096, leaveOpen: true))
            firstLine = await reader.ReadLineAsync(cancellationToken);
        content.Position = 0;

        if (string.IsNullOrWhiteSpace(firstLine))
            throw new FileAnalysisException(FileFailureCode.InvalidContent);

        var delimiter = extension == ".tsv" ? "\t" : Count(firstLine, ';') > Count(firstLine, ',') ? ";" : ",";
        using var parser = new TextFieldParser(content, Encoding.UTF8, detectEncoding: true)
        {
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = true
        };
        parser.SetDelimiters(delimiter);

        try
        {
            var columns = parser.ReadFields();
            if (columns is null || columns.Length == 0)
                throw new FileAnalysisException(FileFailureCode.InvalidContent);

            var rows = new List<IReadOnlyList<string?>>();
            var count = 0;
            while (!parser.EndOfData)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fields = parser.ReadFields();
                if (fields is null)
                    continue;
                count++;
                if (rows.Count < PreviewRows)
                    rows.Add(fields);
            }

            return new FileResultResponse(id,
                new Dictionary<string, object?>
                {
                    ["rows"] = count,
                    ["columns"] = columns.Length,
                    ["encoding"] = "UTF-8",
                    ["durationMs"] = watch.ElapsedMilliseconds
                },
                new FilePreviewResponse("table", columns, rows, null, null), null);
        }
        catch (MalformedLineException)
        {
            throw new FileAnalysisException(FileFailureCode.InvalidContent);
        }
    }

    private static async Task<FileResultResponse> AnalyzeJsonAsync(
        Guid id, Stream content, Stopwatch watch, CancellationToken cancellationToken)
    {
        try
        {
            using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
            var root = document.RootElement;
            var rowCount = root.ValueKind == JsonValueKind.Array ? root.GetArrayLength() : 1;
            var raw = root.GetRawText();
            FilePreviewResponse preview;

            if (raw.Length <= PreviewJsonCharacters)
                preview = new FilePreviewResponse("json", null, null, null, root.Clone());
            else if (root.ValueKind == JsonValueKind.Array)
            {
                var selected = new List<JsonElement>();
                var size = 0;
                foreach (var element in root.EnumerateArray())
                {
                    var itemLength = element.GetRawText().Length;
                    if (selected.Count >= PreviewRows || size + itemLength > PreviewJsonCharacters)
                        break;
                    selected.Add(element.Clone());
                    size += itemLength;
                }
                preview = selected.Count > 0
                    ? new FilePreviewResponse("json", null, null, null, JsonSerializer.SerializeToElement(selected))
                    : new FilePreviewResponse("text", null, null, raw[..PreviewCharacters], null);
            }
            else
                preview = new FilePreviewResponse("text", null, null, raw[..PreviewCharacters], null);

            return new FileResultResponse(id,
                new Dictionary<string, object?>
                {
                    ["rows"] = rowCount,
                    ["encoding"] = "UTF-8",
                    ["durationMs"] = watch.ElapsedMilliseconds
                }, preview, null);
        }
        catch (JsonException)
        {
            content.Position = 0;
            using var reader = new StreamReader(content, Encoding.UTF8, true, 4096, leaveOpen: true);
            var text = await reader.ReadToEndAsync(cancellationToken);
            return new FileResultResponse(id,
                new Dictionary<string, object?> { ["durationMs"] = watch.ElapsedMilliseconds },
                new FilePreviewResponse("text", null, null, text[..Math.Min(text.Length, PreviewCharacters)], null),
                [nameof(ErrorCode.InvalidJsonPreviewWarning)]);
        }
    }

    private static async Task<FileResultResponse> AnalyzeTextAsync(
        Guid id, Stream content, Stopwatch watch, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(content, Encoding.UTF8, true, 4096, leaveOpen: true);
        var preview = new StringBuilder(PreviewCharacters);
        var lines = 0;
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            lines++;
            if (preview.Length >= PreviewCharacters)
                continue;
            if (preview.Length > 0)
                preview.AppendLine();
            preview.Append(line.AsSpan(0, Math.Min(line.Length, PreviewCharacters - preview.Length)));
        }

        return new FileResultResponse(id,
            new Dictionary<string, object?>
            {
                ["lines"] = lines,
                ["encoding"] = "UTF-8",
                ["durationMs"] = watch.ElapsedMilliseconds
            },
            new FilePreviewResponse("text", null, null, preview.ToString(), null), null);
    }

    private static int Count(string value, char character) => value.Count(c => c == character);
}
