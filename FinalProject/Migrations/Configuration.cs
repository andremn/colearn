using System;
using System.Data.Entity.Migrations;
using FinalProject.Context;

namespace FinalProject.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<UserDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
    }
}