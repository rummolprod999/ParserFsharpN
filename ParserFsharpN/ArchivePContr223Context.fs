namespace ParserFsharp

open Microsoft.EntityFrameworkCore
open System
open System.ComponentModel.DataAnnotations.Schema
open System.ComponentModel.DataAnnotations

type ArchivePContr223() =
    [<Key>]
    [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
    [<property: Column("id")>]
    member val public Id = 0 with get, set

    [<property: Column("arhiv")>]
    member val public Archive = "" with get, set

    [<property: Column("size_archive")>]
    member val public SizeArch = 0L with get, set

type ArchivePContr223Context() =
    inherit DbContext()

    [<DefaultValue>]
    val mutable archives : DbSet<ArchivePContr223>
    member x.Archives
        with get () = x.archives
        and set v = x.archives <- v

    override __.OnConfiguring(optbuilder : DbContextOptionsBuilder) =
        optbuilder.UseMySQL(S.Settings.ConS) |> ignore
        ()

    override __.OnModelCreating(modelBuilder : ModelBuilder) =
         base.OnModelCreating(modelBuilder)
         modelBuilder.Entity<ArchivePContr223>().ToTable(String.Format("{0}arhiv_purchase_contracts223", S.Settings.Pref)) |> ignore
         ()
