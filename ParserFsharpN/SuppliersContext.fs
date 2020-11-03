namespace ParserFsharp

open System.ComponentModel.DataAnnotations.Schema
open System.ComponentModel.DataAnnotations

[<AllowNullLiteral>]
[<Table("od_supplier")>]
type Supplier() =

        [<Key>]
        [<DatabaseGenerated(DatabaseGeneratedOption.Identity)>]
        [<property: Column("id")>]
        member val public Id = 0 with get, set

        [<property: Column("inn")>]
        member val public Inn = "" with get, set

        [<property: Column("kpp")>]
        member val public Kpp = "" with get, set

        [<property: Column("contracts_count")>]
        member val public ContractsCount = 0 with get, set

        [<property: Column("contracts223_count")>]
        member val public Contracts223Count = 0 with get, set

        [<property: Column("contracts_sum")>]
        member val public ContractsSum = 0m with get, set

        [<property: Column("contracts223_sum")>]
        member val public Contracts223Sum = 0m with get, set

        [<property: Column("ogrn")>]
        member val public Ogrn = "" with get, set

        [<property: Column("region_code")>]
        member val public RegionCode = "" with get, set

        [<property: Column("organizationName")>]
        member val public OrganizationName = "" with get, set

        [<property: Column("postal_address")>]
        member val public PostallAddress = "" with get, set

        [<property: Column("contactPhone")>]
        member val public ContactPhone = "" with get, set

        [<property: Column("contactFax")>]
        member val public ContactFax = "" with get, set

        [<property: Column("contactEMail")>]
        member val public ContactEmail = "" with get, set

        [<property: Column("contact_name")>]
        member val public ContactName = "" with get, set

        [<property: Column("organizationShortName")>]
        member val public OrganizationShortName = "" with get, set

        [<property: Column("all_names")>]
        member val public AllNames = "" with get, set
