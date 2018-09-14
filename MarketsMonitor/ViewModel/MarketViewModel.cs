using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MarketsSystem.MarketsDbContext;
using MarketsSystem.Model;
using System.Windows.Threading;
using Limilabs.Client.Authentication.Google;

namespace MarketsSystem.ViewModel
{
    public class MarketViewModel : ViewModelBase
    {
        private Products _selectedProducts;
        private Product _selectedProduct;
        private User _user;
        private Notification _notify;
        public MonitorContext Context { get; }
        public Products ProductsInfo { get; set; } = new Products();
        public Product ProductInfo { get; set; } = new Product();
        DateTime localDate = DateTime.Now;


        public MarketViewModel(MonitorContext context)
        {
            Context = context;
            Context.Products.Load();
            Context.Product.Load();
            Context.Markets.Load();
            if (Context.Markets.Count() == 0)
            {
                this.Fill();
            }

            DispatcherTimer timer = new DispatcherTimer();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 20, 0);
            timer.Tick += new EventHandler(this.DBUpdate);
            User = new User
            {
                LastUpdate = Properties.Settings.Default.LastUpdate
            };
            Notify = new Notification();
            // Start the timer. 
            timer.Start();
            try
            {
                GoogleApi api = new GoogleApi(Properties.Settings.Default.Token);
                User.Email = api.GetEmailPlus();
            }
            catch
            {

            }

        }
        public Products SelectedProducts
        {
            get => _selectedProducts;
            set
            {
                _selectedProducts = value;
                RaisePropertyChanged();
            }
        }
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                RaisePropertyChanged();
            }
        }
        public User User
        {
            get => _user;
            set
            {
                _user = value;
                RaisePropertyChanged();
            }
        }
        public Notification Notify
        {
            get => _notify;
            set
            {
                _notify = value;
                RaisePropertyChanged();
            }
        }
        //fill the Db at first start
        private void Fill()
        {
            var market = new[]
            {
                new Markets {Name = "mobimania.ua", Href = "//span[contains(@class, 'price-amount')]"},
                new Markets {Name = "allo.ua", Href = "//span[@class='price']"},
                  new Markets {Name = "foxtrot.com.ua", Href = "//span[contains(@itemprop, 'price')]"},
                  new Markets {Name = "www.mobilluck.com.ua", Href = "//span[@class='price itemprice']"},
                  new Markets {Name = "comfy.ua", Href = "//span[(@class='price-value')]"},
            };

            Context.Markets.AddRange(market);
            Context.SaveChanges();
        }

        private void DBUpdate(object state, EventArgs e)
        {
            HttpWebRequest request = null;

            HttpWebResponse response = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create("http://google.com");
                response = (HttpWebResponse)request.GetResponse();
                response.Close();
            }
            catch
            {
                Notify.ErrorInfo = "No internet connection";

                return;

            }
             var result =  Task.Factory.StartNew(() =>
            {
           
                Notify.ErrorInfo = "Start update...";
          
                foreach (var pr in Context.Product)
                {
                                  pr.ProductPrice = pr.SearchPrice(pr.ProductHref, pr.Markets.Href);
                   
                };
                Context.SaveChanges();
                Notify.ErrorInfo = "Update complit";
                User.LastUpdate = DateTime.Now;
                Properties.Settings.Default.LastUpdate = DateTime.Now;
                Properties.Settings.Default.Save();
            });
        }

        #region Commands
        private RelayCommand<object> mOpenSupportWebsiteCommand;
        private RelayCommand _addProductsCommand;
        private RelayCommand _addLoginCommand;
        private RelayCommand _updateProductCommand;
        private RelayCommand _deleteProductCommand;
        private RelayCommand _updatePriceCommand;
        private RelayCommand _exportProductCommand;
        private RelayCommand _productsGridSelectionChangedCommand;
        private RelayCommand _productGridSelectionChangedCommand;

        /// <summary>
        /// The command object.
        /// </summary>



        public ICommand UpdatePriceCommand =>
           _updatePriceCommand ??
           (_updatePriceCommand = new RelayCommand(
               () =>
               {
                   DBUpdate(null,null);
                 
               },
                () =>

                     true
                )
                );


        public ICommand ExportProductCommand =>
         _exportProductCommand ??
         (_exportProductCommand = new RelayCommand(
             () =>
             {
                 HttpWebRequest request = null;
                 HttpWebResponse response = null;
                 //Checking Internet access
                 try
                 {
                     request = (HttpWebRequest)WebRequest.Create("http://google.com");
                     response = (HttpWebResponse)request.GetResponse();
                     response.Close();
                 }
                 catch
                 {
                     Notify.ErrorInfo = "No internet connection";
                     return;
                 }

                 User.SetExcelDocuments(Context);
                 User.SendEmailwithGoogle();
                 Notify.ErrorInfo = "Email sent";

             },
              () => {
                  if (string.IsNullOrEmpty(User.EmailToSent)|| string.IsNullOrEmpty(User.Email)) return false;
                  return true;
              })
              );

      

        
       
        public ICommand AddProductsCommand =>
            _addProductsCommand ??
            (_addProductsCommand = new RelayCommand(
                () =>
                {
                 
                     //create Products
                     Products newProducts = new Products
                    {
                        AveragePrice = ProductsInfo.AveragePrice,
                        ProductsName = ProductsInfo.ProductsName,

                    };
                    Context.Products.Add(newProducts);
                 Context.SaveChanges();

                    HttpWebRequest request = null;
                    HttpWebResponse response = null;
                    //Checking Internet access
                    try
                    {
                        request = (HttpWebRequest)WebRequest.Create("http://google.com");
                        response = (HttpWebResponse)request.GetResponse();
                        response.Close();
                    }
                    catch
                    {
                        Notify.ErrorInfo = "No internet connection";
                        Context.Products.Remove(newProducts);
                       Context.SaveChanges();
                        return;
                    }

                    //product search on the market site
                    Parallel.ForEach(Context.Markets, markets=>
                   {

                       Product newProduct = new Product();
                       newProduct = newProduct.CreateProduct(newProducts, markets, Notify );
                       if (newProduct!=null)
                           Context.Product.Add(newProduct);
                      

                   });

                    if (newProducts.Product.Count>0)
                    {
                        Context.SaveChanges();
                        //search the lowest price
                        int minPrice = (int)newProducts.Product.Min(o => o.ProductPrice);
                        foreach (var g in newProducts.Product)
                        {
                            if (g.ProductPrice == minPrice)
                            {
                                g.MinPrice = true;
                            }
                            else
                            {
                                g.MinPrice = false;
                            }
                        }
                        //search for average price
                        newProducts.AveragePrice = (int)newProducts.Product.Average(o => o.ProductPrice);
                        SelectedProducts = newProducts;

                        Context.SaveChanges();
                    }
                    else
                    {
                        Context.Products.Remove(newProducts);
                       Context.SaveChanges();
                    }
                },
                () =>{
                         if (string.IsNullOrEmpty(ProductsInfo.ProductsName)) return false;
                                   return true;
                     }));

        public ICommand LoginCommand =>
            _addLoginCommand ??
            (_addLoginCommand = new RelayCommand(
                () =>
                {
                    //get an access token to send a message
                    User.GetAccessToken();
                    Notify.ErrorInfo = "Authorization was successful";
                },
                () => 
                     true
                ));

        public ICommand UpdateProductsCommand =>
            _updateProductCommand ??
            (_updateProductCommand = new RelayCommand(
                () =>
                {
                    HttpWebRequest request = null;

                    HttpWebResponse response = null;
                 
                    try
                    {  //Checking Internet access
                        request = (HttpWebRequest)WebRequest.Create("http://google.com");
                        response = (HttpWebResponse)request.GetResponse();
                        response.Close();
                    }
                    catch
                    {
                    Notify.ErrorInfo = "No internet connection";
                        return ;
                    }
              
                        var task = Task.Factory.StartNew(() =>
                        {
                            Parallel.ForEach(SelectedProducts.Product, pr =>
                            {
                                pr.ProductPrice = pr.SearchPrice(pr.ProductHref, pr.Markets.Href);
                            });
                            Context.SaveChanges();
                            Notify.ErrorInfo = "Update Complit";
                        });
                    
                  
                },
                () => SelectedProducts != null));

        public ICommand OpenSupportWebsiteCommand
        {
            get
            {
                //opening a hyperlink
                if (mOpenSupportWebsiteCommand == null)
                {
                    mOpenSupportWebsiteCommand = new RelayCommand<object>(OpenSupportWebsite);
                }

                return mOpenSupportWebsiteCommand;
            }
        }


        private void OpenSupportWebsite(object url)
        {
            System.Diagnostics.Process.Start(url as string);
        }

        public ICommand DeleteProductsCommand =>
            _deleteProductCommand ??
            (_deleteProductCommand = new RelayCommand(
                () =>
                {
                    Context.Products.Remove(SelectedProducts);
                    Context.SaveChanges();
                  
                  
                },
                () => SelectedProducts != null));


        public ICommand ProductsGridSelectionChangedCommand =>
            _productsGridSelectionChangedCommand ??
            (_productsGridSelectionChangedCommand =
                new RelayCommand(
                    () =>
                    {
                        ProductsInfo.AveragePrice = SelectedProducts.AveragePrice;
                        ProductsInfo.ProductsName = SelectedProducts.ProductsName;
                      
                    },
                    () => SelectedProducts != null));

        public ICommand ProductGridSelectionChangedCommand =>
           _productGridSelectionChangedCommand ??
           (_productGridSelectionChangedCommand =
               new RelayCommand(
                   () =>
                   {
                       ProductInfo.ProductPrice = SelectedProduct.ProductPrice;
                       ProductInfo.ProductHref = SelectedProduct.ProductHref;
                       ProductInfo.Markets = SelectedProduct.Markets;
                       ProductInfo.Products = SelectedProduct.Products;
                       ProductInfo.MinPrice = SelectedProduct.MinPrice;
                       ProductInfo.ProductName = SelectedProduct.ProductName;
                   },
                   () => SelectedProduct != null));



        #endregion
       
    }
}