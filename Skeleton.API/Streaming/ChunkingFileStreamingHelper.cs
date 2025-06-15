using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace Skeleton.API.Streaming
{
    /// <summary>
    /// Based on https://stackoverflow.com/a/47031532
    /// </summary>
    public static class ChunkingFileStreamingHelper
    {
        private static readonly FormOptions _defaultFormOptions = new();

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="targetStream"></param>
        /// <returns>Dictionary of FormData keys and values and the target stream is updated with the file body</returns>
        /// <exception cref="ApplicationException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public static async Task<FormCollection> StreamFile(this HttpRequest request, Stream targetStream)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
            {
                throw new ApplicationException($"Expected a multipart request, but got {request.ContentType}");
            }

            KeyValueAccumulator formAccumulator = new();

            string boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), _defaultFormOptions.MultipartBoundaryLengthLimit);
            MultipartReader reader = new(boundary, request.Body);

            MultipartSection? section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue? contentDisposition))
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        string filename = string.Join("", (contentDisposition.FileName.Value ?? contentDisposition.FileNameStar.Value ?? "").Split(Path.GetInvalidFileNameChars()));
                        formAccumulator.Append("FileName", filename);

                        await section.Body.CopyToAsync(targetStream);
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        // Content-Disposition: form-data; name="key"
                        // value

                        // Do not limit the key name length here because the multipart headers length limit is already in effect.
                        StringSegment key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        if (key.HasValue)
                        {
                            Encoding encoding = GetEncoding(section);

                            using StreamReader streamReader = new(section.Body, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);

                            // The value length limit is enforced by MultipartBodyLengthLimit
                            string value = await streamReader.ReadToEndAsync();

                            if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = "";
                            }

                            formAccumulator.Append(key.Value, value);

                            if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }
            
            if (targetStream.Length == 0)
            {
                throw new InvalidDataException("File not found");
            }

            return new FormCollection(formAccumulator.GetResults());
        }


        private static Encoding GetEncoding(MultipartSection section)
        {
            bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out MediaTypeHeaderValue? mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in most cases.
#pragma warning disable SYSLIB0001 // Type or member is obsolete
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType?.Encoding))
            {
                return Encoding.UTF8;
            }
#pragma warning restore SYSLIB0001 // Type or member is obsolete
            return mediaType?.Encoding ?? Encoding.UTF8;
        }
    }

    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        public static bool IsMultipartContentType(string? contentType)
        {
            return !string.IsNullOrEmpty(contentType) && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }
    }
}
