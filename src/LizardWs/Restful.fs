namespace LizardWs
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Successful
open Suave.Filters
open System.IO
[<AutoOpen>]
module RestFul =
    type RestResource<'a> = {
        GetAll : unit -> 'a seq
        Create : 'a -> 'a
    }
    let fromJson<'a> json =
      JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

    let getResourceFromReq<'a> (req : HttpRequest) =
      let getString rawForm =
        System.Text.Encoding.UTF8.GetString(rawForm)
      req.rawForm |> getString |> fromJson<'a>
    // 'a -> WebPart
    let JSON v =
      let jsonSerializerSettings = new JsonSerializerSettings()
      jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
      JsonConvert.SerializeObject(v, jsonSerializerSettings)
      |> OK
      >=> Writers.setMimeType "application/json; charset=utf-8"

    //set up paths
    let opnfol = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Openings")
    let wfol = 
        let ans = Path.Combine(opnfol, "White")
        Directory.CreateDirectory(ans) |> ignore
        ans
    let bfol = 
        let ans = Path.Combine(opnfol, "Black")
        Directory.CreateDirectory(ans) |> ignore
        ans
    ///wvars gets list of white varns
    let wvars() = 
        Directory.GetFiles(wfol, "*.json")
        |> Array.map Path.GetFileNameWithoutExtension
        |> List.ofArray
    ///bvars gets list of black varns
    let bvars() = 
        Directory.GetFiles(bfol, "*.json")
        |> Array.map Path.GetFileNameWithoutExtension
        |> List.ofArray
    let restchess resourceName =
      let resourcePath = "/" + resourceName
      let getvars = 
        if resourceName="wvars"then warbler (fun _ -> wvars() |> JSON)
        elif resourceName="bvars"then warbler (fun _ -> bvars() |> JSON)
        else failwith "unsupported"
      path resourcePath >=> choose [
        GET >=> getvars
        
      ]