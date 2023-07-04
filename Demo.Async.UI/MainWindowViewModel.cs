using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.Async.UI
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        bool isLoading;

        [RelayCommand]
        public async Task LoadWithDelay()
        {
            IsLoading = true;
            await Task.Delay(TimeSpan.FromSeconds(3));  // uses timer, not CPU
            IsLoading = false;
        }

        [RelayCommand]
        public async Task LoadWithSleep()
        {
            IsLoading = true;
            Thread.Sleep(TimeSpan.FromSeconds(3));
            await Task.CompletedTask;
            IsLoading = false;
        }

        [RelayCommand]
        public async Task LoadWithSleepOnDifferentThread()
        {
            IsLoading = true;
            await Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
                IsLoading = false;
            });
        }
    }
}
