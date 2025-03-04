#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"
#load "layout.fsx"

open Html

let generate' (ctx : SiteContents) (_: string) =
  let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
  let desc =
    siteInfo
    |> Option.map (fun si -> si.description)
    |> Option.defaultValue ""

  let homeContent = """
# Welcome to SpeakEZ Technologies

SpeakEZ provides intelligent systems designed for privacy, reliability, speed, and improving your bottom line.

## Our Services

- Custom AI solutions
- Privacy-focused technology
- High-performance systems
- Business intelligence tools

[View our Blog](/posts/index.html)
"""

  let rendered =
    Layout.layout ctx "Home" [
      section [Class "hero bg-primary text-primary-content py-24"] [
        div [Class "hero-content text-center"] [
          div [Class "max-w-md"] [
            h1 [Class "text-4xl font-bold text-white"] [!!desc]
          ]
        ]
      ]
      div [Class "container mx-auto px-4"] [
        section [Class "py-8"] [
          div [Class "max-w-3xl mx-auto"] [
            div [Class "card w-full bg-base-100 shadow-xl mb-6"] [
              div [Class "card-body"] [
                div [Class "prose lg:prose-lg"] [
                  !! (Markdig.Markdown.ToHtml(homeContent))
                ]
              ]
            ]
          ]
        ]
      ]
    ]
    |> Layout.render ctx
  
  [("index.html", rendered)]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
  generate' ctx page