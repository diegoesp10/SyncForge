using Domain.Imports;
using Domain.Resources;

namespace Application.Imports;

internal static class ImportJobLookup
{
    public static T Require<T>(T? value, string name, string language) where T : class =>
        value ?? throw new ArgumentNullException(name, ErrorMessages.Get(ErrorCode.RequiredValue, language, name));

    public static async Task<ImportJob> GetRequiredAsync(
        IImportJobRepository repository, int id, string language, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException(ErrorMessages.Get(ErrorCode.ImportJobNotFound, language, id));
}
