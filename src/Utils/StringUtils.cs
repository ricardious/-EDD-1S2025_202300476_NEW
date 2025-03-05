using System;

namespace AutoGestPro.src.Utils
{
    /// <summary>
    /// Provides utility methods for string operations using unsafe code.
    /// </summary>
    /// <remarks>
    /// This class contains methods for low-level string manipulation using pointers.
    /// Use with caution as unsafe code can lead to memory issues if not handled properly.
    /// </remarks>
    public static unsafe class StringUtils
    {
        /// <summary>
        /// Copies a string to a fixed-length character buffer with null termination.
        /// </summary>
        /// <param name="destination">Pointer to the destination character buffer.</param>
        /// <param name="source">Source string to copy from.</param>
        /// <param name="maxLength">Maximum length of the destination buffer, including null terminator.</param>
        /// <remarks>
        /// This method ensures the destination buffer is null-terminated to prevent buffer overflows.
        /// If the source string is longer than maxLength-1, it will be truncated.
        /// </remarks>
        /// <example>
        /// <code>
        /// fixed (char* buffer = new char[bufferSize])
        /// {
        ///     StringUtils.CopyToFixedBuffer(buffer, "example", bufferSize);
        /// }
        /// </code>
        /// </example>        
        public static void CopyToFixedBuffer(char* destination, string source, int maxLength)
        {
            if (destination == null || source == null)
                return;

            int i;
            for (i = 0; i < source.Length && i < maxLength - 1; i++)
            {
                destination[i] = source[i];
            }

            destination[i] = '\0'; // Null-terminate to avoid buffer overflows
        }
    }
}
