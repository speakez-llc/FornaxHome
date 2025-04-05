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

    // SIMPLIFIED: Just provide the post list and pagination
    Layout.layout ctx "Posts" (
        postList @ [
            div [Class "flex justify-center items-center gap-4 p-4 rounded-lg"] [
                a [Class "btn btn-outline"; Href previousPage] [!! "Previous"]
                span [Class "text-sm"] [!! (sprintf "Page %i of %i" (i + 1) pages)]
                a [Class "btn btn-outline"; Href nextPage] [!! "Next"]
            ]
        ]
    )

  postList
  |> List.mapi (fun i psts ->
    getFilenameForIndex i,
    layoutForPostSet i psts
    |> Layout.render ctx)

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    generate' ctx page