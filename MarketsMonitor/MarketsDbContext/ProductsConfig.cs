using System.Data.Entity.ModelConfiguration;
using MarketsSystem.Model;

namespace MarketsSystem.MarketsDbContext
{
    public class ProductsConfig : EntityTypeConfiguration<Products>
    {
        public ProductsConfig()
        {
            HasKey(products => products.ProductsId);
            Property(products => products.AveragePrice).IsRequired();
            Property(products => products.ProductsName).IsRequired().HasMaxLength(50);

            ToTable("Products");
        }
    }
}