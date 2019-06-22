using Microsoft.Diagnostics.Runtime.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CsDebugScript.CLR
{
    /// <summary>
    /// Helper class to aid ClrMD in searching for symbols and binaries locations.
    /// </summary>
    internal class ClrMdSymbolLocator : SymbolLocator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMdSymbolLocator"/> class.
        /// </summary>
        public ClrMdSymbolLocator(Process process, SymbolLocator symbolLocator)
        {
            Process = process;
            OriginalSymbolLocator = symbolLocator;
        }

        /// <summary>
        /// Gets the process that this symbol locator is associated to.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the original symbol locator.
        /// </summary>
        public SymbolLocator OriginalSymbolLocator { get; private set; }

        /// <summary>
        /// Validates whether a file on disk matches the properties we expect.
        /// </summary>
        /// <param name="fullPath">The full path on disk of a PEImage to inspect.</param>
        /// <param name="filename">Name of the binary file we expect to match.</param>
        /// <param name="buildTimeStamp">The build timestamp we expect to match.</param>
        /// <param name="imageSize">The build image size we expect to match.</param>
        /// <param name="checkProperties">Whether we should actually validate the imagesize/timestamp or not.</param>
        private bool ValidateBinary(string fullPath, string filename, int buildTimeStamp, int imageSize, bool checkProperties)
        {
            if (string.Compare(Path.GetFileName(fullPath), filename, ignoreCase: true) != 0)
                return false;
            return base.ValidateBinary(fullPath, buildTimeStamp, imageSize, checkProperties);
        }

        /// <summary>
        /// Attempts to locate a binary via the symbol server.  This function will then copy the file
        /// locally to the symbol cache and return the location of the local file on disk.
        /// </summary>
        /// <param name="fileName">The filename that the binary is indexed under.</param>
        /// <param name="buildTimeStamp">The build timestamp the binary is indexed under.</param>
        /// <param name="imageSize">The image size the binary is indexed under.</param>
        /// <param name="checkProperties">Whether or not to validate the properties of the binary after download.</param>
        /// <returns>A full path on disk (local) of where the binary was copied to, null if it was not found.</returns>
        public override string FindBinary(string fileName, int buildTimeStamp, int imageSize, bool checkProperties = true)
        {
            // Check if file exists
            if (base.ValidateBinary(fileName, buildTimeStamp, imageSize, checkProperties))
                return fileName;

            // Check if file is already cached in temp folder
            string testPath = Path.Combine(Path.GetTempPath(), "symbols", fileName, $"{buildTimeStamp:X}{imageSize:X}", fileName);

            if (base.ValidateBinary(testPath, buildTimeStamp, imageSize, checkProperties))
                return testPath;

            // Check if it is known file
            if (fileName.StartsWith("mscordaccore"))
            {
                // mscordacore files are located in known location on Windows platform and can be searched for.
                string knownPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"dotnet\shared\Microsoft.NETCore.App");

                if (Directory.Exists(knownPath))
                {
                    foreach (string versionDirectory in Directory.EnumerateDirectories(knownPath))
                    {
                        testPath = Path.Combine(versionDirectory, fileName);
                        if (base.ValidateBinary(testPath, buildTimeStamp, imageSize, checkProperties))
                            return testPath;
                    }
                }
            }

            // Check if it is one of the modules
            string justFileName = Path.GetFileName(fileName);

            foreach (Module module in Process.ModulesIfCached)
                try
                {
                    if (ValidateBinary(module.ImageName, justFileName, buildTimeStamp, imageSize, checkProperties))
                        return module.ImageName;
                    if (ValidateBinary(module.LoadedImageName, justFileName, buildTimeStamp, imageSize, checkProperties))
                        return module.ImageName;
                    if (ValidateBinary(module.MappedImageName, justFileName, buildTimeStamp, imageSize, checkProperties))
                        return module.ImageName;
                }
                catch
                {
                }

            // Fall back to original symbol locator
            return OriginalSymbolLocator.FindBinary(fileName, buildTimeStamp, imageSize, checkProperties);
        }

        /// <summary>
        /// Attempts to locate a binary via the symbol server.  This function will then copy the file
        /// locally to the symbol cache and return the location of the local file on disk.
        /// </summary>
        /// <param name="fileName">The filename that the binary is indexed under.</param>
        /// <param name="buildTimeStamp">The build timestamp the binary is indexed under.</param>
        /// <param name="imageSize">The image size the binary is indexed under.</param>
        /// <param name="checkProperties">Whether or not to validate the properties of the binary after download.</param>
        /// <returns>A full path on disk (local) of where the binary was copied to, null if it was not found.</returns>
        public override Task<string> FindBinaryAsync(string fileName, int buildTimeStamp, int imageSize, bool checkProperties = true)
        {
            return Task.Run(() => FindBinary(fileName, buildTimeStamp, imageSize, checkProperties));
        }

        /// <summary>
        /// Validates whether a pdb on disk matches the given Guid/revision.
        /// </summary>
        /// <param name="fullPath">The name the pdb is indexed under.</param>
        /// <param name="filename">Just file name that we expect to match.</param>
        /// <param name="pdbIndexGuid">The guid the pdb is indexed under.</param>
        /// <param name="pdbIndexAge">The age of the pdb.</param>
        private bool ValidatePdb(string fullPath, string filename, Guid pdbIndexGuid, int pdbIndexAge)
        {
            if (string.Compare(Path.GetFileName(fullPath), filename, ignoreCase: true) != 0 || !File.Exists(fullPath))
                return false;
            return base.ValidatePdb(fullPath, pdbIndexGuid, pdbIndexAge);
        }

        /// <summary>
        /// Attempts to locate a pdb based on its name, guid, and revision number.
        /// </summary>
        /// <param name="pdbName">The name the pdb is indexed under.</param>
        /// <param name="pdbIndexGuid">The guid the pdb is indexed under.</param>
        /// <param name="pdbIndexAge">The age of the pdb.</param>
        /// <returns>A full path on disk (local) of where the pdb was copied to.</returns>
        public override string FindPdb(string pdbName, Guid pdbIndexGuid, int pdbIndexAge)
        {
            // Check if it is symbol of one of the modules
            string justFileName = Path.GetFileName(pdbName);

            foreach (Module module in Process.ModulesIfCached)
                try
                {
                    if (ValidatePdb(module.SymbolFileName, justFileName, pdbIndexGuid, pdbIndexAge))
                        return module.SymbolFileName;

                    string pdbPath = Path.ChangeExtension(module.MappedImageName, "pdb");

                    if (ValidatePdb(pdbPath, justFileName, pdbIndexGuid, pdbIndexAge))
                        return module.SymbolFileName;
                }
                catch
                {
                }

            // Fall back to original symbol locator
            return OriginalSymbolLocator.FindPdb(pdbName, pdbIndexGuid, pdbIndexAge);
        }

        /// <summary>
        /// Attempts to locate a pdb based on its name, guid, and revision number.
        /// </summary>
        /// <param name="pdbName">The name the pdb is indexed under.</param>
        /// <param name="pdbIndexGuid">The guid the pdb is indexed under.</param>
        /// <param name="pdbIndexAge">The age of the pdb.</param>
        /// <returns>A full path on disk (local) of where the pdb was copied to.</returns>
        public override Task<string> FindPdbAsync(string pdbName, Guid pdbIndexGuid, int pdbIndexAge)
        {
            return Task.Run(() => FindPdb(pdbName, pdbIndexGuid, pdbIndexAge));
        }

        /// <summary>
        /// Copies the given file from the input stream into fullDestPath.
        /// </summary>
        /// <param name="input">The input stream to copy the file from.</param>
        /// <param name="fullSrcPath">The source of this file.  This is for informational/logging purposes and shouldn't be opened directly.</param>
        /// <param name="fullDestPath">The destination path of where the file should go on disk.</param>
        /// <param name="size">The length of the given file.  (Also for informational purposes, do not use this as part of a copy loop.</param>
        /// <returns>A task indicating when the copy is completed.</returns>
        protected override Task CopyStreamToFileAsync(Stream input, string fullSrcPath, string fullDestPath, long size)
        {
            return Task.Run(() => CopyStreamToFile(input, fullSrcPath, fullDestPath, size));
        }
    }
}
