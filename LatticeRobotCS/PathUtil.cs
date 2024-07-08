// using System.CodeDom;
// using System.CodeDom.Compiler;

class PathUtil {
    /// <summary>
    /// Starting from the program location, 
    /// search to the root directory for a directory containing the specified relative path.
    /// </summary>
    /// <param name="relPath">The relative path to be matched</param>
    /// <returns>The path to the directory or `null` if not found.</returns>
    /// <exception cref="Exception"></exception>
    public static string FindDirectoryContaining(string relPath) {
        // Get the path of the program being executed
        string programPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string? dirPath = Path.GetDirectoryName(programPath);

        // Find the Samples directory
        do {
            if (dirPath == null)
                throw new Exception($"Could not find directory containing {relPath} starting at {programPath}");
            string absPath = Path.Combine(dirPath, relPath);
            if (File.Exists(absPath) || Directory.Exists(absPath)) return dirPath;
            dirPath = Path.GetDirectoryName(dirPath);
        } while (true);
    }
}

