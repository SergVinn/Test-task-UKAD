using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ukad_task.Models;

namespace Ukad_task.Services
{
    public class DBService
    {
        DBContext context;
        public DBService()
        {
            context = new DBContext();
        }

        public List<Report> GetAllReports()
        {
            var reports = context.Reports.ToList();
            return reports;
        }

        public void AddNewReports(List<Report> newReports)
        {
            context.Reports.AddRange(newReports);
            context.SaveChanges();
        }

        public void RemoveAllReports()
        {
            context.Reports.RemoveRange(context.Reports);
            context.SaveChanges();

        }

        ~DBService()
        {
            context.Dispose();
        }
    }
}