namespace ParserFsharp
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open NewtonExt
open Microsoft.EntityFrameworkCore
open System.Linq
open System.Collections.Generic

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
                            let cus = db.customers.FirstOrDefault(fun x -> x.Inn = innCus && x.Kpp = kppCus)
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
                    let nameSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.name"
                    let kppSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.kpp"
                    let ogrnSup = GetStringFromJtoken __.item "purchaseContractData.supplier.mainInfo.ogrn"
                    if innSup <> "" || nameSup <> "" then
                                                let sup = db.suppliers.FirstOrDefault(fun x -> x.Inn = innSup && x.Kpp = kppSup)
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
                    let url = GetStringFromJtoken __.item "purchaseContractData.urlEIS"
                    let contrCreateDateT = GetDateTimeStringFromJtoken __.item "purchaseContractData.contractCreateDate"
                    let contrCreateDate = contrCreateDateT.DateFromStringRus("yyyy-MM-dd")
                    let createDateT = GetDateTimeStringFromJtoken __.item "purchaseContractData.createDateTime"
                    let createDate = createDateT.Replace("T", " ").DateFromStringRus("yyyy-MM-dd HH:mm:ss")
                    let notificationNumber = GetStringFromJtoken __.item "purchaseContractData.purchaseInfo.purchaseNoticeNumber"
                    let contractPrice = GetDecimalFromJtoken __.item "purchaseContractData.sum"
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
                    return ""
                    }
                match res with
                | Success r -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
                ()
