namespace MujRozhlas.Runner;

public interface IRunner
{
    int Run(string command, string? runInFolder = null);
}