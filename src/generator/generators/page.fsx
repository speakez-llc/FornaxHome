#r "nuget: Fornax.Core, 0.15.1"
#load "layout.fsx"

open Html
open System.IO

let generate' (ctx : SiteContents) (page: string) =
    printfn "Generating page: %s" page
    
    // Get all pages
    let pages = ctx.TryGetValues<Layout.Page>() |> Option.defaultValue Seq.empty
    
    // Try to find the page by exact path first, then by filename
    let pageOption = 
        pages |> Seq.tryFind (fun p -> 
            p.file = page || 
            Path.GetFileName(p.file) = Path.GetFileName(page))
    
    match pageOption with
    | Some pageData ->
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
        // Page not found
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
    let content = generate' ctx page
    Layout.render ctx content