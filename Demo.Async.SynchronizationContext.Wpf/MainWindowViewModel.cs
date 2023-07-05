using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System;
using System.Threading;
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
        public async Task UpdateTextBoxInDifferentThread()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            await Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
                CurrentThreadId = GetCurrentThreadId();
                IsLoading = false;
                UpdateTextBox();
            });
        }

        [RelayCommand]
        public async Task ExecuteAsyncCrossContext()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            await DelayAndReturnMilliseconds(continueOnCapturedContext: false).ConfigureAwait(continueOnCapturedContext: false);

            CurrentThreadId = GetCurrentThreadId();
            IsLoading = false;
            UpdateTextBox();
        }

        [RelayCommand]
        public async Task ExecuteAsyncContextAware()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            await DelayAndReturnMilliseconds(continueOnCapturedContext: true);

            CurrentThreadId = GetCurrentThreadId();
            IsLoading = false;
            UpdateTextBox();
        }

        [RelayCommand]
        public void BlockOnAsyncWithDeadlock()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            var _ = DelayAndReturnMilliseconds(continueOnCapturedContext: true).Result;

            CurrentThreadId = GetCurrentThreadId();
            IsLoading = false;
            UpdateTextBox();
        }

        [RelayCommand]
        public void BlockOnAsyncWithoutDeadlock()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            var _ = DelayAndReturnMilliseconds(continueOnCapturedContext: false).Result;

            CurrentThreadId = GetCurrentThreadId();
            IsLoading = false;
            UpdateTextBox();
        }

        [RelayCommand]
        public void BlockOnAsyncWithDeadlockAndContinueOnCapturedContextFalse()
        {
            CurrentThreadId = GetCurrentThreadId();
            IsLoading = true;

            var thirdPartyFunction = () =>
            {
                // Cannot change, not our function...
                return DelayAndReturnMilliseconds(continueOnCapturedContext: true);
            };

            var ourFunction = async () =>
            {
                await thirdPartyFunction().ConfigureAwait(continueOnCapturedContext: false);
                return "Not even gonna get here...";
            };

            var _ = ourFunction().Result;

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

        private static async Task<int> DelayAndReturnMilliseconds(bool continueOnCapturedContext = true)
        {
            var span = TimeSpan.FromSeconds(3);
            await Task.Delay(TimeSpan.FromSeconds(3)).ConfigureAwait(continueOnCapturedContext);
            return (int)span.TotalMilliseconds;
        }
    }
}
