using Spectre.Console;

namespace Demo.Async.TaskCompletionSource;

public static class Program
{
    public static async Task Main(string[] args)
    {
        ProgramStates next = ProgramStates.Main;

        while (next is not ProgramStates.Quit)
        {
            ClearPage();

            next = next switch
            {
                ProgramStates.Main => RunMainState(),
                ProgramStates.AsyncWithEvents => RunAsyncWithEventsState(),
                ProgramStates.AsyncifyNaive => await RunAsyncifyNaiveStateAsync(),
                ProgramStates.AsyncifyRight => await RunAsyncifyRightStateAsync(),
                _ => ProgramStates.Quit,
            };
        }
    }

    private static void ClearPage()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[purple bold]Task Completion Source Demo[/]");
    }

    private static ProgramStates RunMainState()
    {
        string description =
            @"
This program showcases a traditional async workflow that works via
event handlers (but is still not blocking to the main thread) as well
as how to make that workflow async/await compatible ('asyncify') using 
System.Threading.Tasks.[springgreen4]TaskCompletionSource[/].
            ";
        AnsiConsole.MarkupLine(description);

        var selectionPrompt = 
            new SelectionPrompt<int>()
                .UseConverter(c =>
                {
                    return c switch
                    {
                        1 => "1. Traditional async workflow via event handlers",
                        2 => "2. 'Asyncify' the naive way (wasting a thread)",
                        3 => "3. 'Asincify' the right way (TaskCompletionSource)",
                        _ => "0. Quit"
                    };
                });
        var selections = new List<int> { 1, 2, 3, 0 };
        selections.ForEach(c => selectionPrompt.AddChoice(c));

        int selection = AnsiConsole.Prompt(selectionPrompt);

        return selection switch
        {
            1 => ProgramStates.AsyncWithEvents,
            2 => ProgramStates.AsyncifyNaive,
            3 => ProgramStates.AsyncifyRight,
            _ => ProgramStates.Quit
        };
    }

    private static ProgramStates RunAsyncWithEventsState()
    {
        string description =
            @"
This example uses the [springgreen4]Timer[/].[yellow]SetTimeout[/]([springgreen4]Action[/], [springgreen4]TimeSpan[/]) custom method to call
an action at the end of the time span that will print 'Timer is finally done!'. The goal here is to show that the operation is async
and the main thread does not get blocked.
            ";
        AnsiConsole.MarkupLine(description);

        var selectionPrompt =
            new SelectionPrompt<int>()
                .UseConverter(c =>
                {
                    return c switch
                    {
                        1 => "1. Start",
                        2 => "2. Back",
                        _ => "0. Quit"
                    };
                });
        var selections = new List<int> { 1, 2, 0 };
        selections.ForEach(c => selectionPrompt.AddChoice(c));

        ProgramStates RunExample()
        {
            int timeoutInSeconds = AnsiConsole.Prompt(new TextPrompt<int>("Delay (seconds):"));
            TimeSpan timeout = TimeSpan.FromSeconds(timeoutInSeconds);

            bool isElapsed = false;
            Timer.SetTimeout(() => isElapsed = true, timeout);

            while (!isElapsed)
            {
                AnsiConsole.WriteLine("Main thread still free while timer running...");
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
            }

            AnsiConsole.WriteLine("Timer is finally done!");
            AnsiConsole.Prompt(new TextPrompt<string>("Hit ENTER to go back...").DefaultValue(""));

            return ProgramStates.Main;
        }

        return AnsiConsole.Prompt(selectionPrompt) switch
        {
            1 => RunExample(),
            2 => ProgramStates.Main,
            _ => ProgramStates.Quit
        };
    }

    private static async Task<ProgramStates> RunAsyncifyNaiveStateAsync()
    {
        string description =
    @"
This example wraps the previous method [springgreen4]Timer[/].[yellow]SetTimeout[/]([springgreen4]Action[/], [springgreen4]TimeSpan[/]) in a
[springgreen4]Timer[/].[yellow]DelayNaivelyAsync[/]([springgreen4]TimeSpan[/]) so it can now be used with async/await. This example
showcases the naive way to do so, since this method borrows a thread from the thread pool, spins it while the timer runs
and then completes when the timer is done. This is a complete waste of a thread, as we will see in the next example.
    ";
        AnsiConsole.MarkupLine(description);

        var selectionPrompt =
            new SelectionPrompt<int>()
                .UseConverter(c =>
                {
                    return c switch
                    {
                        1 => "1. Start",
                        2 => "2. Back",
                        _ => "0. Quit"
                    };
                });
        var selections = new List<int> { 1, 2, 0 };
        selections.ForEach(c => selectionPrompt.AddChoice(c));

        return AnsiConsole.Prompt(selectionPrompt) switch
        {
            1 => await RunAsyncifyExampleAsync(true),
            2 => ProgramStates.Main,
            _ => ProgramStates.Quit
        };
    }

    private static async Task<ProgramStates> RunAsyncifyRightStateAsync()
    {
        string description =
    @"
This example is the same as the previous one, except that it implements a [springgreen4]Timer[/].[yellow]DelayAsync[/]([springgreen4]TimeSpan[/]) method the
'correct' way, using System.Threading.Tasks.[springgreen4]TaskCompletionSource[/], thus preventing the waste of an extra thread.
    ";
        AnsiConsole.MarkupLine(description);

        var selectionPrompt =
            new SelectionPrompt<int>()
                .UseConverter(c =>
                {
                    return c switch
                    {
                        1 => "1. Start",
                        2 => "2. Back",
                        _ => "0. Quit"
                    };
                });
        var selections = new List<int> { 1, 2, 0 };
        selections.ForEach(c => selectionPrompt.AddChoice(c));

        return AnsiConsole.Prompt(selectionPrompt) switch
        {
            1 => await RunAsyncifyExampleAsync(),
            2 => ProgramStates.Main,
            _ => ProgramStates.Quit
        };
    }

    private static async Task<ProgramStates> RunAsyncifyExampleAsync(bool isNaive = false)
    {
        int timeoutInSeconds = AnsiConsole.Prompt(new TextPrompt<int>("Delay (seconds):"));
        TimeSpan delay = TimeSpan.FromSeconds(timeoutInSeconds);
        Task delayTask = isNaive ? Timer.DelayNaivelyAsync(delay) : Timer.DelayAsync(delay);

        AnsiConsole.WriteLine("Main thread still free while delay is running... Next we will await the delay.");

        await delayTask;
        AnsiConsole.WriteLine("Delay is finally over!");
        AnsiConsole.Prompt(new TextPrompt<string>("Hit ENTER to go back...").DefaultValue(""));

        return ProgramStates.Main;
    }
}

enum ProgramStates
{
    Main,
    AsyncWithEvents,
    AsyncifyNaive,
    AsyncifyRight,
    Quit
}