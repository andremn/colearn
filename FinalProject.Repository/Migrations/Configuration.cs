using System.Data.Entity.Migrations;

namespace FinalProject.DataAccess.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<DatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
    }
}