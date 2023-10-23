using Microsoft.EntityFrameworkCore;
using System.IO;
using System;

namespace DemoExporter
{
    public class TestingDataContext : DbContext
    {
        public DbSet<TestData> TestDatas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var solutionDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName).FullName).FullName).FullName;
            var databasePath = Path.Join(solutionDir, "AutoPcDemo", "TestData.sqlite");

            options.UseSqlite($"Data Source={databasePath}");
        }
    }
}
