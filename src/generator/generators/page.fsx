#r "nuget: Fornax.Core, 0.15.1"
#load "layout.fsx"

open Html
open System.IO

let generate' (ctx : SiteContents) (page: string) =
    printfn "Generating page: %s" page
    
    // Debug: print all available pages
    let pages = ctx.TryGetValues<Layout.Page>() |> Option.defaultValue Seq.empty
    printfn "Available pages: %A" (pages |> Seq.map (fun p -> p.file) |> Seq.toArray)
    
    // Use exact path matching
    let pageOption = pages |> Seq.tryFind (fun p -> p.file = page)
    
    match pageOption with
    // Rest remains the same
    | Some pageData ->
        // Rest of the function remains the same
        // ...
        // Found the page, generate it
        let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
        let desc =
            siteInfo
            |> Option.map (fun si -> si.description)
            |> Option.defaultValue ""
        
        Layout.layout ctx pageData.title [
            section [Class "hero bg-primary text-primary-content py-24"] [
                div [Class "hero-content text-center"] [
                    div [Class "max-w-md"] [
                        h1 [Class "text-4xl font-bold accent"] [!!desc]
                    ]
                ]
            ]
            div [Class "container mx-auto px-4"] [
                section [Class "py-8"] [
                    div [Class "max-w-3xl mx-auto"] [
                        div [Class "card bg-base-100 shadow-xl"] [
                            div [Class "card-body"] [
                                div [Class "prose lg:prose-lg"] [
                                    !! pageData.content
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    | None ->
        // Critical error - page not found
        // This will halt the Fornax process
        failwithf "ERROR: Page '%s' not found in site contents. Available pages: %A" 
            page 
            (pages |> Seq.map (fun p -> p.file) |> Seq.toArray)

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    try
        let content = generate' ctx page
        Layout.render ctx content
    with ex ->
        // Critical error - halt the process
        printfn "CRITICAL ERROR in page generator: %s" ex.Message
        printfn "Page: %s" page
        printfn "Stack trace: %s" ex.StackTrace
        failwith ex.Message  // Re-throw to ensure Fornax stops