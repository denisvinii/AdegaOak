using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AdegaOak.Data.Data;

#nullable disable

namespace AdegaOak.Data.Migrations
{
    [DbContext(typeof(AdegaOakDbContext))]
    partial class AdegaOakDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");
#pragma warning restore 612, 618
        }
    }
}
