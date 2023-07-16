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

    /// <remarks>
    ///     Warning: This method cooks a breakfast partially asynchronously. In a real UI application this is
    ///     not a good thing, since parts of this method would block the UI; to unblock the UI during
    ///     the synchronous/CPU-bound parts, one would usually use something like <see cref="Task.Run(Action)"/>.
    /// </remarks>
    /// <returns>
    ///     A partially asynchronously cooked <see cref="Breakfast"/>
    /// </returns>
    /// <exception cref="IncompleteBreakfastException">
    ///     When the breakfast cooking task is cancelled.
    /// </exception>
    public static async Task<Breakfast> CookDefaultAsync(int eggCount = 2, int toastCount = 2, CancellationToken ct = default)
    {
        try
        {
            var breakfastParts = new List<BreakfastPart>();

            Task<IEnumerable<BreakfastPart>> fryEggsTask = FryEggsAsync(eggCount, ct);
            Task<IEnumerable<BreakfastPart>> makeToastsTask = MakeToastsAsync(toastCount, ct);

            ThrowIfCancelled(ct);

            BreakfastPart juiceGlass = PourJuice().Result;
            breakfastParts.Add(juiceGlass);

            ThrowIfCancelled(ct);

            IEnumerable<BreakfastPart> toasts = await makeToastsTask;

            ThrowIfCancelled(ct);

            IEnumerable<BreakfastPart> toastsWithSpread = ApplySpreadToToasts(toasts);
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

    private static async Task<IEnumerable<BreakfastPart>> FryEggsAsync(int eggCount, CancellationToken ct = default)
    {
        AnsiConsole.MarkupLine($"Started frying [orange4_1]{eggCount} egg(s)[/] [green]asynchronously[/]...");

        await Task.Delay(TimeSpan.FromSeconds(15), ct);

        AnsiConsole.MarkupLine($"Done frying [orange4_1]{eggCount} egg(s)[/] [green]asynchronously[/].");

        return Enumerable.Range(0, eggCount).Select(e => BreakfastPart.FriedEgg);
    }

    private static async Task<IEnumerable<BreakfastPart>> MakeToastsAsync(int toastCount, CancellationToken ct = default)
    {
        AnsiConsole.MarkupLine($"Started making [orange4_1]{toastCount} toast(s)[/] [green]asynchronously[/]...");

        await Task.Delay(TimeSpan.FromSeconds(5), ct);

        AnsiConsole.MarkupLine($"Done making [orange4_1]{toastCount} toast(s)[/] [green]asynchronously[/].");

        return Enumerable.Range(0, toastCount).Select(e => BreakfastPart.Toast);
    }

    private static Task<BreakfastPart> PourJuice()
    {
        AnsiConsole.MarkupLine("Started pouring [orange4_1]juice[/] [red]synchronously[/]...");

        Thread.Sleep(TimeSpan.FromSeconds(3));

        AnsiConsole.MarkupLine("Done pouring [orange4_1]juice[/] [red]synchronously[/].");

        return Task.FromResult(BreakfastPart.JuiceGlass);
    }

    private static IEnumerable<BreakfastPart> ApplySpreadToToasts(IEnumerable<BreakfastPart> toasts)
    {
        if (toasts.Any(t => t is not BreakfastPart.Toast))
        {
            throw new InvalidOperationException("Can only apply spread to toasts.");
        }

        return toasts.Select((t, i) =>
        {
            AnsiConsole.MarkupLine($"Started applying spread to [orange4_1]toast #{i + 1}[/] [red]synchronously[/]...");

            Thread.Sleep(TimeSpan.FromSeconds(3));

            AnsiConsole.MarkupLine($"Done applying spread to [orange4_1]toast #{i + 1}[/] [red]synchronously[/].");

            return BreakfastPart.ToastWithSpread;
        });
    }

    private static void ThrowIfCancelled(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            throw new IncompleteBreakfastException();
        }
    }
}

