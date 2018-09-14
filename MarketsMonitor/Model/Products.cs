using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarketsSystem.Annotations;

namespace MarketsSystem.Model
{
    

    public class Products : INotifyPropertyChanged
    {
        public Products()
        {
            this._products = new List<Product>();
        }
        
        private string _productsName;
        private int _avaragePrice;
        private int _productsId;
        private IList<Product> _products;



        public int ProductsId
        {
            get => _productsId;
            set
            {
                if (value == _productsId) return;
                _productsId = value;
                OnPropertyChanged();
            }
        }

        public int AveragePrice
        {
                 get => _avaragePrice;
                 set
                 {
                     if (value == _avaragePrice) return;
                _avaragePrice = value;
                     OnPropertyChanged();
                 }
         }

        public string  ProductsName
        {
            get => _productsName;
            set
            {
                if (value == _productsName) return;
                _productsName = value;
                OnPropertyChanged();
            }
        }


        public virtual IList<Product> Product
        { 
                 get => _products;
                 set
                 {
                     if (Equals(value, _products)) return;
                _products = value;
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