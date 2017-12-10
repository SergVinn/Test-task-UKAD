using System.Data.Entity;
using Ukad_task.Models;

namespace Ukad_task.Services
{
    public class DBContext : System.Data.Entity.DbContext
    {
        public DBContext()
            :base("DbConnection")
        { }

        public DbSet<Report> Reports { get; set; }
    }
}