#r "nuget: Fornax.Core, 0.15.1"
#load "layout.fsx"

open Html

let generate' (ctx : SiteContents) (_: string) =
  let posts = ctx.TryGetValues<Postloader.Post> () |> Option.defaultValue Seq.empty
  let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
  let desc, postPageSize =
    siteInfo
    |> Option.map (fun si -> si.description, si.postPageSize)
    |> Option.defaultValue ("", 10)

  let postList =
    posts
    |> Seq.sortByDescending Layout.published
    |> Seq.toList
    |> List.chunkBySize postPageSize
    |> List.map (List.map (Layout.postLayout true))

  let pages = List.length postList

  let getFilenameForIndex i =
    if i = 0 then
      sprintf "posts/index.html"
    else
      sprintf "posts/page%i.html" i

  let layoutForPostSet i postList =
      let nextPage =
          if i = (pages - 1) then "#"
          else "/" + getFilenameForIndex (i + 1)
  
      let previousPage =
          if i = 0 then "#"
          else "/" + getFilenameForIndex (i - 1)

      Layout.layout ctx "Posts" [
          section [Class "hero bg-primary text-primary-content py-24"] [
              div [Class "hero-content text-center"] [
                  div [Class "max-w-md"] [
                      h2 [Class "text-4xl font-bold accent"] [!!desc]
                  ]
              ]
          ]
          div [Class "container mx-auto px-4"] [
              section [Class "py-8"] [
                  div [Class "max-w-3xl mx-auto"] postList
              ]
              div [Class "flex justify-center items-center gap-4 p-4 rounded-lg"] [
                  a [Class "btn btn-outline transition-opacity duration-500 ease-in-out"; Href previousPage] [!! "Previous"]
                  span [Class "text-sm"] [!! (sprintf "Page %i of %i" (i + 1) pages)]
                  a [Class "btn btn-outline transition-opacity duration-500 ease-in-out"; Href nextPage] [!! "Next"]
              ]
          ]
      ]

  postList
  |> List.mapi (fun i psts ->
    getFilenameForIndex i,
    layoutForPostSet i psts
    |> Layout.render ctx)

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    generate' ctx page