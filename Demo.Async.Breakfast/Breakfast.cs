using Spectre.Console;

namespace Demo.Async.Breakfast;

public enum BreakfastPart
{
    FriedEgg,
    Toast,
    ToastWithSpread,
    JuiceGlass
}

public class IncompleteBreakfastException : Exception
{
    public IncompleteBreakfastException() : base("Breakfast was cancelled :(") { }
}

public class Breakfast
{
    public IEnumerable<BreakfastPart> Parts { get; }

    public Breakfast(IEnumerable<BreakfastPart> parts)
    {
        Parts = parts ?? Enumerable.Empty<BreakfastPart>();
    }

    public static async Task<Breakfast> CookDefaultAsync(int eggCount = 2, int toastCount = 2, CancellationToken ct = default)
    {
        try
        {
            var breakfastParts = new List<BreakfastPart>();

            Task<IEnumerable<BreakfastPart>> fryEggsTask = FryEggsAsync(eggCount, ct);
            Task<IEnumerable<BreakfastPart>> makeToastTask = MakeToastAsync(toastCount, ct);

            ThrowIfCancelled(ct);

            BreakfastPart juiceGlass = PourJuice();
            breakfastParts.Add(juiceGlass);

            ThrowIfCancelled(ct);

            IEnumerable<BreakfastPart> toasts = await makeToastTask;

            ThrowIfCancelled(ct);

            IEnumerable<BreakfastPart> toastsWithSpread = toasts.Select(ApplySpreadToToast);
            breakfastParts.AddRange(toastsWithSpread);

            ThrowIfCancelled(ct);

            IEnumerable<BreakfastPart> friedEggs = await fryEggsTask;
            breakfastParts.AddRange(friedEggs);

            AnsiConsole.MarkupLine("The [mediumvioletred]breakfast[/] is ready!");

            return new Breakfast(breakfastParts);
        }
        catch (TaskCanceledException)
        {
            throw new IncompleteBreakfastException();
        }
    }

    private static async Task<IEnumerable<BreakfastPart>> FryEggsAsync(int eggCount, CancellationToken cts = default)
    {
        AnsiConsole.MarkupLine("Started frying [orange4_1]eggs[/] [green]asynchronously[/]...");

        await Task.Delay(TimeSpan.FromSeconds(15), cts);

        AnsiConsole.MarkupLine("Done frying [orange4_1]eggs[/] [green]asynchronously[/].");

        return Enumerable.Range(0, eggCount).Select(e => BreakfastPart.FriedEgg);
    }

    private static async Task<IEnumerable<BreakfastPart>> MakeToastAsync(int toastCount, CancellationToken cts = default)
    {
        AnsiConsole.MarkupLine("Started making [orange4_1]toast[/] [green]asynchronously[/]...");

        await Task.Delay(TimeSpan.FromSeconds(5), cts);

        AnsiConsole.MarkupLine("Done making [orange4_1]toast[/] [green]asynchronously[/].");

        return Enumerable.Range(0, toastCount).Select(e => BreakfastPart.Toast);
    }

    private static BreakfastPart PourJuice()
    {
        AnsiConsole.MarkupLine("Started pouring [orange4_1]juice[/] [red]synchronously[/]...");

        Thread.Sleep(TimeSpan.FromSeconds(3));

        AnsiConsole.MarkupLine("Done pouring [orange4_1]juice[/] [red]synchronously[/].");

        return BreakfastPart.JuiceGlass;
    }

    private static BreakfastPart ApplySpreadToToast(BreakfastPart _)
    {
        AnsiConsole.MarkupLine("Started applying spread to [orange4_1]toast[/] [red]synchronously[/]...");

        Thread.Sleep(TimeSpan.FromSeconds(3));

        AnsiConsole.MarkupLine("Done applying spread to [orange4_1]toast[/] [red]synchronously[/].");

        return BreakfastPart.ToastWithSpread;
    }

    private static void ThrowIfCancelled(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            throw new IncompleteBreakfastException();
        }
    }
}

