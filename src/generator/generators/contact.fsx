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

  Layout.layout ctx "Contact" [
    section [Class "hero bg-primary text-primary-content py-24"] [
      div [Class "hero-content text-center"] [
        div [Class "max-w-md"] [
          h1 [Class "text-4xl font-bold"] [!!desc]
        ]
      ]
    ]
    div [Class "container mx-auto px-4"] [
      section [Class "py-8"] [
        div [Class "max-w-md mx-auto"] [
          div [Class "card bg-base-100 shadow-xl"] [
            div [Class "card-body"] [
              h2 [Class "card-title justify-center"] [!!"Contact Us"]
              form [Action "/api/contact"; Method "post"] [
                div [Class "form-control w-full"] [
                  label [Class "label"] [span [Class "label-text"] [!!"Name"]]
                  input [Type "text"; Name "name"; Class "input input-bordered w-full"; Required true]
                ]
                div [Class "form-control w-full mt-4"] [
                  label [Class "label"] [span [Class "label-text"] [!!"Email"]]
                  input [Type "email"; Name "email"; Class "input input-bordered w-full"; Required true]
                ]
                div [Class "form-control w-full mt-4"] [
                  label [Class "label"] [span [Class "label-text"] [!!"Message"]]
                  textarea [Name "message"; Class "textarea textarea-bordered h-24"; Required true] []
                ]
                div [Class "form-control mt-6"] [
                  button [Type "submit"; Class "btn btn-primary w-full"] [!!"Send Message"]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
  ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
  generate' ctx page
  |> Layout.render ctx