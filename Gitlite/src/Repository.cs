using MessagePack;

namespace Gitlite;

/// <summary>
/// Class representing a GitLite repository.
/// </summary>
    public class Repository
{
    
    private static DirectoryInfo CWD = Directory.GetParent(AppContext.BaseDirectory);
    public static DirectoryInfo GITLITE_DIR = Utils.JoinDirectory(CWD, ".gitlite");
    public static DirectoryInfo COMMITS_DIR = Utils.JoinDirectory(GITLITE_DIR, "commits");
    private static DirectoryInfo BLOBS_DIR = Utils.JoinDirectory(GITLITE_DIR, "blobs");
    private static DirectoryInfo BRANCHES = Utils.JoinDirectory(GITLITE_DIR, "branches");
    
    
    /* GITLITE MAIN COMMANDS */
    
    /// <summary>
    /// This initializes the GitLite repository on the Current Working Directory.
    /// </summary>
    public static void Init()
    {
        // Check if .gitlite folder already exists
        if (GitliteAlreadyInitialized())
        {
            Utils.ExitWithError("A Gitlet version-control system already exists in the current directory.");
        }
        
        // Creates the directory structure inside .gitlite
        CreateDirs();
        
        string hash = Gitlite.Commit.CreateInitialCommit();
        CreateBranch("master", hash);
        Utils.WriteContent(Path.Combine(GITLITE_DIR.ToString(), "HEAD"), hash);
        
        Console.WriteLine($"Initialized a new GitLite at {CWD.ToString()}");
    }
    public static void Add(string fileName)
    {
        if (!File.Exists(fileName))
        {
            Utils.ExitWithError("File does not exist.");
        }

        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();
        Dictionary<string, byte[]> forAddition = stagingArea.GetStagingForAddition();

        // using (FileStream file = File.Open(fileName, FileMode.Open))
        // {
        //     // Serializing is kind of useless i Guess
        //     // byte[] serializedFile = MessagePackSerializer.Serialize(file.ReadByte());
        //     
        //     if (!forAddition.ContainsKey(fileName))
        //     {
        //         forAddition.Add(fileName, File.ReadAllBytes(fileName));
        //         Console.WriteLine("Not yet staged");
        //     }
        //
        //     byte[] sameFileFromStagingArea = forAddition[fileName];
        //
        //     if (!serializedFile.SequenceEqual(sameFileFromStagingArea))
        //     {
        //         forAddition[fileName] = serializedFile;
        //         Console.WriteLine("Different file");
        //     }
        // }

        byte[] fileInByte = File.ReadAllBytes(fileName);
        
        if (!forAddition.ContainsKey(fileName))
        {
            forAddition.Add(fileName, File.ReadAllBytes(fileName));
        }
        
        byte[] sameFileFromStagingArea = forAddition[fileName];
        
        if (!fileInByte.SequenceEqual(sameFileFromStagingArea))
        {
            forAddition[fileName] = fileInByte;
        }

        byte[] reserializedStagingArea = MessagePackSerializer.Serialize(stagingArea);
        Utils.WriteContent(StagingArea.STAGING_AREA, reserializedStagingArea);
    }

    public static void Commit(string logMessage)
    {
        /* Steps:
         * 1. Clone the current commit. This will be the new commit's
         *    parent.
         * 2. Save the new files from staging area as blobs
         * 3. Update blobs of the updated files on staging area for addition.
         * 4. Clear the staging area and save it.
         * 5. Save the commit using its SHA-1 as name.
         * 6. Update the pointers. HEAD and branch pointer.
         */
        
        // Get staging area
        StagingArea stagingArea = StagingArea.GetDeserializedStagingArea();

        // Check for failure cases
        if (!stagingArea.IsThereStagedFiles())
        {
            Utils.ExitWithError("No changes added to the commit.");
        }
        
        Dictionary<string, byte[]> forAddition = stagingArea.GetStagingForAddition();
        Dictionary<string, byte[]> forRemoval = stagingArea.GetStagingForRemoval();
        Commit commitOnHEAD = Gitlite.Commit.GetHeadCommit();
        Dictionary<string, string> fileMapping = commitOnHEAD.FileMapping;

        // Invariant: COMMIT.FileMapping contains the mapping of file names to blobs.
        
        foreach (var keyValuePair in forAddition)
        {
            // If file in addition is not yet being tracked.
            if (!fileMapping.ContainsKey(keyValuePair.Key))
            {
                // Save the contents of file as blob
                byte[] bytes = keyValuePair.Value;
                string hash = SaveAsBlob(bytes);
                
                // Finally add the mapping to commit
                fileMapping.Add(keyValuePair.Key, hash);
                
            } else if (fileMapping.ContainsKey(keyValuePair.Key))
            {
                byte[] bytes = keyValuePair.Value;
                string hash = SaveAsBlob(bytes);
                
                // Update the file mapping of commit
                fileMapping[keyValuePair.Key] = hash;
            }
        }
        
        // For removal, todo later.
        // code...
        
        // Clear the staging area then save
        forAddition.Clear();
        forRemoval.Clear();
        stagingArea.Save();
        
        // Create and save the new commit
        string hashRef = Gitlite.Commit.CreateCommit(logMessage, DateTime.Now, fileMapping, commitOnHEAD.Branch, Gitlite.Commit.GetHash(commitOnHEAD));
        
        // Update HEAD and branch pointer
        Utils.WriteContent(Path.Combine(GITLITE_DIR.ToString(), "HEAD"), hashRef);
        Utils.WriteContent(Path.Combine(BRANCHES.ToString(), commitOnHEAD.Branch), hashRef);
        
    }

    
    /// <summary>
    /// Creates a GitLite branch.
    /// </summary>
    /// <param name="name">Name of the branch</param>
    /// <param name="commitHashRef">Commit hash reference that the branch points to</param>
    private static void CreateBranch(string name, string commitHashRef)
    {
        string branch = Path.Combine(BRANCHES.ToString(), name);
        Utils.WriteContent(branch, commitHashRef);
    }

    /// <summary>
    /// Creates the directory structures inside the .gitlite directory
    /// </summary>
    private static void CreateDirs()
    {
        GITLITE_DIR.Create();
        COMMITS_DIR.Create();
        BLOBS_DIR.Create();
        BRANCHES.Create();
        StagingArea.CreateStagingArea();
    }

    /// <summary>
    /// Checks if GitLite has already been initialized.
    /// </summary>
    /// <returns>true if already initialized otherwise false</returns>
    public static bool GitliteAlreadyInitialized()
    {
        return GITLITE_DIR.Exists;
    }

    /// <summary>
    /// Saves CONTENTS as a blob file.
    /// </summary>
    /// <param name="contents">Contents to write as blob.</param>
    /// <returns>The hash value of CONTENTS.</returns>
    public static string SaveAsBlob(byte[] contents)
    {
        string hash = Utils.HashBytes(contents);
        if (!File.Exists(Path.Combine(BLOBS_DIR.ToString(), hash)))
        {
            Utils.WriteContent(Path.Combine(BLOBS_DIR.ToString(), hash), contents);
        }
        return hash;
    }
    
}