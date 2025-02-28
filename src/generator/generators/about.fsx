#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open Html

let about = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi nisi diam, vehicula quis blandit id, suscipit sed libero. Proin at diam dolor. In hac habitasse platea dictumst. Donec quis dui vitae quam eleifend dignissim non sed libero. In hac habitasse platea dictumst. In ullamcorper mollis risus, a vulputate quam accumsan at. Donec sed felis sodales, blandit orci id, vulputate orci."

let generate' (ctx : SiteContents) (_: string) =
  let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
  let desc =
    siteInfo
    |> Option.map (fun si -> si.description)
    |> Option.defaultValue ""


  Layout.layout ctx "About" [
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
                        !! about
                    ]
                ]
            ]
        ]
      ]
    ]]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
  generate' ctx page
  |> Layout.render ctx