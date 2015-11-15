using kOS.Safe.Compilation;
using kOS.Safe.Exceptions;
using System;
using System.Linq;
using System.Text;

namespace kOS.Safe.Persistence
{
    public static class PersistenceUtilities
    {
        /// <summary>
        /// Given the first few bytes of content, decide what the FileCategory
        /// should be, based on what's in the Content.<br/>
        /// This should be called before deciding how to set the content.
        /// </summary>
        /// <param name="firstBytes">At least the first four bytes of the file read in binary form - can be longer if you wish</param>
        /// <returns>The type that should be used to store this file.</returns>
        public static FileCategory IdentifyCategory(byte[] firstBytes)
        {
            var firstFour = new Byte[4];
            int atMostFour = Math.Min(4, firstBytes.Length);
            Array.Copy(firstBytes, 0, firstFour, 0, atMostFour);
            var returnCat = (atMostFour < 4) ? FileCategory.TOOSHORT : FileCategory.OTHER; // default if none of the conditions pass

            if (firstFour.SequenceEqual(CompiledObject.MagicId))
            {
                returnCat = FileCategory.KSM;
            }
            else
            {
                bool isAscii = firstFour.All(b => b == (byte)'\n' || b == (byte)'\t' || b == (byte)'\r' || (b >= (byte)32 && b <= (byte)127));
                if (isAscii)
                    returnCat = FileCategory.ASCII;
            }
            return returnCat;

            // There is not currently an explicit test for KERBOSRIPT versus other types of ASCII.
            // At current, any time you want to test for is Kerboscript, make sure to test for is ASCII too,
            // since nothing causes a file to become type kerboscript yet.
        }

        public static byte[] DecodeLine(string input)
        {
            StringBuilder output = new StringBuilder();
            for (int inputPos = 0; inputPos < input.Length; ++inputPos)
            {
                char ch = input[inputPos];
                if (ch == '&')
                {
                    // The reason for catching all these exception cases is because people might use this
                    // new code to try to read files that the old buggy code wrote out incorrectly:
                    if (input[inputPos + 1] != '#')
                        throw new KOSPersistenceException("Improperly encoded saved file contains '&' without '#'");
                    int semicolonPos = input.IndexOf(';', inputPos);
                    if (semicolonPos < 0)
                        throw new KOSPersistenceException("Improperly encoded saved file contains '&' without closing ';'");
                    int charOrdinal;
                    if (!int.TryParse(input.Substring(inputPos + 2, (semicolonPos - (inputPos + 2))), out charOrdinal))
                        throw new KOSPersistenceException("Improperly encoded saved file contains non-digits between the '&#' and the ';'");
                    output.Append((char)charOrdinal);
                    inputPos = semicolonPos; // skip to the end of the encoding section, as if everything between '&' and ';' was one char of input.
                }
                else
                {
                    output.Append(ch);
                }
            }
            return Encoding.UTF8.GetBytes(output.ToString());
        }

        public static string EncodeLine(byte[] input)
        {
            StringBuilder output = new StringBuilder();
            foreach (char ch in input)
            {
                if (CharNeedsEncoding(ch))
                    output.Append("&#" + (uint)ch + ";"); // Casting to uint will get the Unicode number of the character
                else
                    output.Append(ch);
            }
            return output.ToString();
        }

        /// <summary>
        /// Returns true if the character has to be encoded and cannot be dumped into a persistence file as-is.
        /// </summary>
        /// <param name="character">character to test</param>
        /// <returns>true if the character needs protective encoding</returns>
        public static bool CharNeedsEncoding(char character)
        {
            return !(Char.IsLetterOrDigit(character) || WHITELISTED_SYMBOLS.Contains(character));
        }

        // Note: deliberately missing from the whitelist are:
        //  '&' - because it's the marker used to start an encoding.
        //  '/' - because while one is safe, two of them consecutively would start a comment,
        //          and it's just easier to encode all of them than write the special logic that tracks
        //          the context of prev char or next char.
        private const string WHITELISTED_SYMBOLS = "~`!@#$%^*()_-+=[]|:;\"'<>,.?";
    }
}