namespace ParserFsharp
open System
open System
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open NewtonExt
open Microsoft.EntityFrameworkCore
open System.Linq
open System.Collections.Generic
open MySql.Data.MySqlClient
open System.Data

type DocumentPcontr223() =
      static member val AddProd : int = 0 with get, set
      [<DefaultValue>] val mutable file : FileInfo
      [<DefaultValue>] val mutable item : JToken
      [<DefaultValue>] val mutable region : Region
      new(f, i, r) as this = DocumentPcontr223()
                             then
                                 this.file <- f
                                 this.item <- i
                                 this.region <- r
      inherit AbstractDocumentFtpEis()
      interface IDocument with
        override __.Worker() =
          let guid = GetStringFromJtoken __.item "guid"
          let regNum = GetStringFromJtoken __.item "purchaseContractData.registrationNumber"
          if guid = "" || regNum = "" then failwith <| sprintf "guid or regnum not found %s" __.file.Name
          let version = match __.item.SelectToken("purchaseContractData.version") with
                        | null -> 1
                        | x -> (int) x
          let mutable cancel = 0
          let mutable updated = false
          use con = new MySqlConnection(S.Settings.ConS)
          con.Open()
          let selectDoc =
            sprintf "SELECT id FROM %spurchase_contracts223 WHERE regnum = @regnum AND version_number = @version_number" S.Settings.Pref
          let cmd : MySqlCommand = new MySqlCommand(selectDoc, con)
          cmd.Prepare()
          cmd.Parameters.AddWithValue("@regnum", regNum) |> ignore
          cmd.Parameters.AddWithValue("@version_number", version) |> ignore
          let reader : MySqlDataReader = cmd.ExecuteReader()
          if reader.HasRows then reader.Close()
          else
              reader.Close()
              let mutable maxNum = 0
              let selectMax = sprintf "SELECT IFNULL(MAX(version_number), 0) AS m FROM %spurchase_contracts223 WHERE regnum = @regnum" S.Settings.Pref
              let cmd1 : MySqlCommand = new MySqlCommand(selectMax, con)
              cmd1.Prepare()
              cmd1.Parameters.AddWithValue("@regnum", regNum) |> ignore
              match cmd1.ExecuteReader() with
              | x when x.HasRows -> x.Read() |> ignore
                                    maxNum <- x.GetInt32("m")
                                    x.Close()
              | y -> y.Close()
              if maxNum <> 0 then updated <- true
              if version > maxNum then
                    let selectDocs = sprintf "SELECT id FROM %spurchase_contracts223 WHERE regnum = @regnum" S.Settings.Pref
                    let cmd2 : MySqlCommand = new MySqlCommand(selectDocs, con)
                    cmd2.Prepare()
                    cmd2.Parameters.AddWithValue("@regnum", regNum) |> ignore
                    let adapter = new MySqlDataAdapter()
                    adapter.SelectCommand <- cmd2
                    let dt = new DataTable()
                    adapter.Fill(dt) |> ignore
                    for row in dt.Rows do
                        row.["cancel"] <- 1
                    let commandBuilder = new MySqlCommandBuilder(adapter)
                    commandBuilder.ConflictOption <- ConflictOption.OverwriteChanges
                    adapter.Update(dt) |> ignore
              else cancel <- 1
              let idCustomer = ref 0
              let innCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.inn"
              let kppCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.kpp"
              let fullNameCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.fullName"
              if innCus <> "" || fullNameCus <> "" then
                let selectCustomer = "SELECT id  FROM od_customer WHERE inn = @inn AND kpp = @kpp AND full_name = @full_name"
                let cmd3 = new MySqlCommand(selectCustomer, con)
                cmd3.Prepare()
                cmd3.Parameters.AddWithValue("@inn", innCus) |> ignore
                cmd3.Parameters.AddWithValue("@kpp", kppCus) |> ignore
                cmd3.Parameters.AddWithValue("@full_name", fullNameCus) |> ignore
                let reader = cmd3.ExecuteReader()
                match reader.HasRows with
                | true ->
                    reader.Read() |> ignore
                    idCustomer := reader.GetInt32("id")
                    reader.Close()
                | false ->
                    reader.Close()
                    let ogrnCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.ogrn"
                    let postalAddressCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.postalAddress"
                    let phoneCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.phone"
                    let emailCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.email"
                    let shortNameCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.shortName"
                    let addCustomer = "INSERT INTO od_customer SET inn = @inn, kpp = @kpp, full_name = @full_name, ogrn = @ogrn, postal_address = @postal_address, phone = @phone, email = @email, short_name = @short_name"
                    let cmd5 = new MySqlCommand(addCustomer, con)
                    cmd5.Prepare()
                    cmd5.Parameters.AddWithValue("@inn", innCus) |> ignore
                    cmd5.Parameters.AddWithValue("@kpp", kppCus) |> ignore
                    cmd5.Parameters.AddWithValue("@full_name", fullNameCus) |> ignore
                    cmd5.Parameters.AddWithValue("@ogrn", ogrnCus) |> ignore
                    cmd5.Parameters.AddWithValue("@postal_address", postalAddressCus) |> ignore
                    cmd5.Parameters.AddWithValue("@phone", phoneCus) |> ignore
                    cmd5.Parameters.AddWithValue("@email", emailCus) |> ignore
                    cmd5.Parameters.AddWithValue("@short_name", shortNameCus) |> ignore
                    cmd5.ExecuteNonQuery() |> ignore
                    idCustomer := int cmd5.LastInsertedId
              let idSupplier = ref 0
              let innSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.inn"
              let mutable nameSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.name"
              if nameSup = "" then nameSup <- GetStringFromJtoken __.item "purchaseContractData.nonResidentInfo.info"
              let nsUp = nameSup
              let kppSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.kpp"
              let ogrnSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.ogrn"
              if innSup <> "" || nameSup <> "" then
                  let selectSupplier = "SELECT id  FROM od_supplier WHERE inn = @inn AND kpp = @kpp AND organizationName = @organizationName"
                  let cmd3 = new MySqlCommand(selectSupplier, con)
                  cmd3.Prepare()
                  cmd3.Parameters.AddWithValue("@inn", innSup) |> ignore
                  cmd3.Parameters.AddWithValue("@kpp", kppSup) |> ignore
                  cmd3.Parameters.AddWithValue("@organizationName", nsUp) |> ignore
                  let reader = cmd3.ExecuteReader()
                  match reader.HasRows with
                  | true ->
                    reader.Read() |> ignore
                    idCustomer := reader.GetInt32("id")
                    reader.Close()
                  | false ->
                    reader.Close()
                    let addSupplier = "INSERT INTO od_supplier SET inn = @inn, kpp = @kpp, organizationName = @organizationName, ogrn = @ogrn"
                    let cmd5 = new MySqlCommand(addSupplier, con)
                    cmd5.Prepare()
                    cmd5.Parameters.AddWithValue("@inn", innSup) |> ignore
                    cmd5.Parameters.AddWithValue("@kpp", kppSup) |> ignore
                    cmd5.Parameters.AddWithValue("@organizationName", nsUp) |> ignore
                    cmd5.Parameters.AddWithValue("@ogrn", ogrnSup) |> ignore
                    cmd5.ExecuteNonQuery() |> ignore
                    idSupplier := int cmd5.LastInsertedId
              let currentContractStage = GetStringFromJtoken __.item "purchaseContractData.status"
              let regionCode = __.region.Conf
              let mutable url = GetStringFromJtoken __.item "purchaseContractData.urlEIS"
              if url = "" then url <- GetStringFromJtoken __.item "purchaseContractData.urlOOS"
              let contrCreateDateT = GetDateTimeStringFromJtoken __.item "purchaseContractData.contractCreateDate"
              let contrCreateDate = contrCreateDateT.DateFromStringRus("yyyy-MM-dd")
              let createDateT = GetDateTimeStringFromJtoken __.item "purchaseContractData.createDateTime"
              let createDate = createDateT.Replace("T", " ").DateFromStringRus("yyyy-MM-dd HH:mm:ss")
              let notificationNumber = GetStringFromJtoken __.item "purchaseContractData.purchaseInfo.purchaseNoticeNumber"
              let contractPriceT = GetStringFromJtoken __.item "purchaseContractData.sum"
              let contractPrice = match Decimal.TryParse(contractPriceT.GetPriceFromString()) with
                                  | (true, y) -> y
                                  | _ -> 0m
              let currency = GetStringFromJtoken __.item "purchaseContractData.currency.code"
              let fulFillmentDate = GetStringFromJtoken __.item "purchaseContractData.fulfillmentDate"
              let xml = __.GetXml(__.file.FullName)
              let address = GetStringFromJtoken __.item "purchaseContractData.deliveryPlace.address"
              let idContract = ref 0
              let insertPcontr =
                String.Format ("INSERT INTO {0}purchase_contracts223 SET guid = @guid, regnum = @regnum, current_contract_stage = @current_contract_stage, region_code = @region_code, url = @url, contr_create_date = @contr_create_date, create_date = @create_date, notification_number = @notification_number, contract_price = @contract_price, currency = @currency, version_number = @version_number, fulfillment_date = @fulfillment_date, id_customer = @id_customer, id_supplier = @id_supplier, cancel = @cancel, xml = @xml, address = @address", S.Settings.Pref)
              let cmd9 = new MySqlCommand(insertPcontr, con)
              cmd9.Prepare()
              cmd9.Parameters.AddWithValue("@guid", guid) |> ignore
              cmd9.Parameters.AddWithValue("@regnum", regNum) |> ignore
              cmd9.Parameters.AddWithValue("@current_contract_stage", currentContractStage) |> ignore
              cmd9.Parameters.AddWithValue("@region_code", regionCode) |> ignore
              cmd9.Parameters.AddWithValue("@url", url) |> ignore
              cmd9.Parameters.AddWithValue("@contr_create_date", contrCreateDate) |> ignore
              cmd9.Parameters.AddWithValue("@create_date", createDate) |> ignore
              cmd9.Parameters.AddWithValue("@notification_number", notificationNumber) |> ignore
              cmd9.Parameters.AddWithValue("@contract_price", contractPrice) |> ignore
              cmd9.Parameters.AddWithValue("@currency", currency) |> ignore
              cmd9.Parameters.AddWithValue("@version_number", version) |> ignore
              cmd9.Parameters.AddWithValue("@fulfillment_date", fulFillmentDate) |> ignore
              cmd9.Parameters.AddWithValue("@id_customer", !idCustomer) |> ignore
              cmd9.Parameters.AddWithValue("@id_supplier", !idSupplier) |> ignore
              cmd9.Parameters.AddWithValue("@cancel", cancel) |> ignore
              cmd9.Parameters.AddWithValue("@xml", xml) |> ignore
              cmd9.Parameters.AddWithValue("@address", address) |> ignore
              cmd9.ExecuteNonQuery() |> ignore
              idContract := int cmd9.LastInsertedId
              match updated with
              | true -> AbstractDocumentFtpEis.Upd <- AbstractDocumentFtpEis.Upd + 1
              | false -> AbstractDocumentFtpEis.Add <- AbstractDocumentFtpEis.Add + 1
              let items = __.item.GetElements("purchaseContractData.contractItems.contractItem")
              for item in items do
                let mutable name = GetStringFromJtoken item "additionalInfo"
                let okpdCode = GetStringFromJtoken item "okdp.code"
                let okpdName = GetStringFromJtoken item "okdp.name"
                if name = "" then name <- okpdName
                let okvedCode = GetStringFromJtoken item "okved.code"
                let okvedName = GetStringFromJtoken item "okved.name"
                let qT = GetStringFromJtoken item "qty"
                let qty = match Decimal.TryParse(qT.GetPriceFromString()) with
                          | (true, y) -> y
                          | _ -> 0m
                let okei = GetStringFromJtoken item "okei.name"
                let insertProduct = String.Format ("INSERT INTO {0}purchase_products223 SET  id_purchase_contract = @id_purchase_contract, name = @name, okpd_code = @okpd_code, okpd_name = @okpd_name, okved_code = @okved_code, okved_name = @okved_name, quantity = @quantity, okei = @okei", S.Settings.Pref)
                let cmd10 = new MySqlCommand(insertProduct, con)
                cmd10.Prepare()
                cmd10.Parameters.AddWithValue("@id_purchase_contract", !idContract) |> ignore
                cmd10.Parameters.AddWithValue("@name", name) |> ignore
                cmd10.Parameters.AddWithValue("@okpd_code", okpdCode) |> ignore
                cmd10.Parameters.AddWithValue("@okpd_name", okpdName) |> ignore
                cmd10.Parameters.AddWithValue("@okved_code", okvedCode) |> ignore
                cmd10.Parameters.AddWithValue("@okved_name", okvedName) |> ignore
                cmd10.Parameters.AddWithValue("@quantity", qty) |> ignore
                cmd10.Parameters.AddWithValue("@okei", okei) |> ignore
                cmd10.ExecuteNonQuery() |> ignore
                ()
              ()
          ()


      member private __.ReturnItems(contr : PContr223) =
          let ar = new List<Contr223Prod>()
          let items = __.item.GetElements("purchaseContractData.contractItems.contractItem")
          for item in items do
              let mutable name = GetStringFromJtoken item "additionalInfo"
              let okpdCode = GetStringFromJtoken item "okdp.code"
              let okpdName = GetStringFromJtoken item "okdp.name"
              if name = "" then name <- okpdName
              let okvedCode = GetStringFromJtoken item "okved.code"
              let okvedName = GetStringFromJtoken item "okved.name"
              let qT = GetStringFromJtoken item "qty"
              let qty = match Decimal.TryParse(qT.GetPriceFromString()) with
                                        | (true, y) -> y
                                        | _ -> 0m
              let okei = GetStringFromJtoken item "okei.name"
              let product = Contr223Prod()
              product.Contract <- contr
              product.Name <- name
              product.OkpdCode <- okpdCode
              product.OkpdName <- okpdName
              product.OkvedCode <- okvedCode
              product.OkvedName <- okvedName
              product.Quantity <- qty
              product.Okei <- okei
              ar.Add(product)
              DocumentPcontr223.AddProd <- DocumentPcontr223.AddProd + 1
              ()
          ar

      member __.WorkerOld() =
                let builder = DocumentBuilder()
                use db = new PContr223Context()
                let res =
                   builder {
                    let! guid = __.item.StDString "guid" <| sprintf "guid not found %s" __.file.FullName
                    let! regNum = __.item.StDString "purchaseContractData.registrationNumber" <| sprintf "registrationNumber not found %s" __.file.FullName
                    let version = match __.item.SelectToken("purchaseContractData.version") with
                                  | null -> 1
                                  | x -> (int) x
                    let mutable cancel = 0
                    let mutable updated = false
                    let exist = db.Contr223.Where(fun x -> x.RegNum = regNum && x.VersionNumber = version).Select(fun x -> x.Id) .Count()
                    if exist > 0 then return! Error ""
                    let maxNum = db.Contr223.Where(fun x -> x.RegNum = regNum).Select(fun x -> x.VersionNumber).DefaultIfEmpty(0) .Max()
                    if maxNum <> 0 then updated <- true
                    if version > maxNum then
                            let contracts = db.Contr223.Where(fun x -> x.RegNum = regNum)
                            contracts |> Seq.iter (fun (x : PContr223) -> x.Cancel <- 1; db.Entry(x).State <- EntityState.Modified)
                            db.SaveChanges() |> ignore
                    else cancel <- 1
                    let mutable customer : Option<Customer> = None
                    let innCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.inn"
                    let kppCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.kpp"
                    let fullNameCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.fullName"
                    if innCus <> "" || fullNameCus <> "" then
                            let cus = db.customers.FirstOrDefault (fun x -> x.Inn = innCus && x.Kpp = kppCus && x.FullName = fullNameCus)
                            if cus <> null then customer <- Some cus
                            else
                                let ogrnCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.ogrn"
                                let postalAddressCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.postalAddress"
                                let phoneCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.phone"
                                let emailCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.email"
                                let shortNameCus = GetStringFromJtoken __.item "purchaseContractData.customerInfo.shortName"
                                let cusC = Customer()
                                cusC.RegNumber <- ""
                                cusC.Inn <- innCus
                                cusC.Kpp <- kppCus
                                cusC.ContractsCount <- 0
                                cusC.Contracts223Count <- 0
                                cusC.ContractsSum <- 0m
                                cusC.Contracts223Sum <- 0m
                                cusC.Ogrn <- ogrnCus
                                cusC.RegionCode <- ""
                                cusC.FullName <- fullNameCus
                                cusC.PostalAddress <- postalAddressCus
                                cusC.Phone <- phoneCus
                                cusC.Fax <- ""
                                cusC.Email <- emailCus
                                cusC.ContactName <- ""
                                cusC.ShortName <- shortNameCus
                                customer <- Some cusC
                                match customer with
                                | None -> ()
                                | Some cc ->
                                            db.Customers.Add(cc) |> ignore
                                            db.SaveChanges() |> ignore
                                ()
                    let mutable supplier : Option<Supplier> = None
                    let innSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.inn"
                    let mutable nameSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.name"
                    if nameSup = "" then nameSup <- GetStringFromJtoken __.item "purchaseContractData.nonResidentInfo.info"
                    let nsUp = nameSup
                    let kppSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.kpp"
                    let ogrnSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.ogrn"
                    if innSup <> "" || nameSup <> "" then
                                                let sup = db.suppliers.FirstOrDefault (fun x -> x.Inn = innSup && x.Kpp = kppSup && x.OrganizationName = nsUp)
                                                if sup <> null then supplier <- Some sup
                                                else
                                                    let supC = Supplier()
                                                    supC.Inn <- innSup
                                                    supC.Kpp <- kppSup
                                                    supC.ContractsCount <- 0
                                                    supC.Contracts223Count <- 0
                                                    supC.ContractsSum <- 0m
                                                    supC.Contracts223Sum <- 0m
                                                    supC.Ogrn <- ogrnSup
                                                    supC.RegionCode <- ""
                                                    supC.OrganizationName <- nameSup
                                                    supC.PostallAddress <- ""
                                                    supC.ContactPhone <- ""
                                                    supC.ContactFax <- ""
                                                    supC.ContactEmail <- ""
                                                    supC.ContactName <- ""
                                                    supC.OrganizationShortName <- ""
                                                    supC.AllNames <- ""
                                                    supplier <- Some supC
                                                    match supplier with
                                                    | None -> ()
                                                    | Some ss ->
                                                        db.Suppliers.Add(ss) |> ignore
                                                        db.SaveChanges() |> ignore
                    let currentContractStage = GetStringFromJtoken __.item "purchaseContractData.status"
                    let regionCode = __.region.Conf
                    let mutable url = GetStringFromJtoken __.item "purchaseContractData.urlEIS"
                    if url = "" then url <- GetStringFromJtoken __.item "purchaseContractData.urlOOS"
                    let contrCreateDateT = GetDateTimeStringFromJtoken __.item "purchaseContractData.contractCreateDate"
                    let contrCreateDate = contrCreateDateT.DateFromStringRus("yyyy-MM-dd")
                    let createDateT = GetDateTimeStringFromJtoken __.item "purchaseContractData.createDateTime"
                    let createDate = createDateT.Replace("T", " ").DateFromStringRus("yyyy-MM-dd HH:mm:ss")
                    let notificationNumber = GetStringFromJtoken __.item "purchaseContractData.purchaseInfo.purchaseNoticeNumber"
                    let contractPriceT = GetStringFromJtoken __.item "purchaseContractData.sum"
                    let contractPrice = match Decimal.TryParse(contractPriceT.GetPriceFromString()) with
                                        | (true, y) -> y
                                        | _ -> 0m
                    let currency = GetStringFromJtoken __.item "purchaseContractData.currency.code"
                    let fulFillmentDate = GetStringFromJtoken __.item "purchaseContractData.fulfillmentDate"
                    let xml = __.GetXml(__.file.FullName)
                    let address = GetStringFromJtoken __.item "purchaseContractData.deliveryPlace.address"
                    let contract = PContr223()
                    contract.Guid <- guid
                    contract.RegNum <- regNum
                    contract.CurrentContractStage <- currentContractStage
                    contract.RegionCode <- regionCode
                    contract.Url <- url
                    contract.ContrCreateDate <- contrCreateDate
                    contract.CreateDate <- createDate
                    contract.NotificationNumber <- notificationNumber
                    contract.ContractPrice <- contractPrice
                    contract.Currency <- currency
                    contract.VersionNumber <- version
                    contract.FulFillmentDate <- fulFillmentDate
                    match customer with
                    | None -> contract.CustomerId <- 0
                    | Some c -> contract.Customer <- c
                    match supplier with
                    | None -> contract.SupplierId <- 0
                    | Some s -> contract.Supplier <- s
                    contract.Cancel <- cancel
                    contract.Xml <- xml
                    contract.Address <- address
                    db.Contr223.Add(contract) |> ignore
                    db.SaveChanges() |> ignore
                    match updated with
                    | true -> AbstractDocumentFtpEis.Upd <- AbstractDocumentFtpEis.Upd + 1
                    | false -> AbstractDocumentFtpEis.Add <- AbstractDocumentFtpEis.Add + 1
                    let products = __.ReturnItems(contract)
                    db.Products.AddRange(products)
                    db.SaveChanges() |> ignore
                    return ""
                    }
                match res with
                | Success r -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
                ()

