using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace JhonMark.Models;

public class StoreContext: DbContext
{
    public DbSet<Product> Products { get; set; }

    public StoreContext(DbContextOptions<StoreContext> options) : base(options) 
    { 
        
    }
}
