namespace Demo.Async.SynchronizationContext.Console;

public static class SomeProcess
{
    public static void Run(IProgress<int> progress)
    {
        for (int i = 1; i <= 100; i++)
        {
            progress.Report(i);
        }
    }
}
