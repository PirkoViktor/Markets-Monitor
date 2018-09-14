using System.Data.Entity;
using MarketsSystem.Model;

namespace MarketsSystem.MarketsDbContext
{
    public class MonitorContext : DbContext
    {
        public MonitorContext(string connectionString = "MarketDbConnectionString")
            : base(connectionString)
        {
            
        }

        public virtual DbSet<Markets> Markets { get; set; }
        public virtual DbSet<Product> Product { get; set; }

        public virtual DbSet<Products> Products { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new MarketsConfig());
            modelBuilder.Configurations.Add(new ProductConfig());
            modelBuilder.Configurations.Add(new ProductsConfig());
        }
    }
}