using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NuLink.Cli
{
    public class ExternalProgramRunner
    {
        public int Run(
            string nameOrFilePath,
            string[] args = null,
            string workingDirectory = null,
            bool validateExitCode = true)
        {
            return Run(
                out IEnumerable<string> output, 
                nameOrFilePath, 
                args, 
                workingDirectory, 
                validateExitCode, 
                shouldInterceptOutput: false);
        }

        public int Run(
            out IEnumerable<string> output,
            string nameOrFilePath,
            string[] args = null,
            string workingDirectory = null,
            bool validateExitCode = true)
        {
            return Run(
                out output, 
                nameOrFilePath, 
                args, 
                workingDirectory, 
                validateExitCode, 
                shouldInterceptOutput: true);
        }

        public int Run(
            out IEnumerable<string> output,
            string nameOrFilePath,
            string[] args,
            string workingDirectory,
            bool validateExitCode,
            bool shouldInterceptOutput)
        {
            var info = new ProcessStartInfo() {
                FileName = nameOrFilePath,
                Arguments = (args != null ? string.Join(" ", args) : string.Empty),
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = shouldInterceptOutput
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
