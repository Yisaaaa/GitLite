using System.Security.Cryptography;

namespace Gitlite;

public static class Utils
{
    
    /* READING & WRITING */
    /// <summary>
    /// Writes a single string content to a file.
    /// </summary>
    /// <param name="file">File name</param>
    /// <param name="content">Content to write into the file</param>
    public static void WriteContent(string file, string content)
    {
        using (StreamWriter sw = new StreamWriter(file))
        {
            sw.Write(content);
        }
    }

    /// <summary>
    /// Writes byte array content to a file.
    /// </summary>
    /// <param name="file">File name</param>
    /// <param name="content">Content in to write represented in byte array into the file</param>
    public static void WriteContent(string file, byte[] content)
    {
        File.WriteAllBytes(file, content);
    }

    public static byte[] ReadContentsAsBytes(string file)
    {
        return File.ReadAllBytes(file);
    }

    public static string ReadContentsAsString(string file)
    {
        return File.ReadAllText(file);
    }
    
    /* FILE & DIR UTILS*/
  
    /// <summary>
    /// Joins the first and second directory path together.
    /// </summary>
    /// <param name="first">String representation of the first path</param>
    /// <param name="second">String representation of the second path to add
    /// to the FIRST path</param>
    /// <returns>A DirectoryInfo referencing to the joined directory.</returns>
    public static DirectoryInfo JoinDirectory(string first, string second)
    {
        return new DirectoryInfo(Path.Combine(first, second));
    }

    /// <summary>
    /// Joins a DirectoryInfo with a string together.
    /// </summary>
    /// <param name="first">DirectoryInfo representation of the first path</param>
    /// <param name="second">String representation of the second path to add
    /// to the FIRST path</param>
    /// <returns>A DirectoryInfo referencing to the joined directory.</returns>
    public static DirectoryInfo JoinDirectory(DirectoryInfo first, string second)
    {
        return JoinDirectory(first.ToString(), second);
    }
    
    
    /* MESSAGES & ERROR REPORTING*/
    
    /// <summary>
    /// Exits the program and prints out MESSAGE.
    /// </summary>
    /// <param name="message">Message to print out</param>
    public static void ExitWithError(string? message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Console.WriteLine(message);
        }

        Environment.Exit(-1);
    }
    
    /// <summary>
    /// Checks if the number of arguments provided with CMD(command) is valid.
    /// </summary>
    /// <param name="cmd">Command name.</param>
    /// <param name="args">The arguments provided along with the COMMAND.</param>
    /// <param name="n">The number of arguments required.</param>
    /// <param name="message">Message to print if command is not valid.</param>>
    public static void ValidateArguments(string cmd, string[] args, int n, string? message=null)
    {
        if (args.Length == n) return;
        
        if (string.IsNullOrEmpty(message))
        {
            ExitWithError($"Invalid number of arguments for: {cmd}");
        }
            
        ExitWithError(message);
    }
    
    /* HASHING */
    public static string HashBytes(byte[] bytes)
    {
        return Convert.ToHexString(SHA1.HashData(bytes)).ToLower();
    }
}