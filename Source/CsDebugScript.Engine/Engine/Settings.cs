using System.Collections.Generic;

namespace CsDebugScript.Engine
{
    /// <summary>
    /// The settings for script execution
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// Gets or sets the list of search folders for scripts imports.
        /// </summary>
        public List<string> SearchFolders { get; set; } = new List<string>();
    }
}
