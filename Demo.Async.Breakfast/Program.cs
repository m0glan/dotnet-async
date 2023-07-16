// See https://aka.ms/new-console-template for more information
using Demo.Async.Breakfast;
using Spectre.Console;

try
{
    await Breakfast.CookDefaultAsync();
}
catch (Exception e)
{
    AnsiConsole.WriteException(e);
}