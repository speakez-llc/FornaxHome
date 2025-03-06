#r "nuget: Fornax.Core, 0.15.1"
#load "layout.fsx"

open Html
open System.IO

let generate' (ctx : SiteContents) (page: string) =
    let postOption =
        ctx.TryGetValues<Postloader.Post> ()
        |> Option.defaultValue Seq.empty
        |> Seq.tryFind (fun n -> n.file = page)

    match postOption with
    | Some post ->
        let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
        let desc =
            siteInfo
            |> Option.map (fun si -> si.description)
            |> Option.defaultValue ""

        Layout.layout ctx post.title [
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
                        Layout.postLayout false post
                    ]
                ]
            ]
        ]
    | None ->
        printfn "Warning: Post '%s' not found" page
        Layout.layout ctx "Post Not Found" [
            div [Class "container mx-auto px-4 py-8"] [
                div [Class "card bg-warning text-warning-content max-w-md mx-auto"] [
                    div [Class "card-body"] [
                        h2 [Class "card-title"] [!!"Post Not Found"]
                        p [] [!!(sprintf "The post '%s' could not be found." page)]
                        a [Class "btn"; Href "/"] [!!"Return Home"]
                    ]
                ]
            ]
        ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    try
        let rendered = generate' ctx page

        // Figure out the relative path within "posts"
        // e.g. "posts/page1.md" -> "page1.html"
        //      "posts/subfolder/post.md" -> "subfolder/post.html"
        let relativePathInPosts =
            page.TrimStart([|'p';'o';'s';'t';'s';'\\';'/'|])  // remove leading "posts/" or "posts\"
            |> Path.ChangeExtension ".html"

        // Combine everything into _public/posts
        let outputPath = 
            Path.Combine(projectRoot, "_public", "posts", relativePathInPosts)

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)) |> ignore
        
        // Convert HtmlElement to string and write to file
        let html = Layout.render ctx rendered
        File.WriteAllText(outputPath, html)
        
        printfn "Generated post at: %s" outputPath
    with ex ->
        printfn "Error generating post %s: %s" page ex.Message
        
    // Return empty list as expected by Fornax
    []