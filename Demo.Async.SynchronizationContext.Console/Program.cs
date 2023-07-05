using Nito.AsyncEx;
using Spectre.Console;

namespace Demo.Async.SynchronizationContext.Console;

enum ProgramStates
{
    Main,
    ProgressReportingWithoutSynchronizationContext,
    ProgressReportingWithSynchronizationContext,
    Quit
}

public static class Program
{
    public static void Main(string[] _)
    {
        ProgramStates next = ProgramStates.Main;

        while (next is not ProgramStates.Quit)
        {
            NewPage();

            next = next switch
            {
                ProgramStates.Main => RunMainState(),
                ProgramStates.ProgressReportingWithoutSynchronizationContext => RunProgressReportingWithoutSynchronizationContextState(),
                ProgramStates.ProgressReportingWithSynchronizationContext => RunProgressReportingWithSynchronizationContextState(),
                _ => ProgramStates.Quit
            };
        }
    }

    private static ProgramStates RunMainState()
    {
        var prompt = new SelectionPrompt<int>();
        List<int> selectionOptions = new() { 1, 2, 0 };
        selectionOptions.ForEach(s => prompt.AddChoice(s));
        prompt.UseConverter(s =>
        {
            return s switch
            {
                1 => "Progress reporting without a synchronization context.",
                2 => "Progress reporting with a synchronization context.",
                _ => "Quit."
            };
        });
        int selection = AnsiConsole.Prompt(prompt);

        return selection switch
        {
            1 => ProgramStates.ProgressReportingWithoutSynchronizationContext,
            2 => ProgramStates.ProgressReportingWithSynchronizationContext,
            _ => ProgramStates.Quit
        };
    }

    private static ProgramStates RunProgressReportingWithoutSynchronizationContextState()
    {
        Progress<int> progress = new();
        bool isComplete = false;
        progress.ProgressChanged += (s, e) =>
        {
            AnsiConsole.WriteLine($"Current progress: {e}%");

            if (e >= 100)
            {
                isComplete = true;
            }
        };
        SomeProcess.Run(progress);
        SpinWait.SpinUntil(() => isComplete);

        AnsiConsole.Prompt(new TextPrompt<string>("Hit ENTER to go back...").DefaultValue(""));
        return ProgramStates.Main;
    }

    private static ProgramStates RunProgressReportingWithSynchronizationContextState()
    {
        AsyncContext.Run(() =>
        {
            Progress<int> progress = new();
            progress.ProgressChanged += (s, e) =>
            {
                AnsiConsole.WriteLine($"Current progress: {e}%");
            };
            SomeProcess.Run(progress);
        });

        AnsiConsole.Prompt(new TextPrompt<string>("Hit ENTER to go back...").DefaultValue(""));
        return ProgramStates.Main;
    }

    private static void NewPage()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[purple bold]Console application and sync context demo[/]");
    }
}


