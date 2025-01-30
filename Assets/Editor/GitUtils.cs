using System;
using System.Diagnostics;

public class GitUtils
{
        public static string RunGitCommand(string gitCommand)
        {
            const string TAG = "<color=green>[GIT]</color> ";
            const string FATAL_TAG = "<color=red>[GIT]</color> ";
            
            // Strings that will catch the output from our process.
            string output = "no-git";
            string errorOutput = "no-git";

            // Set up our processInfo to run the git command and log to output and errorOutput.
            ProcessStartInfo processInfo = new ProcessStartInfo("git", @gitCommand) {
                CreateNoWindow = true,          // We want no visible pop-ups
                UseShellExecute = false,        // Allows us to redirect input, output and error streams
                RedirectStandardOutput = true,  // Allows us to read the output stream
                RedirectStandardError = true    // Allows us to read the error stream
            };

            // Set up the Process
            Process process = new Process {StartInfo = processInfo};
            
            UnityEngine.Debug.Log(TAG + "--> git " + gitCommand);

            try 
            {
                process.Start();  // Try to start it, catching any exceptions if it fails
            } 
            catch (Exception e) 
            {
                // For now just assume its failed cause it can't find git.
                UnityEngine.Debug.LogError(TAG + "Git is not set-up correctly, required to be on PATH, and to be a git project.");
                return "";
            }

            // Read the results back from the process so we can get the output and check for errors
            output = process.StandardOutput.ReadToEnd();
            errorOutput = process.StandardError.ReadToEnd();

            process.WaitForExit();  // Make sure we wait till the process has fully finished.
            process.Close();        // Close the process ensuring it frees it resources.

            if (errorOutput != "")
            {
                UnityEngine.Debug.Log(FATAL_TAG + "<-- " + errorOutput);
                return errorOutput;
            }

            if(output != "")
                UnityEngine.Debug.Log(TAG + "<-- " + output);
            
            return output;  // Return the output from git.
        }
    }
