using System;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using MarketsSystem.Annotations;
using System.ComponentModel;
using HtmlAgilityPack;
using System.Net;
using System.IO;

namespace MarketsSystem.Model
{
    public  class Product: INotifyPropertyChanged
    {
        private int _productId;
        private int _productPrice;
        private string _productHref;
        private bool _minPrice;
        private string _productName;
        private  Products _products;
        private  Markets _market;

        public int ProductId
        {
            get => _productId;
            set
            {
                if (value == _productId) return;
                _productId = value;
                OnPropertyChanged();
            }
        }
        public int ProductPrice
        {
            get => _productPrice;
            set
            {
                if (value == _productPrice) return;
                _productPrice = value;
                OnPropertyChanged();
            }
        }
        public bool MinPrice
        {
            get => _minPrice;
            set
            {
                if (value == _minPrice) return;
                _minPrice = value;
                OnPropertyChanged();
            }
        }
        public string ProductHref
        {
            get => _productHref;
            set
            {
                if (value == _productHref) return;
                _productHref = value;
                OnPropertyChanged();
            }
        }
        public string ProductName
        {
            get => _productName;
            set
            {
                if (value == _productName) return;
                _productName = value;
                OnPropertyChanged();
            }
        }
        public virtual Products Products
        {
            get => _products;
            set
            {
                if (Equals(value, _products)) return;
                _products = value;
                OnPropertyChanged();
            }
        }
        public virtual Markets Markets
        {
            get => _market;
            set
            {
                if (Equals(value, _market)) return;
                _market = value;
                OnPropertyChanged();
            }
        }
     

        public Product CreateProduct(Products products, Markets markets, Notification notify)
        {
            //search for a product link in google
            String Href = this.SearchUrl(markets.Name, products.ProductsName);
            if (Href != null)
            {
                //parsing the market page for price search
                int Price = SearchPrice(Href, markets.Href);
                if (Price != 0)
                {
                    return new Product() {
                        ProductHref = Href,
                        ProductPrice = Price,
                        Products = products,
                        Markets = markets,
                        MinPrice = false,
                        ProductName = _productName
                };

                }
                else
                {
                    notify.ErrorInfo = "The product is not available in: " + markets.Name;
                    return null;
                }
            }
            else
            {
                notify.ErrorInfo = "Could not find the product on the site:" + markets.Name;
                return null;

            }
        }
        public int SearchPrice(string pageHref, string marketHref)
        {
            StringBuilder sb = new StringBuilder();
            byte[] ResultsBuffer = new byte[8192];
            string SearchResults = pageHref;
            HttpWebRequest request;
            HttpWebResponse response=null;
            try
            {
               //creating a connection to the market
                request = (HttpWebRequest)WebRequest.Create(pageHref);
                response = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                return 0;
            }
            Stream resStream = response.GetResponseStream();
           
            string tempString = null;
            int count = 0;
            do
            {
                count = resStream.Read(ResultsBuffer, 0, ResultsBuffer.Length);
                if (count != 0)
                {
                    tempString = Encoding.ASCII.GetString(ResultsBuffer, 0, count);
                    sb.Append(tempString);
                }
            }

            while (count > 0);
            //get market page code
            string sbb = sb.ToString();
            HtmlDocument htmlDoc = new HtmlDocument();
            try
            {
               
                htmlDoc.LoadHtml(sbb);
            }
            catch
            {
                return 0;
            }
            
            //getting the name of the product from the market
            _productName = htmlDoc.DocumentNode.SelectSingleNode("html/head/title").InnerText;
            if (_productName.IndexOf(',') != -1)
                _productName = (_productName.Remove(_productName.IndexOf(',')).Replace("?", ""));
           
            //getting the price of the product from the market
            var priceElement = htmlDoc.DocumentNode
                .SelectSingleNode(marketHref);
       
            int price = 0;
            if (priceElement != null)
            {
                int.TryParse(string.Join("", priceElement.InnerText.Where(c => char.IsDigit(c))),out price);
            }
           
            return price;
        }

        public String SearchUrl(string marketsName, string productsName)
        {

            StringBuilder sb = new StringBuilder();
            byte[] ResultsBuffer = new byte[8192];
            //create a search string
            string SearchResults = "https://google.com/search?q=site:" + marketsName+" "+productsName;
            HttpWebRequest request = null;
            HttpWebResponse response=null;
            try
            {
                //creating a connection to the market
                request = (HttpWebRequest)WebRequest.Create(SearchResults);
                response = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                return null;
            }
          
            Stream resStream = response.GetResponseStream();
            string tempString = null;
            int count = 0;
            do
            {
                count = resStream.Read(ResultsBuffer, 0, ResultsBuffer.Length);
                if (count != 0)
                {
                    tempString = Encoding.ASCII.GetString(ResultsBuffer, 0, count);
                    sb.Append(tempString);

                }
            }

            while (count > 0);
            string sbb = sb.ToString();
            
            HtmlAgilityPack.HtmlDocument html = new HtmlAgilityPack.HtmlDocument();
            html.OptionOutputAsXml = true;
            html.LoadHtml(sbb);
            HtmlNode doc = html.DocumentNode;
          
            foreach (HtmlNode link in doc.SelectNodes("//a[@href]"))
            {
                //search for product reference
                string hrefValue = link.GetAttributeValue("href", string.Empty);
                if (!hrefValue.ToString().ToUpper().Contains("GOOGLE") && hrefValue.ToString().Contains("/url?q=") && (hrefValue.ToString().ToUpper().Contains("HTTP://")|| hrefValue.ToString().ToUpper().Contains("HTTPS://")))
                {
                    int index = hrefValue.IndexOf("&");
                    if (index > 0)
                    {
                        hrefValue = hrefValue.Substring(0, index);
                      return hrefValue.Replace("/url?q=", "");
                    }
                }
            }
         
            return null;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       
    }
}
