using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NotionFlow.App.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Patrón centralizado de carga de colecciones: gestiona IsBusy, errores y actualización
        /// en el hilo principal. Elimina el boilerplate duplicado en cada ViewModel.
        /// </summary>
        protected async Task ExecuteLoadAsync<T>(
            Func<Task<List<T>>> fetch,
            ObservableCollection<T> target,
            string context)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var items = await fetch();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    target.Clear();
                    foreach (var item in items)
                        target.Add(item);
                });
            }
            catch (Exception ex)
            {
                CrashLog.Write(context, ex);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert("Error", ex.Message, "OK"));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
