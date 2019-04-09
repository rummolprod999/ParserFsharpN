namespace ParserFsharp

open Microsoft.EntityFrameworkCore
open System
open MySql.Data.EntityFrameworkCore.Extensions
open System.ComponentModel.DataAnnotations.Schema
open System.ComponentModel.DataAnnotations

[<Table("od_customer")>]
type Customer() =

        [<Key>]
        [<DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)>]
        [<property: Column("id")>]
        member val public Id = 0 with get, set

        [<property: Column("regNumber")>]
        member val public RegNumber = "" with get, set

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

        [<property: Column("full_name")>]
        member val public FullName = "" with get, set

        [<property: Column("postal_address")>]
        member val public PostalAddress = "" with get, set

        [<property: Column("phone")>]
        member val public Phone = "" with get, set

        [<property: Column("fax")>]
        member val public Fax = "" with get, set

        [<property: Column("email")>]
        member val public Email = "" with get, set

        [<property: Column("contact_name")>]
        member val public ContactName = "" with get, set

        [<property: Column("short_name")>]
        member val public ShortName = "" with get, set
