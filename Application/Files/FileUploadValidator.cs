using System.Text;
using Domain.Resources;

namespace Application.Files;

public sealed class FileUploadValidator : IFileUploadValidator
{
    private static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    public void ValidateMetadata(string fileName, string? contentType, string language)
    {
        if (!SupportedFileFormats.Contains(fileName))
            throw new UnsupportedFileException(ErrorMessages.Get(
                ErrorCode.UnsupportedFileFormat, language, Path.GetExtension(fileName)));

        var mediaType = (contentType ?? string.Empty).Split(';', 2)[0].Trim();
        if (!IsTextMediaType(mediaType))
            throw new UnsupportedFileException(ErrorMessages.Get(ErrorCode.UnsupportedFileContent, language));
    }

    public async Task ValidateContentAsync(Stream content, string language, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[8192];
        var characters = new char[8192];
        var decoder = new UTF8Encoding(false, true).GetDecoder();
        var firstChunk = true;

        try
        {
            int count;
            while ((count = await content.ReadAsync(bytes, cancellationToken)) > 0)
            {
                if (firstChunk && HasBinarySignature(bytes.AsSpan(0, count)))
                    throw new UnsupportedFileException(ErrorMessages.Get(ErrorCode.UnsupportedFileContent, language));
                firstChunk = false;

                var charCount = decoder.GetChars(bytes, 0, count, characters, 0, flush: false);
                if (HasNonTextCharacters(characters.AsSpan(0, charCount)))
                    throw new UnsupportedFileException(ErrorMessages.Get(ErrorCode.UnsupportedFileContent, language));
            }

            var remaining = decoder.GetChars(Array.Empty<byte>(), 0, 0, characters, 0, flush: true);
            if (HasNonTextCharacters(characters.AsSpan(0, remaining)))
                throw new UnsupportedFileException(ErrorMessages.Get(ErrorCode.UnsupportedFileContent, language));
        }
        catch (DecoderFallbackException)
        {
            throw new UnsupportedFileException(ErrorMessages.Get(ErrorCode.UnsupportedFileContent, language));
        }
    }

    private static bool IsTextMediaType(string mediaType) =>
        !mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
        && !mediaType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
        && !mediaType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
        && (string.IsNullOrEmpty(mediaType)
        || mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
        || mediaType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase)
        || mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase)
        || mediaType.Equals("application/xml", StringComparison.OrdinalIgnoreCase)
        || mediaType.Equals("application/csv", StringComparison.OrdinalIgnoreCase)
        || mediaType.Equals("application/x-csv", StringComparison.OrdinalIgnoreCase)
        || mediaType.Equals("application/vnd.ms-excel", StringComparison.OrdinalIgnoreCase)
        || mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)
        || mediaType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase));

    private static bool HasNonTextCharacters(ReadOnlySpan<char> characters)
    {
        foreach (var character in characters)
        {
            if (char.IsControl(character) && character is not ('\t' or '\r' or '\n'))
                return true;
        }
        return false;
    }

    private static bool HasBinarySignature(ReadOnlySpan<byte> bytes) =>
        bytes.StartsWith("GIF87a"u8) || bytes.StartsWith("GIF89a"u8)
        || bytes.StartsWith("%PDF-"u8) || bytes.StartsWith("RIFF"u8)
        || bytes.StartsWith("MZ"u8) || bytes.StartsWith("ID3"u8)
        || bytes.StartsWith("OggS"u8) || bytes.StartsWith(ZipSignature)
        || bytes.StartsWith(PngSignature) || bytes.StartsWith(JpegSignature)
        || bytes.Length >= 8 && bytes[4..8].SequenceEqual("ftyp"u8);
}
