namespace Scenario


module XmlToJson = 
    
    open System
    open System.IO
    open System.Text
    open FSharp.Json
    open FSharp.Data
    
    // ---------------------------------
    // Models
    // ---------------------------------
    
    type ScenarioXml = XmlProvider<"sampleScenario.xml">
    
    type VehicleType =
        | Engine
        | Wagon
        
    type Vehicle =
        { Name: string
          Provider: string
          Product: string
          Path: string
          IsStatic: bool
          Type: VehicleType }
          
    type ScenarioInfo = {
        Vehicles: Vehicle list
    }    
        
    // ---------------------------------
    // Views
    // ---------------------------------
    
    
    let railVehicleToVehicle (activeVehicles : string list) (rv: ScenarioXml.COwnedEntity)  : Vehicle =
        { Name = rv.Name.Value
          Provider = rv
                         .BlueprintId
                         .IBlueprintLibraryCAbsoluteBlueprintId
                         .BlueprintSetId
                         .IBlueprintLibraryCBlueprintSetId
                         .Provider
                         .Value
          Product = rv
                         .BlueprintId
                         .IBlueprintLibraryCAbsoluteBlueprintId
                         .BlueprintSetId
                         .IBlueprintLibraryCBlueprintSetId
                         .Product
                         .Value
          Path = rv.BlueprintId.IBlueprintLibraryCAbsoluteBlueprintId.BlueprintId.Value
          IsStatic = not (List.contains rv.BlueprintId.IBlueprintLibraryCAbsoluteBlueprintId.BlueprintId.Value activeVehicles)
          Type =
            match rv.Component.CEngine with
            | Some _ -> Engine
            | None _ -> Wagon }
    
    let getScenarioObjects(xml: string) =
        let scenario = ScenarioXml.Parse(xml)
        let activeVehicles =
            scenario.Record.CConsists
            |> Array.filter (fun c -> Option.isSome c.Driver.CDriver)
            |> Array.collect (fun c -> c.RailVehicles.COwnedEntities)
            |> Array.map (fun rv -> rv.BlueprintId.IBlueprintLibraryCAbsoluteBlueprintId.BlueprintId.Value)
            |> Array.toList
        
        let vehicles =
            scenario.Record.CConsists
            |> Array.collect (fun c -> c.RailVehicles.COwnedEntities)
            |> Array.map (railVehicleToVehicle activeVehicles)
            |> Array.distinct
            |> Array.sortByDescending (fun v -> v.IsStatic)
            |> Array.toList
        
        let result = { Vehicles = vehicles }
        
        result
        |> Json.serialize