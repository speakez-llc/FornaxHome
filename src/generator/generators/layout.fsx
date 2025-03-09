#r "nuget: Fornax.Core, 0.15.1"
#if !FORNAX
#load "../loaders/postloader.fsx"
#load "../loaders/pageloader.fsx"
#load "../loaders/globalloader.fsx"
#endif

open Html

let injectWebsocketCode (webpage:string) =
    let websocketScript =
        """
        <script type="text/javascript">
          var wsUri = "ws://localhost:8080/websocket";
      function init()
      {
        websocket = new WebSocket(wsUri);
        websocket.onclose = function(evt) { onClose(evt) };
      }
      function onClose(evt)
      {
        console.log('closing');
        websocket.close();
        document.location.reload();
      }
      window.addEventListener("load", init, false);
      </script>
        """
    let head = "<head>"
    let index = webpage.IndexOf head
    webpage.Insert ( (index + head.Length + 1),websocketScript)

type Page = {
    title: string
    link: string
    content: string
    file: string
}

// Just define a simpler navigation item type for the menu
type NavItem = {
    title: string
    link: string
}

// CENTRAL DEFINITION OF NAVIGATION
// This is the single source of truth for standard navigation across all pages
let getStandardNavigation () =
    [
        { NavItem.title = "Home"; link = "/" }
        { NavItem.title = "Posts"; link = "/posts/index.html" }
        { NavItem.title = "About"; link = "/about.html" }
        { NavItem.title = "Contact"; link = "/contact.html" }
    ]

// Creates a consistent navigation bar for all pages
let createNavBar (active: string) =
    let menuEntries =
        getStandardNavigation()
        |> List.map (fun p ->
            let cls = if p.title = active then "active" else ""
            li [] [a [Class cls; Href p.link] [!! p.title]])

    nav [Class "navbar"] [
        div [Class "container flex justify-between mx-auto items-center w-full"] [
            div [Class "navbar-start"] [
                a [Class "btn btn-ghost text-xl"; Href "/"] [
                    img [Src "/images/SpeakEZ_standard.png"; Alt "Logo"; Class "h-8 mr-2"]
                ]
            ]
            div [Class "navbar-center hidden lg:flex items-center"] [
                ul [Class "menu menu-horizontal px-1"] menuEntries
                label [Class "swap swap-rotate ml-4"] [
                    input [Type "checkbox"; Class "theme-controller"; HtmlProperties.Custom ("data-toggle-theme", "dark,light")]
                    i [Class "swap-on fa-solid fa-moon text-xl"] []
                    i [Class "swap-off fa-solid fa-sun text-xl"] []
                ]
            ]
            div [Class "navbar-end lg:hidden"] [
                div [Class "dropdown dropdown-end"] [
                    label [TabIndex 0; Class "btn btn-ghost"] [
                        i [Class "fa-solid fa-bars text-xl"] []
                    ]
                    ul [TabIndex 0; Class "dropdown-content menu menu-vertical mt-3 p-2 shadow bg-base-100 rounded-box w-52"] [
                        yield! menuEntries
                        li [] [
                            label [Class "swap swap-rotate justify-center"] [
                                input [Type "checkbox"; Class "theme-controller"; HtmlProperties.Custom ("data-toggle-theme", "business,custom")]
                                i [Class "swap-on fa-solid fa-moon text-xl"] []
                                i [Class "swap-off fa-solid fa-sun text-xl"] []
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// Core layout function that all generators should use
let layout (ctx : SiteContents) active bodyCnt =
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
    let ttl =
      siteInfo
      |> Option.map (fun si -> si.title)
      |> Option.defaultValue ""

    let navBar = createNavBar active

    html [HtmlProperties.Custom ("data-theme", "dark")] [
        head [] [
            meta [CharSet "utf-8"]
            meta [Name "viewport"; Content "width=device-width, initial-scale=1"]
            title [] [!! ttl]
            link [Rel "icon"; Type "image/ico"; Sizes "32x32"; Href "/images/favicon.ico"]
            link [Rel "stylesheet"; Href "https://fonts.googleapis.com/css?family=Varela+Round"]
            script [Src "https://cdnjs.cloudflare.com/ajax/libs/mermaid/9.1.3/mermaid.min.js"] [] 
            link [Rel "stylesheet"; Type "text/css"; Href "/style/style.css"]
            script [Src "https://kit.fontawesome.com/3e50397676.js"; CrossOrigin "anonymous"] []
        ]
        body [] [
            navBar
            yield! bodyCnt
        ]
    ]

let render (ctx : SiteContents) cnt =
  let disableLiveRefresh = ctx.TryGetValue<Postloader.PostConfig> () |> Option.map (fun n -> n.disableLiveRefresh) |> Option.defaultValue false
  cnt
  |> HtmlElement.ToString
  |> fun n -> if disableLiveRefresh then n else injectWebsocketCode n

let published (post: Postloader.Post) =
    post.published
    |> Option.defaultValue System.DateTime.Now
    |> fun n -> n.ToString("yyyy-MM-dd")

let postLayout (useSummary: bool) (post: Postloader.Post) =
    div [Class "card w-full bg-base-100 shadow-xl mb-6"] [
        div [Class "card-body"] [
            div [Class "text-center"] [
                h2 [Class "card-title justify-center"] [ a [Href post.link] [!! post.title]]
                p [Class "text-sm opacity-70"] [
                a [Href "#"] [!! (defaultArg post.author "")]
                !! (sprintf " on %s" (published post))
                ]
            ]
            div [Class "mt-4 prose lg:prose-lg"] [
                !! (if useSummary then post.summary else post.content)
            ]
        ]
    ]