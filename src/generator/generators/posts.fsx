#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html

let generate' (ctx : SiteContents) (_: string) =
  let posts = ctx.TryGetValues<Postloader.Post> () |> Option.defaultValue Seq.empty
  let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
  let desc, postPageSize =
    siteInfo
    |> Option.map (fun si -> si.description, si.postPageSize)
    |> Option.defaultValue ("", 10)

  let psts =
    posts
    |> Seq.sortByDescending Layout.published
    |> Seq.toList
    |> List.chunkBySize postPageSize
    |> List.map (List.map (Layout.postLayout true))

  let pages = List.length psts

  let getFilenameForIndex i =
    if i = 0 then
      sprintf "posts/index.html"
    else
      sprintf "posts/page%i.html" i

  let layoutForPostSet i psts =
      let nextPage =
          if i = (pages - 1) then "#"
          else "/" + getFilenameForIndex (i + 1)
  
      let previousPage =
          if i = 0 then "#"
          else "/" + getFilenameForIndex (i - 1)
  
      // Use Layout.layout exactly as other pages do
      Layout.layout ctx "Posts" [
          // Hero section - identical to other pages
          section [Class "hero bg-primary text-primary-content py-24"] [
              div [Class "hero-content text-center"] [
                  div [Class "max-w-md"] [
                      h1 [Class "text-4xl font-bold accent"] [!!desc]
                  ]
              ]
          ]
          
          // Content container - structured exactly like page.fsx
          div [Class "container mx-auto px-4"] [
              section [Class "py-8"] [
                  div [Class "max-w-3xl mx-auto"] psts
              ]
              
              // Pagination controls
              div [Class "flex justify-center items-center gap-4 p-4 rounded-lg"] [
                  a [Class "btn btn-outline transition-opacity duration-500 ease-in-out"; Href previousPage] [!! "Previous"]
                  span [Class "text-sm"] [!! (sprintf "Page %i of %i" (i + 1) pages)]
                  a [Class "btn btn-outline transition-opacity duration-500 ease-in-out"; Href nextPage] [!! "Next"]
              ]
          ]
      ]

  psts
  |> List.mapi (fun i psts ->
    getFilenameForIndex i,
    layoutForPostSet i psts
    |> Layout.render ctx)

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    try
        generate' ctx page
    with ex ->
        printfn "Error generating posts: %s" ex.Message
        [("posts/index.html", 
          Layout.layout ctx "Posts" [
            div [Class "container mx-auto px-4 py-8"] [
                div [Class "card bg-error text-error-content max-w-md mx-auto"] [
                    div [Class "card-body"] [
                        h2 [Class "card-title"] [!!"Error Loading Posts"]
                        p [] [!!"There was an error generating the posts page."]
                        a [Class "btn"; Href "/"] [!!"Return Home"]
                    ]
                ]
            ]
          ]
          |> Layout.render ctx)]