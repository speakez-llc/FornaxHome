#r "nuget: Fornax.Core, 0.15.1"
#load "layout.fsx"

open Html

let generate' (ctx : SiteContents) (page: string) =
    // Print debugging information
    printfn "Generating page: %s" page
    
    // Find the page in the site contents
    let pageOption = 
        ctx.TryGetValues<Layout.Page>() 
        |> Option.defaultValue Seq.empty
        |> Seq.tryFind (fun p -> p.file = page)
    
    match pageOption with
    | Some pageData ->
        // Get site info for the header
        let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
        let desc =
            siteInfo
            |> Option.map (fun si -> si.description)
            |> Option.defaultValue ""
        
        // Generate HTML using the layout
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
        // Page not found
        printfn "Warning: Page '%s' not found in site contents" page
        Layout.layout ctx "Page Not Found" [
            div [Class "container mx-auto px-4 py-8"] [
                div [Class "card bg-warning text-warning-content max-w-md mx-auto"] [
                    div [Class "card-body"] [
                        h2 [Class "card-title"] [!!"Page Not Found"]
                        p [] [!!(sprintf "The page '%s' could not be found." page)]
                        a [Class "btn"; Href "/"] [!!"Return Home"]
                    ]
                ]
            ]
        ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    printfn "Page generator called for: %s" page
    try
        let rendered = generate' ctx page
        Layout.render ctx rendered
    with ex ->
        printfn "Error in page generator: %s" ex.Message
        
        // Fallback to a simple error page
        let errorPage = Layout.layout ctx "Error" [
            div [Class "container mx-auto px-4 py-8"] [
                div [Class "card bg-error text-error-content max-w-md mx-auto"] [
                    div [Class "card-body"] [
                        h2 [Class "card-title"] [!!"Error Generating Page"]
                        p [] [!!(sprintf "Error: %s" ex.Message)]
                    ]
                ]
            ]
        ]
        
        Layout.render ctx errorPage