using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace CsDebugScript
{
    /// <summary>
    /// Compiles and executes scripts
    /// </summary>
    internal static class ScriptExecution
    {
        /// <summary>
        /// Resolves the path for the specified base file path.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="baseFilePath">The base file path.</param>
        /// <returns>Resolved path if file exists or string.Empty.</returns>
        private static string ResolvePath(string reference, string baseFilePath)
        {
            if (Path.IsPathRooted(reference))
            {
                return File.Exists(reference) ? reference : string.Empty;
            }

            // Try to find in path relative to the base file path
            string path = string.Empty;

            if (!string.IsNullOrEmpty(baseFilePath))
            {
                string folder = baseFilePath;

                if (!Path.IsPathRooted(baseFilePath))
                {
                    folder = Path.GetFullPath(baseFilePath);
                }

                folder = Path.GetDirectoryName(folder);
                path = Path.Combine(folder, reference);
                path = Path.GetFullPath(path);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Try to find in path relative to the current working directory
            path = Path.GetFullPath(reference);
            if (File.Exists(path))
            {
                return path;
            }

            // Look into search folders
            foreach (string folder in ScriptCompiler.SearchFolders)
            {
                path = Path.Combine(folder, reference);
                path = Path.GetFullPath(path);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Helper class to do metadata reference resolver for files.
        /// </summary>
        /// <seealso cref="Microsoft.CodeAnalysis.MetadataReferenceResolver" />
        private class MetadataResolver : MetadataReferenceResolver
        {
            private MetadataReferenceResolver previousResolver;

            public MetadataResolver(MetadataReferenceResolver previousResolver)
            {
                this.previousResolver = previousResolver;
            }

            public override bool Equals(object other)
            {
                return other is MetadataResolver;
            }

            public override int GetHashCode()
            {
                return 0;
            }

            public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
            {
                // Check the previous resolver
                var result = previousResolver.ResolveReference(reference, baseFilePath, properties);

                if (result.Length > 0)
                {
                    return result;
                }

                // Try to use file resolver
                string path = ResolvePath(reference, baseFilePath);

                if (!string.IsNullOrEmpty(path))
                {
                    return ImmutableArray.Create(MetadataReference.CreateFromFile(path, properties));
                }

                return ImmutableArray<PortableExecutableReference>.Empty;
            }
        }

        /// <summary>
        /// Helper class to do source reference resolver for files.
        /// </summary>
        /// <seealso cref="Microsoft.CodeAnalysis.MetadataReferenceResolver" />
        private class SourceResolver : SourceReferenceResolver
        {
            private SourceReferenceResolver originalSourceResolver;

            public SourceResolver(SourceReferenceResolver originalSourceResolver)
            {
                this.originalSourceResolver = originalSourceResolver;
            }

            public override bool Equals(object other)
            {
                return other is SourceResolver;
            }

            public override int GetHashCode()
            {
                return 1;
            }

            public override string NormalizePath(string path, string baseFilePath)
            {
                string result = originalSourceResolver.NormalizePath(path, baseFilePath);

                return result;
            }

            public override Stream OpenRead(string resolvedPath)
            {
                return originalSourceResolver.OpenRead(resolvedPath);
            }

            public override string ResolveReference(string path, string baseFilePath)
            {
                string result = originalSourceResolver.ResolveReference(path, baseFilePath);

                if (string.IsNullOrEmpty(result))
                {
                    result = ResolvePath(path, baseFilePath);
                }

                return result;
            }
        }

        /// <summary>
        /// Executes the specified script.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="args">The arguments.</param>
        internal static void Execute(string path, params string[] args)
        {
            var scriptBase = new ScriptBase();
            var scriptOptions = ScriptOptions.Default.WithImports(ScriptCompiler.DefaultUsings).WithReferences(ScriptCompiler.DefaultAssemblyReferences);
            var originalSourceResolver = scriptOptions.SourceResolver;
            var originalMetadataResolver = scriptOptions.MetadataResolver;

            scriptOptions = scriptOptions.WithMetadataResolver(new MetadataResolver(originalMetadataResolver));
            scriptOptions = scriptOptions.WithSourceResolver(new SourceResolver(originalSourceResolver));

            var argsCode = Convert(args);
            var scriptState = CSharpScript.RunAsync(argsCode, scriptOptions, scriptBase).Result;

            // TODO: What about loading and clearing metadata?
            scriptState = scriptState.ContinueWithAsync(string.Format(@"#load ""{0}""", path)).Result;
        }

        /// <summary>
        /// Converts the specified arguments to the script code so that they can be loaded there.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static string Convert(params string[] args)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;

            sb.Append("string[] args = new string[] { ");
            foreach (var arg in args)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append("@\"");
                sb.Append(arg.Replace("\"", "\"\""));
                sb.Append("\"");
            }

            if (!first)
            {
                sb.Append(" ");
            }

            sb.Append("};");
            return sb.ToString();
        }
    }
}
