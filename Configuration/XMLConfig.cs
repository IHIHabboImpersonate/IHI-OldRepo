using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace IHI.Server.Configuration
{
    public class XmlConfig
    {
        private string fPath;
        private XmlDocument fDocument;
        private readonly bool fCreated; // Set to true when EnsureFile had to create the file.

        /// <summary>
        /// Loads an Xml config file into memory.
        /// </summary>
        /// <param name="Path">The path to the Xml config file.</param>
        public XmlConfig(string Path)
        {
            if (!EnsureFile(new FileInfo(Path), out this.fCreated))
            {
                Core.GetStandardOut().PrintError("File '" + Path + "' does not exist and couldn't be created automatically! (XmlConfig)");
            }

            this.fDocument = new XmlDocument();

            try
            {
                this.fDocument.Load(Path);
            }
            catch (XmlException)
            {

                this.fDocument.LoadXml(@"<?xml version='1.0' encoding='ISO-8859-1'?>
<config>
</config>");
                this.fDocument.Save(Path);
            }

            this.fPath = Path;
        }

        /// <summary>
        /// Save changes to the configuration file.
        /// </summary>
        public void Save()
        {
            this.fDocument.Save(this.fPath);
        }

        /// <summary>
        /// Returns the internal XmlDocument object.
        /// Not recommended for the inexperienced.
        /// </summary>
        public XmlDocument GetInternalDocument()
        {
            return this.fDocument;
        }

        /// <summary>
        /// Returns a value as a byte.
        /// </summary>
        /// <param name="XPath">The XPath query.</param>
        /// <param name="Fallback">The value to return if the value is invalid.</param>
        public byte ValueAsByte(string XPath, byte Fallback)
        {
            byte Result;

            try
            {
                if (!byte.TryParse(ValueAsString(XPath), out Result))
                {
                    Core.GetStandardOut().PrintWarning("Configuration Error: '" + XPath + "' is not a valid byte! Fallback: " + Fallback);
                    return Fallback;
                }
            }
            catch
            {
                Result = Fallback;
            }
            return Result;
        }

        /// <summary>
        /// Returns a value as a signed byte.
        /// </summary>
        /// <param name="XPath">The XPath query.</param>
        /// <param name="Fallback">The value to return if the value is invalid.</param>
        public sbyte ValueAsSbyte(string XPath, sbyte Fallback)
        {
            sbyte Result;

            try
            {
                if (!sbyte.TryParse(ValueAsString(XPath), out Result))
                {
                    Core.GetStandardOut().PrintWarning("Configuration Error: '" + XPath + "' is not a valid sbyte! Fallback: " + Fallback);
                    return Fallback;
                }
            }
            catch
            {
                Result = Fallback;
            }
            return Result;
        }

        /// <summary>
        /// Returns a value as a short.
        /// </summary>
        /// <param name="XPath">The XPath query.</param>
        /// <param name="Fallback">The value to return if the value is invalid.</param>
        public short ValueAsShort(string XPath, short Fallback)
        {
            short Result;

            try
            {
                if (!short.TryParse(ValueAsString(XPath), out Result))
                {
                    Core.GetStandardOut().PrintWarning("Configuration Error: '" + XPath + "' is not a valid short! Fallback: " + Fallback);
                    return Fallback;
                }
            }
            catch
            {
                Result = Fallback;
            }
            return Result;
        }

        /// <summary>
        /// Returns a value as an unsigned short.
        /// </summary>
        /// <param name="XPath">The XPath query.</param>
        /// <param name="Fallback">The value to return if the value is invalid.</param>
        public ushort ValueAsUshort(string XPath, ushort Fallback)
        {
            ushort Result;

            try
            {
                if (!ushort.TryParse(ValueAsString(XPath), out Result))
                {
                    Core.GetStandardOut().PrintWarning("Configuration Error: '" + XPath + "' is not a valid ushort! Fallback: " + Fallback);
                    return Fallback;
                }
            }
            catch
            {
                Result = Fallback;
            }
            return Result;
        }

        /// <summary>
        /// Returns a value as an int.
        /// </summary>
        /// <param name="XPath">The XPath query.</param>
        /// <param name="Fallback">The value to return if the value is invalid.</param>
        public int ValueAsInt(string XPath, int Fallback)
        {
            int Result;

            try
            {
                if (!int.TryParse(ValueAsString(XPath), out Result))
                {
                    Core.GetStandardOut().PrintWarning("Configuration Error: '" + XPath + "' is not a valid int! Fallback: " + Fallback);
                    return Fallback;
                }
            }
            catch
            {
                Result = Fallback;
            }
            return Result;
        }

        /// <summary>
        /// Returns a value as an unsigned int.
        /// </summary>
        /// <param name="XPath">The XPath query.</param>
        /// <param name="Fallback">The value to return if the value is invalid.</param>
        public uint ValueAsUint(string XPath, uint Fallback)
        {
            uint Result;

            try
            {
                if (!uint.TryParse(ValueAsString(XPath), out Result))
                {
                    Core.GetStandardOut().PrintWarning("Configuration Error: '" + XPath + "' is not a valid uint! Fallback: " + Fallback);
                    return Fallback;
                }
            }
            catch
            {
                Result = Fallback;
            }
            return Result;
        }

        /// <summary>
        /// Returns a value as a long.
        /// </summary>
        /// <param name="XPath">The XPath query.</param>
        /// <param name="Fallback">The value to return if the value is invalid.</param>
        public long ValueAsLong(string XPath, long Fallback)
        {
            long Result;

            try
            {
                if (!long.TryParse(ValueAsString(XPath), out Result))
                {
                    Core.GetStandardOut().PrintWarning("Configuration Error: '" + XPath + "' is not a valid long! Fallback: " + Fallback);
                    return Fallback;
                }
            }
            catch
            {
                Result = Fallback;
            }
            return Result;
        }

        /// <summary>
        /// Returns a value as an unsigned long.
        /// </summary>
        /// <param name="XPath">The XPath query.</param>
        /// <param name="Fallback">The value to return if the value is invalid.</param>
        public ulong ValueAsUlong(string XPath, ulong Fallback)
        {
            ulong Result;

            try
            {
                if (!ulong.TryParse(ValueAsString(XPath), out Result))
                {
                    Core.GetStandardOut().PrintWarning("Configuration Error: '" + XPath + "' is not a valid ulong! Fallback: " + Fallback);
                    return Fallback;
                }
            }
            catch
            {
                Result = Fallback;
            }
            return Result;
        }

        /// <summary>
        /// Returns a value as a string.
        /// </summary>
        /// <param name="XPath">The XPath query.</param>
        public string ValueAsString(string XPath)
        {
            XmlNode Node = this.fDocument.SelectSingleNode(XPath + "/text()");
            if (Node == null)
                return "";
            return Node.Value;
        }

        /// <summary>
        /// Returns true if the file required creating when constructing this object.
        /// </summary>
        public bool WasCreated()
        {
            return this.fCreated;
        }


        /// <summary>
        /// Creates a file if it doesn't exist, creating all non-existing parent directories in the process.
        /// </summary>
        /// <param name="File">The file to create.</param>
        /// <returns>True if the file was created, false otherwise.</returns>
        public static bool EnsureFile(FileInfo File)
        {
            if (File.Exists)    // Does the file already exist?
                return true;    // Yes, nothing needed.
            if (EnsureDirectory(File.Directory))    // Ensure all parent directories exist.
            {
                File.Create().Close(); ;  // All missing parent directories created, create the file.
                return true;
            }
            return false;   // Something went wrong, return false.
        }

        /// <summary>
        /// Creates a file if it doesn't exist, creating all non-existing parent directories in the process.
        /// </summary>
        /// <param name="File">The file to create.</param>
        /// <returns>True if the file was created, false otherwise.</returns>
        public static bool EnsureFile(FileInfo File, out bool Created)
        {
            if (File.Exists)    // Does the file already exist?
            {
                Created = false;
                return true;    // Yes, nothing needed.
            }
            if (EnsureDirectory(File.Directory))    // Ensure all parent directories exist.
            {
                File.Create().Close(); ;  // All missing parent directories created, create the file.
                Created = true;
                return true;
            }
            Created = false;
            return false;   // Something went wrong, return false.
        }

        /// <summary>
        /// Creates a directory if it doesn't exist, creating all non-existing parent directories in the process.
        /// </summary>
        /// <param name="Directory">The directory to create.</param>
        /// <returns>True if the directory was created, false otherwise.</returns>
        public static bool EnsureDirectory(DirectoryInfo Directory)
        {
            if (Directory.Exists)   // Does the directory already exist?
                return true;        // Yes, nothing needed.

            Stack<DirectoryInfo> MissingParents = new Stack<DirectoryInfo>(new DirectoryInfo[] { null, Directory });

            DirectoryInfo P = Directory.Parent;

            while (!P.Exists)   // While the next parent directory don't exist...
            {
                MissingParents.Push(P); // Push it onto the MissingParents stack.
                P = P.Parent;           // Go to the next parent
            }

            P = MissingParents.Pop();

            while (MissingParents.Count > 0)    // While their are missing parent directories
            {
                P.Create();                 // Create the directory
                P = MissingParents.Pop();   // Go to the next.
            }

            return true;
        }
    }
}