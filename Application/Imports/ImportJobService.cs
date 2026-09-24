using Application.Imports.Commands;
using Application.Imports.Queries;
using Contracts.Imports;
using Domain.Imports.Enums;
using Domain.Resources;

namespace Application.Imports;

public sealed class ImportJobService(
    CreateImportJobHandler create,
    GetImportJobHandler get,
    ListImportJobsHandler list,
    StartImportJobHandler start,
    CompleteImportJobHandler complete,
    FailImportJobHandler fail,
    RetryImportJobHandler retry) : IImportJobService
{
    public Task<IReadOnlyList<ImportJobResponse>> ListAsync(int skip, int take, string language, CancellationToken cancellationToken = default) =>
        list.HandleAsync(new ListImportJobsQuery(skip, take), language, cancellationToken);

    public async Task<ImportJobResponse> GetAsync(int id, string language, CancellationToken cancellationToken = default) =>
        await get.HandleAsync(new GetImportJobQuery(id), language, cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.ImportJobNotFound, language, id));

    public Task<ImportJobResponse> CreateAsync(CreateImportJobRequest request, string language, CancellationToken cancellationToken = default)
    {
        ImportJobLookup.Require(request, nameof(request), language);
        if (!Enum.TryParse<ImportFormat>(request.Format, true, out var format) || !Enum.IsDefined(format))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.InvalidFormat, language), nameof(request));

        return create.HandleAsync(new CreateImportJobCommand(
            request.SourceSystem, request.FileName, request.StoredFileKey, format), language, cancellationToken);
    }

    public Task<ImportJobResponse> StartAsync(int id, string language, CancellationToken cancellationToken = default) =>
        start.HandleAsync(new StartImportJobCommand(id), language, cancellationToken);

    public Task<ImportJobResponse> CompleteAsync(int id, string language, CancellationToken cancellationToken = default) =>
        complete.HandleAsync(new CompleteImportJobCommand(id), language, cancellationToken);

    public Task<ImportJobResponse> FailAsync(int id, FailImportJobRequest request, string language, CancellationToken cancellationToken = default)
    {
        ImportJobLookup.Require(request, nameof(request), language);
        if (!Enum.TryParse<ImportFailureCode>(request.FailureCode, true, out var failureCode) || !Enum.IsDefined(failureCode))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.InvalidFailureCode, language), nameof(request));

        return fail.HandleAsync(new FailImportJobCommand(id, failureCode), language, cancellationToken);
    }

    public Task<ImportJobResponse> RetryAsync(int id, string language, CancellationToken cancellationToken = default) =>
        retry.HandleAsync(new RetryImportJobCommand(id), language, cancellationToken);
}
