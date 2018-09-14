using System.Collections.Generic;


using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarketsSystem.Annotations;
namespace MarketsSystem.Model
{
   public class Markets:INotifyPropertyChanged
    {
        private int _marketsId;
        private string _marketName;
        private string _href;
        private  IList<Product> _products;

        public Markets()
        {
            this._products = new List<Product>();
        }
        public virtual IList<Product> Products
        {
            get => _products;
            set
            {
                if (Equals(value, _products)) return;
                _products = value;
                OnPropertyChanged();
            }
        }

        public int MarketId
        {
            get => _marketsId;
            set
            {
                if (value == _marketsId) return;
                _marketsId = value;
                OnPropertyChanged();
            }
        }
        public string Name
        {
            get => _marketName;
            set
            {
                if (value == _marketName) return;
                _marketName = value;
                OnPropertyChanged();
            }
        }
        public string Href
        {
            get => _href;
            set
            {
                if (value == _href) return;
                _href = value;
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
