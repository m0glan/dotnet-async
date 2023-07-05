using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Demo.Async.SynchronizationContext.Wpf
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        string? currentThreadId;

        [ObservableProperty]
        bool isLoading;

        public TextBox? TextBox { get; set; }

        [RelayCommand]
        public async Task ExecuteAsyncCrossContext()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            await Delay(continueOnCapturedContext: false).ConfigureAwait(continueOnCapturedContext: false);

            CurrentThreadId = GetCurrentThreadId();
            IsLoading = false;
            UpdateTextBox();
        }

        [RelayCommand]
        public async Task ExecuteAsyncContextAware()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            await Delay(continueOnCapturedContext: true);

            CurrentThreadId = GetCurrentThreadId();
            IsLoading = false;
            UpdateTextBox();
        }

        [RelayCommand]
        public void BlockOnAsyncWithDeadlock()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            var _ = Delay(continueOnCapturedContext: true).Result;

            CurrentThreadId = GetCurrentThreadId();
            IsLoading = false;
            UpdateTextBox();
        }

        [RelayCommand]
        public void BlockOnAsyncWithoutDeadlock()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            var _ = Delay(continueOnCapturedContext: false).Result;

            CurrentThreadId = GetCurrentThreadId();
            IsLoading = false;
            UpdateTextBox();
        }

        [RelayCommand]
        public void BlockOnAsyncWithDeadlockAndContinueOnCapturedContextFalse()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            var f = async () =>
            {
                await Delay(continueOnCapturedContext: true).ConfigureAwait(continueOnCapturedContext: false);
                return string.Empty;
            };

            var _ = f().Result;

            CurrentThreadId = GetCurrentThreadId();
            IsLoading = false;
            UpdateTextBox();
        }

        private void UpdateTextBox()
        {
            string message = $"Got here without crashing or deadlocking! {Guid.NewGuid()}";

            try
            {
                TextBox?.SetCurrentValue(TextBox.TextProperty, message);
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetCurrentThreadId() => Environment.CurrentManagedThreadId.ToString();

        private static async Task<string> Delay(bool continueOnCapturedContext = true)
        {
            await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(continueOnCapturedContext);
            return string.Empty;
        }
    }
}
