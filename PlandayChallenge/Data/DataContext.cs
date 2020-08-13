using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlandayChallenge.Domain;

namespace PlandayChallenge.Data
{
    public class DataContext : IdentityDbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {

        }

        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Employee> Employees { get; set; }
    }
}
