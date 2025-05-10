using Microsoft.EntityFrameworkCore;
using SearchHistoryService.Models;

namespace SearchHistoryService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<SearchHistory> SearchHistories { get; set; }
    public DbSet<SearchFilter> SearchFilters { get; set; }
}