using System.Collections.Generic;
using System.IO;

namespace BlangParser
{
    /// <summary>
    /// BlangFile class
    /// </summary>
    public class BlangFile
    {
        /// <summary>
        /// Store the string amount as bytes to avoid easily write them as big-endian
        /// Too lazy to reimplement BinaryWriter for jut a big-endian value
        /// </summary>
        public byte[] StringAmountBytes;

        /// <summary>
        /// The strings in this file
        /// </summary>
        public List<BlangString> Strings;

        /// <summary>
        /// Parses the given Blang file into a BlangFile object
        /// </summary>
        /// <param name="path">path to the Blang file</param>
        /// <returns>parsed Blang file in a BlangFile object</returns>
        public static BlangFile Parse(string path)
        {
            var blangFile = new BlangFile();

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    var blangStrings = new List<BlangString>();

                    // Read the amount of strings (big-endian 32 bit integer)
                    fileStream.Seek(0x0, SeekOrigin.Begin);
                    byte[] stringAmountBytes = binaryReader.ReadBytes(4);
                    int stringAmount = (stringAmountBytes[0] << 24) | (stringAmountBytes[1] << 16) | (stringAmountBytes[2] << 8) | stringAmountBytes[3];

                    // Parse each string
                    for (int i = 0; i < stringAmount; i++)
                    {
                        // Read string hash
                        int hash = binaryReader.ReadInt32();

                        // Read string identifier
                        int identifierBytes = binaryReader.ReadInt32();
                        string identifier = System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(identifierBytes));

                        // Read string
                        int textBytes = binaryReader.ReadInt32();
                        string text = System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(textBytes));

                        // Read unknown data
                        int unknownBytes = binaryReader.ReadInt32();
                        string unknown = System.Text.Encoding.UTF8.GetString(binaryReader.ReadBytes(unknownBytes));

                        blangStrings.Add(new BlangString()
                        {
                            Hash = hash,
                            Identifier = identifier,
                            Text = text,
                            Unknown = unknown
                        });
                    }

                    blangFile.StringAmountBytes = stringAmountBytes;
                    blangFile.Strings = blangStrings;
                }
            }

            return blangFile;
        }

        /// <summary>
        /// Writes the current BlangFile object to the specified path
        /// </summary>
        /// <param name="path"></param>
        public void WriteTo(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (var binaryWriter = new BinaryWriter(fileStream))
                {
                    // Write string amount
                    binaryWriter.Write(StringAmountBytes);

                    // Write each string
                    foreach (var blangString in Strings)
                    {
                        // Write hash
                        binaryWriter.Write(blangString.Hash);

                        // Write identifier
                        var identifierBytes = System.Text.Encoding.UTF8.GetBytes(blangString.Identifier);
                        binaryWriter.Write(identifierBytes.Length);
                        binaryWriter.Write(identifierBytes);

                        // Write text
                        var textBytes = System.Text.Encoding.UTF8.GetBytes(blangString.Text);
                        binaryWriter.Write(textBytes.Length);
                        binaryWriter.Write(textBytes);

                        // Write unknown data
                        var unknownBytes = System.Text.Encoding.UTF8.GetBytes(blangString.Unknown);
                        binaryWriter.Write(unknownBytes.Length);
                        binaryWriter.Write(unknownBytes);
                    }
                }
            }
        }
    }
}
