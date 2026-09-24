namespace Contracts.Imports;

public sealed record CreateImportJobRequest(string SourceSystem, string FileName, string StoredFileKey, string Format);
