﻿// See https://aka.ms/new-console-template for more information

namespace GitLite;

/// <summary>
/// Class that runs GitLite.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Utils.ExitWithError("Please enter a command.");   
        }

        if (args[0] == "init")
        {
            Utils.ValidateArguments("init", args, 1);
            Repository.Init();
            return;
        }

        if (!Repository.GitliteAlreadyInitialized())
        {
            Utils.ExitWithError("Not in an initialized GitLite directory.");
        }
            
        switch (args[0])
        {   
            default:
                Utils.ExitWithError($"No command with such name exists: {args[0]}");
                break;
        }
    }
}