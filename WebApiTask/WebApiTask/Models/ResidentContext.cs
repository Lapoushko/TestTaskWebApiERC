using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace WebApiTask.Models
{
    public class ResidentContext : DbContext
    {
        //Создание базы данных
        public DbSet<Resident> Residents { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "MyDb.db" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);
        }
    }
}
