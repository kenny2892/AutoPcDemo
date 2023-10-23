using AutoPcDemoWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPcDemoWebsite.Data
{
    public class TestingDataContext : DbContext
    {
        public DbSet<TestData> TestDatas { get; set; }

        public TestingDataContext(DbContextOptions<TestingDataContext> options) : base(options)
        {
        }
    }
}
