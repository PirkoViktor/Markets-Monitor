using MarketsSystem.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MarketsSystem.Model
{
    public class Notification : INotifyPropertyChanged
    {
        
        private string _errorInfo;

        
        public string ErrorInfo
        {
            get => _errorInfo;
            set
            {
                if (value == _errorInfo) return;
                _errorInfo = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
