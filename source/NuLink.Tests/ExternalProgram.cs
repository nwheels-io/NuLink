using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NuLink.Tests
{
    public class ExternalProgram
    {
        public static string[] Exec(string program, params string[] args)
        {
            return ExecIn(TestEnvironment.RepoFolder, program, args);
        }

        public static string[] ExecIn(string directory, string program, params string[] args)
        {
            var exitCode = Execute(
                out var output,
                nameOrFilePath: program,
                args: args,
                workingDirectory: directory,
                validateExitCode: false);

            if (exitCode != 0)
            {
                Console.WriteLine($"PROGRAM FAILED: {program} {string.Join(" ", args)}");
                Console.WriteLine($"--- PROGRAM OUTPUT ---");
                foreach (var line in output)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine($"--- END OF PROGRAM OUTPUT ---");
                throw new Exception($"Program '{program}' failed with code {exitCode}.");
            }

            return output.ToArray();
        }
        
        private static int Execute(
            string nameOrFilePath,
            string[] args = null,
            string workingDirectory = null,
            bool validateExitCode = true)
        {
            return Execute(
                out IEnumerable<string> output, 
                nameOrFilePath, 
                args, 
                workingDirectory, 
                validateExitCode, 
                shouldInterceptOutput: false);
        }

        private static int Execute(
            out IEnumerable<string> output,
            string nameOrFilePath,
            string[] args = null,
            string workingDirectory = null,
            bool validateExitCode = true)
        {
            return Execute(
                out output, 
                nameOrFilePath, 
                args, 
                workingDirectory, 
                validateExitCode, 
                shouldInterceptOutput: true);
        }
        
        private static int Execute(
            out IEnumerable<string> output,
            string nameOrFilePath,
            string[] args,
            string workingDirectory,
            bool validateExitCode,
            bool shouldInterceptOutput)
        {
            if (workingDirectory != null && !Directory.Exists(workingDirectory))
            {
                throw new DirectoryNotFoundException("Working folder doesn't exist: " + workingDirectory);
            }
            
            var info = new ProcessStartInfo() {
                UseShellExecute = false,
                FileName = nameOrFilePath,
                Arguments = (args != null ? string.Join(" ", args) : string.Empty),
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = shouldInterceptOutput,
                RedirectStandardError = shouldInterceptOutput
            };

            var process = Process.Start(info);
            List<string> outputLines = null;

            if (shouldInterceptOutput)
            {
                outputLines = new List<string>(capacity: 100);
                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    outputLines.Add(line);
                }
                while ((line = process.StandardError.ReadLine()) != null)
                {
                    outputLines.Add(line);
                }
            }

            process.WaitForExit();
            output = outputLines;

            if (validateExitCode && process.ExitCode != 0)
            {
                throw new Exception(
                    $"Program '{nameOrFilePath}' failed with code {process.ExitCode}." +
                    (outputLines != null ? Environment.NewLine + string.Join(Environment.NewLine, outputLines) : string.Empty));
            }

            return process.ExitCode;
        }
    }
}
