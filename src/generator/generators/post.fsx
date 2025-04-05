#r "nuget: Fornax.Core, 0.15.1"
#load "layout.fsx"

open Html
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
            // Hero section should match posts.fsx structure exactly
            section [Id "static-hero-container"; Class "hero bg-primary text-primary-content py-24"] [
                div [Class "hero-content text-center"] [
                    div [Class "max-w-md"] [
                        h1 [Class "text-4xl font-bold text-white"] [!!desc]
                    ]
                ]
            ]
            // Content area should match posts.fsx structure exactly 
            div [Id "content-area"; Class "container mx-auto px-4"] [
                // Posts page has a section inside content-area with a class of py-8
                section [Class "py-8"] [
                    div [Class "max-w-3xl mx-auto"] [
                        // Back button at the top
                        div [Class "flex items-center mb-6"] [
                            a [
                                Class "btn btn-outline btn-sm gap-2"; 
                                Href "/posts/index.html"
                            ] [
                                i [Class "fa-solid fa-arrow-left text-sm"] []
                                !! "Back to Posts"
                            ]
                        ]
                        
                        // Post content
                        Layout.postLayout false post
                        
                        // Back button at the bottom
                        div [Class "flex justify-center mt-8"] [
                            a [
                                Class "btn btn-outline gap-2"; 
                                Href "/posts/index.html"
                            ] [
                                i [Class "fa-solid fa-arrow-left"] []
                                !! "Back to Posts"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    | None ->
        printfn "Warning: Post '%s' not found" page
        Layout.layout ctx "Post Not Found" [
            section [Id "static-hero-container"; Class "hero bg-warning text-warning-content py-24"] [
                div [Class "hero-content text-center"] [
                    div [Class "max-w-md"] [
                        h1 [Class "text-4xl font-bold"] [!!"Page Not Found"]
                    ]
                ]
            ]
            div [Id "content-area"; Class "container mx-auto px-4"] [
                section [Class "py-8"] [
                    div [Class "max-w-3xl mx-auto"] [
                        div [Class "card bg-warning text-warning-content max-w-md mx-auto"] [
                            div [Class "card-body"] [
                                h2 [Class "card-title"] [!!"Post Not Found"]
                                p [] [!!(sprintf "The post '%s' could not be found." page)]
                                a [Class "btn"; Href "/"] [!!"Return Home"]
                            ]
                        ]
                    ]
                ]
            ]
        ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    let content = generate' ctx page
    Layout.render ctx content