using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RainDetectionApp.Models;

namespace RainDetectionApp.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        
        public DbSet<RainLog> RainLogs { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<DeviceCommand> DeviceCommands { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<RainLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.EventType);
            });
            
            
            modelBuilder.Entity<SystemSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SettingName).IsUnique();
            });
            
            
            modelBuilder.Entity<DeviceCommand>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsExecuted);
            });
            
            base.OnModelCreating(modelBuilder);
        }
    }
}