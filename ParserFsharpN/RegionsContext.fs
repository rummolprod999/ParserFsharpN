namespace ParserFsharp
open Microsoft.EntityFrameworkCore
open System
open MySql.Data.EntityFrameworkCore.Extensions
open System.ComponentModel.DataAnnotations.Schema

type Region() =
    [<property: Column("id")>]
    member val public Id = 0 with get, set
    
    [<property: Column("okrug_id")>]
    member val public OkrugId = 0 with get, set
    
    [<property: Column("name")>]
    member val public Name = "" with get, set
    
    [<property: Column("path")>]
    member val public Path = "" with get, set
    
    [<property: Column("conf")>]
    member val public Conf = "" with get, set
    
    [<property: Column("path223")>]
    member val public Path223 = "" with get, set
    
type RegionContext() =
    inherit DbContext()
    member val Regions: DbSet<Region> = null with get, set
    
    override this.OnConfiguring(optbuilder: DbContextOptionsBuilder) =
        optbuilder.UseMySQL(S.Settings.ConS) |> ignore
        ()
     
    override this.OnModelCreating(modelBuilder: ModelBuilder) =
         modelBuilder.Entity<Region>().ToTable(String.Format("{0}region", S.Settings.Pref)) |> ignore
         ()
    

