using System;
using System.Data.Entity;
using System.Windows;
using System.Windows.Controls;
using MarketsSystem.MarketsDbContext;
using MarketsSystem.Model;
using MarketsSystem.ViewModel;

namespace MarketsSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MonitorContext Context { get; }
        public static MainWindow main=null; 

        public MainWindow()
        {
            InitializeComponent();
            main = this;
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<MonitorContext>());
          
            Context = new MonitorContext();
            if (Context==null)
            {
               Database.SetInitializer(new DropCreateDatabaseAlways<MonitorContext>()); //  to recreate database

            }

            ProductsTab.DataContext = new MarketViewModel(Context);
       
        }

       
    }
}