using System.Text.Json;
using Domain.Files;

namespace Application.Files;

public sealed class FileProcessingService(IFileRepository repository, IFileStorage storage, IFileAnalyzer analyzer)
{
    public async Task<IReadOnlyList<Guid>> RecoverQueuedAsync(CancellationToken cancellationToken = default)
    {
        var files = await repository.ListQueuedAsync(cancellationToken);
        foreach (var file in files)
            file.RecoverInterrupted();
        await repository.SaveChangesAsync(cancellationToken);
        return files.Select(file => file.Id).ToArray();
    }

    public async Task ProcessAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var file = await repository.GetByIdAsync(id, cancellationToken);
        if (file is null)
            return;

        if (file.Status == FileStatus.Processing)
        {
            file.RecoverInterrupted();
            await repository.SaveChangesAsync(cancellationToken);
        }
        if (file.Status != FileStatus.Pending)
            return;

        file.Start("en");
        await repository.SaveChangesAsync(cancellationToken);

        try
        {
            await using var content = await storage.OpenReadAsync(id, cancellationToken);
            var result = await analyzer.AnalyzeAsync(id, file.FileName, content, cancellationToken);
            file.Complete(JsonSerializer.Serialize(result), "en");
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (FileAnalysisException ex)
        {
            file.Fail(ex.Code, "en");
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (FileNotFoundException)
        {
            file.Fail(FileFailureCode.MissingContent, "en");
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            file.Fail(FileFailureCode.ProcessingFailed, "en");
            await repository.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}
