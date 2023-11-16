using System.Diagnostics;

namespace MujRozhlas.Runner;

public class BashRunner : IRunner
{
    public int Run(string command)
    {
        Console.WriteLine(command);

        var proc = new Process();
        proc.StartInfo.FileName = "/bin/bash";
        proc.StartInfo.Arguments = "-c \" " + command + " \"";
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = false;
        proc.StartInfo.RedirectStandardError = false;
        proc.Start();
        proc.WaitForExit();

        return 0;
    }
}