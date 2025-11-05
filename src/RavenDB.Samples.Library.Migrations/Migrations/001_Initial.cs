using Raven.Migrations;

namespace RavenDB.Samples.Library.Migrations.Migrations;

[Migration(1)]
public class Initial : Migration 
{
    public override void Up()
    {
        this.PatchCollection(@"
            from Authors as author
            update {
                author.FullName = author.FirstName + ' ' + author.LastName;
            }
        ");
    }
}