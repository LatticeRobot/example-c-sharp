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
        string programPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        return FindDirectoryContaining(relPath, Path.GetDirectoryName(programPath));
    }

    /// <summary>
    /// Starting from the specified starting location,
    /// search to the root directory for a directory containing the specified relative path.
    /// </summary>
    /// <param name="relPath">The relative path to be matched</param>
    /// <param name="startingDirPath">The starting directory path</param>
    /// <returns>The path to the directory or `null` if not found.</returns>
    /// <exception cref="Exception"></exception>
    private static string FindDirectoryContaining(string relPath, string? startingDirPath) {
        var dirPath = startingDirPath;
        do {
            if (dirPath == null)
                throw new Exception($"Could not find directory containing {relPath} starting at {startingDirPath}");
            string absPath = Path.Combine(dirPath, relPath);
            if (File.Exists(absPath) || Directory.Exists(absPath)) return dirPath;
            dirPath = Path.GetDirectoryName(dirPath);
        } while (true);
    }

    /// <summary>
    /// Throw an exception if any of the specified paths do not exist.
    /// </summary>
    /// <param name="paths"></param>
    public static void AssertExists(string[] paths) {
        foreach (var p in paths) PathUtil.AssertExists(p);
    }

    /// <summary>
    /// Throw an exception if the specified path does not exist.
    /// </summary>
    /// <param name="path"></param>
    public static void AssertExists(string path) {
        if (File.Exists(path) || Directory.Exists(path)) return;
        throw new Exception($"Could not find {path}");
    }
}

