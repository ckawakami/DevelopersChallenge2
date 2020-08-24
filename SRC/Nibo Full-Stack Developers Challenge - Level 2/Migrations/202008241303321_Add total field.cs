namespace Nibo_Full_Stack_Developers_Challenge___Level_2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addtotalfield : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FileDatas", "Total", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FileDatas", "Total");
        }
    }
}
