namespace ParserFsharp

open Microsoft.EntityFrameworkCore
open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations.Schema
open System.ComponentModel.DataAnnotations

[<AllowNullLiteral>]
type PContr223() =
        [<Key>]
        [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
        [<property: Column("id")>]
        member val public Id = 0 with get, set
        
        [<property: Column("guid")>]
        member val public Guid = "" with get, set
        
        [<property: Column("regnum")>]
        member val public RegNum = "" with get, set
        
        [<property: Column("current_contract_stage")>]
        member val public CurrentContractStage = "" with get, set
        
        [<property: Column("region_code")>]
        member val public RegionCode = "" with get, set
        
        [<property: Column("url")>]
        member val public Url = "" with get, set
        
        [<property: Column("contr_create_date")>]
        member val public ContrCreateDate = DateTime.MinValue with get, set
        
        [<property: Column("create_date")>]
        member val public CreateDate = DateTime.MinValue with get, set
        
        [<property: Column("notification_number")>]
        member val public NotificationNumber = "" with get, set
        
        [<property: Column("contract_price")>]
        member val public ContractPrice = 0m with get, set
        
        [<property: Column("currency")>]
        member val public Currency = "" with get, set
        
        [<property: Column("version_number")>]
        member val public VersionNumber = 0 with get, set
        
        [<property: Column("fulfillment_date")>]
        member val public FulFillmentDate = "" with get, set
        
        [<Column("id_customer")>]
        member val public CustomerId = 0 with get, set
        
        [<DefaultValue>]
        val mutable customer : Customer
        member x.Customer
            with get () = x.customer
            and set v = x.customer <- v
        
        [<Column("id_supplier")>]
        member val public SupplierId = 0 with get, set
        
        [<DefaultValue>]
        val mutable supplier : Supplier
        member x.Supplier
            with get () = x.supplier
            and set v = x.supplier <- v
            
        [<property: Column("cancel")>]
        member val public Cancel = 0 with get, set
        
        [<property: Column("xml")>]
        member val public Xml = "" with get, set
        
        [<property: Column("address")>]
        member val public Address = "" with get, set
        
        [<DefaultValue>]
        val mutable products : ICollection<Contr223Prod> 
        member x.Products
            with get () = x.products
            and set v = x.products <- v
        

and  Contr223Prod() =
        [<Key>]
        [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
        [<property: Column("id")>]
        member val public Id = 0 with get, set
        
        [<DefaultValue>]
        val mutable  pContr223Id : int
        
        [<property: Column("id_purchase_contract")>]
        member x.PContr223Id
            with get () = x.pContr223Id
            and set v = x.pContr223Id <- v
        
        [<DefaultValue>]
        val mutable contract : PContr223
        member x.Contract
            with get () = x.contract
            and set v = x.contract <- v
            
        [<property: Column("name")>]
        member val public Name = "" with get, set
        
        [<property: Column("okpd_code")>]
        member val public OkpdCode = "" with get, set
        
        [<property: Column("okpd_name")>]
        member val public OkpdName = "" with get, set
        
        [<property: Column("okved_code")>]
        member val public OkvedCode = "" with get, set
        
        [<property: Column("okved_name")>]
        member val public OkvedName = "" with get, set
        
        [<property: Column("quantity")>]
        member val public Quantity = 0m with get, set
        
        [<property: Column("okei")>]
        member val public Okei = "" with get, set



type PContr223Context() =
    inherit DbContext()

    [<DefaultValue>] val mutable contr223 : DbSet<PContr223>
    member x.Contr223
        with get () = x.contr223
        and set v = x.contr223 <- v
        
    [<DefaultValue>] val mutable products : DbSet<Contr223Prod>
    member x.Products
        with get () = x.products
        and set v = x.products <- v
    
    [<DefaultValue>] val mutable customers : DbSet<Customer>
    member x.Customers
        with get () = x.customers
        and set v = x.customers <- v
        
    [<DefaultValue>] val mutable suppliers : DbSet<Supplier>
    member x.Suppliers
        with get () = x.suppliers
        and set v = x.suppliers <- v
        
    override __.OnConfiguring(optbuilder : DbContextOptionsBuilder) =
        optbuilder.UseMySQL(S.Settings.ConS) |> ignore
        ()

    override __.OnModelCreating(modelBuilder : ModelBuilder) =
         base.OnModelCreating(modelBuilder)
         modelBuilder.Entity<PContr223>().ToTable(String.Format("{0}purchase_contracts223", S.Settings.Pref)) |> ignore
         modelBuilder.Entity<Contr223Prod>().ToTable(String.Format("{0}purchase_products223", S.Settings.Pref)) |> ignore
         ()

