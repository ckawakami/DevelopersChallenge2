namespace Nibo_Full_Stack_Developers_Challenge___Level_2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOfxFileNameFieldToTransactionDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionDetails", "OfxFileName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionDetails", "OfxFileName");
        }
    }
}
