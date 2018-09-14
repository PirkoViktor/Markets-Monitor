using System.Data.Entity.ModelConfiguration;
using MarketsSystem.Model;

namespace MarketsSystem.MarketsDbContext
{
    public class MarketsConfig : EntityTypeConfiguration<Markets>
    {
        public MarketsConfig()
        {
            HasKey(market => market.MarketId);
            Property(market => market.Href).IsRequired();
            Property(market => market.Name).IsRequired();


            ToTable("Markets");
        }
    }
}