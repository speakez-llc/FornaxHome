open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Oxpecker

let webRoot = Path.Combine(Directory.GetCurrentDirectory(), "D:/repos/FornaxHome/src/generator/_public")

// Helper to determine if a file is binary
let isBinaryFile (extension: string) =
    match extension.ToLowerInvariant() with
    | ".png" | ".jpg" | ".jpeg" | ".gif" | ".ico" | ".pdf" | ".svg" -> true
    | _ -> false

// Properly typed as EndpointHandler
let staticFileHandler (filePath: string) : EndpointHandler =
    fun (ctx: HttpContext) ->
        task {
            let fullPath = Path.Combine(webRoot, filePath.TrimStart('/'))
            if File.Exists(fullPath) then
                // Determine content type based on file extension
                let extension = Path.GetExtension(filePath)
                let contentType = 
                    match extension.ToLowerInvariant() with
                    | ".html" -> "text/html"
                    | ".css" -> "text/css"
                    | ".js" -> "application/javascript"
                    | ".json" -> "application/json"
                    | ".png" -> "image/png"
                    | ".jpg" | ".jpeg" -> "image/jpeg"
                    | ".gif" -> "image/gif"
                    | ".svg" -> "image/svg+xml"
                    | ".ico" -> "image/x-icon"
                    | ".pdf" -> "application/pdf"
                    | _ -> "text/plain"
                
                ctx.Response.ContentType <- contentType
                ctx.Response.StatusCode <- 200
                
                // Handle binary files differently from text files
                if isBinaryFile extension then
                    let! bytes = File.ReadAllBytesAsync(fullPath)
                    do! ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length)
                else
                    let! content = File.ReadAllTextAsync(fullPath)
                    do! ctx.Response.WriteAsync(content)
            else
                ctx.Response.StatusCode <- 404
                do! ctx.Response.WriteAsync($"File not found: {filePath}")
        }

let fileByPathHandler (ctx: HttpContext) : Task =
    let path = ctx.Request.RouteValues.["path"].ToString()
    staticFileHandler path ctx

let configureServices (services: IServiceCollection) =
    services.AddOxpecker() |> ignore

let configureApp (app: IApplicationBuilder) =
    app.UseRouting() |> ignore
    app.UseOxpecker([
        GET [
            route "/" (staticFileHandler "index.html")
            route "/about.html" (staticFileHandler "about.html")
            route "/contact.html" (staticFileHandler "contact.html")
            route "/data-room/login.html" (staticFileHandler "data-room/login.html")
            routef "/{*path}" fileByPathHandler
        ]
    ]) |> ignore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    configureServices builder.Services
    
    let app = builder.Build()
    configureApp app
    
    app.Run()
    0