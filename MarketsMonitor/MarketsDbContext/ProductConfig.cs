using System.Data.Entity.ModelConfiguration;
using MarketsSystem.Model;

namespace MarketsSystem.MarketsDbContext
{
    public class ProductConfig : EntityTypeConfiguration<Product>
    {
        public ProductConfig()
        {
            HasKey(product => product.ProductId);
            Property(product => product.ProductPrice).IsRequired();

            Property(product => product.ProductName).IsRequired();
            Property(product => product.MinPrice).IsRequired();
            HasRequired(product => product.Products).WithMany(products => products.Product).WillCascadeOnDelete(true);
            HasRequired(product => product.Markets).WithMany(market => market.Products).WillCascadeOnDelete(true);
                        ToTable("Product");
        }
    }
}