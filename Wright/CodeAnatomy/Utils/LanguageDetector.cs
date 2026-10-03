namespace Wright.CodeAnatomy.Utils;

public static class LanguageDetector
{
    public static string InferLanguage(string filePath)
    {
        if (!File.Exists(filePath)) return "Unknown";

        // Step 1: Check extension first (fastest)
        string ext = Path.GetExtension(filePath).ToLower();
        switch (ext)
        {
            case ".cs": return "C#";
            case ".py": return "Python";
            case ".js":
            case ".mjs":
            case ".cjs": return "JavaScript";
            case ".ts": return "TypeScript";
            case ".tsx": return "TSX";
            case ".go": return "Go";
            case ".java": return "Java";
            case ".c":
            case ".h": return "C";
            case ".cc":
            case ".cpp":
            case ".cxx":
            case ".hpp": return "C++";
            case ".rs": return "Rust";
            case ".rb": return "Ruby";
            case ".php": return "PHP";
            case ".swift": return "Swift";
        }

        // Step 2: Fallback to reading the file content
        using var reader = new StreamReader(filePath);
        string firstLine = reader.ReadLine()?.Trim() ?? "";

        // Check for Linux shebangs (e.g., #!/usr/bin/env python)
        if (firstLine.StartsWith("#!"))
        {
            if (firstLine.Contains("python")) return "Python";
            if (firstLine.Contains("node")) return "JavaScript";
            if (firstLine.Contains("bash") || firstLine.Contains("sh")) return "Shell";
        }

        // Step 3: Heuristics based on keyword combinations
        string fullContent = File.ReadAllText(filePath);
        if (fullContent.Contains("using System;") && fullContent.Contains("namespace")) return "C#";
        if (fullContent.Contains("import React") || fullContent.Contains("console.log(")) return "JavaScript";
        if (fullContent.Contains("def ") && fullContent.Contains("if __name__ == ")) return "Python";
        if (fullContent.Contains("#include <iostream>")) return "C++";

        return "Unknown Text/Code";
    }
}
