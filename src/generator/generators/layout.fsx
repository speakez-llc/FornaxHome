#r "../_lib/Fornax.Core.dll"
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
      <script>
        document.addEventListener('DOMContentLoaded', function() {
          // Theme handling
          const themeController = document.querySelector('.theme-controller');
          if (themeController) {
            // Set initial theme based on localStorage or default to business
            const savedTheme = localStorage.getItem('theme') || 'business';
            document.documentElement.setAttribute('data-theme', savedTheme);
            themeController.checked = savedTheme === 'corporate';
            
            // Add event listener for theme changes
            themeController.addEventListener('change', function() {
              const newTheme = this.checked ? 'corporate' : 'business';
              document.documentElement.setAttribute('data-theme', newTheme);
              localStorage.setItem('theme', newTheme);
            });
          }
        });
        </script>
        """
    let head = "<head>"
    let index = webpage.IndexOf head
    webpage.Insert ((index + head.Length + 1), websocketScript)

// Define Page type directly in layout.fsx - this is the single source of truth
type Page = {
    title: string
    link: string
    content: string
    file: string
}

let layout (ctx : SiteContents) active bodyCnt =
    // Define standard navigation directly here - the ONLY place it's defined
    let standardNavigation = [
        { title = "Home"; link = "/"; content = ""; file = "index.md" }
        { title = "Posts"; link = "/posts/index.html"; content = ""; file = "posts.md" }
        { title = "About"; link = "/about.html"; content = ""; file = "about.md" }
        { title = "Contact"; link = "/contact.html"; content = ""; file = "contact.md" }
    ]
    
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
    let ttl =
      siteInfo
      |> Option.map (fun si -> si.title)
      |> Option.defaultValue ""

    let menuEntries =
      standardNavigation
        |> Seq.sortBy (fun p -> 
            match p.title with
            | "Home" -> 0
            | "Posts" -> 1
            | "About" -> 2
            | "Contact" -> 3
            | _ -> 10
        )
        |> Seq.map (fun p ->
          let cls = if p.title = active then "active" else ""
          li [] [a [Class cls; Href p.link] [!! p.title]])
        |> Seq.toList

    html [] [
        head [] [
            meta [CharSet "utf-8"]
            meta [Name "viewport"; Content "width=device-width, initial-scale=1"]
            title [] [!! ttl]
            link [Rel "icon"; Type "image/ico"; Sizes "32x32"; Href "/images/favicon.ico"]
            link [Rel "stylesheet"; Href "https://cdn.jsdelivr.net/npm/tailwindcss@2.2/dist/tailwind.min.css"]
            link [Rel "stylesheet"; Href "https://cdn.jsdelivr.net/npm/daisyui@3.7.4/dist/full.css"]
            link [Rel "stylesheet"; Href "https://fonts.googleapis.com/css?family=Varela+Round"]
            link [Rel "stylesheet"; Href "https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.7.0/styles/default.min.css"]
            script [Src "https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.7.0/highlight.min.js"] []
            script [] [!! "document.addEventListener('DOMContentLoaded', () => hljs.highlightAll());"]
            link [Rel "stylesheet"; Type "text/css"; Href "/style/style.css"]
            script [Src "https://kit.fontawesome.com/3e50397676.js"; CrossOrigin "anonymous"] []
        ]
        body [] [
          nav [Class "navbar"] [
              div [Class "container flex justify-between mx-auto items-center w-full"] [
                  // Logo section - stays the same
                  div [Class "navbar-start"] [
                      a [Class "btn btn-ghost text-xl"; Href "/"] [
                          img [Src "/images/SpeakEZ_standard.png"; Alt "Logo"; Class "h-8 mr-2"]
                      ]
                  ]
                  // Desktop menu - ensure it's centered
                  div [Class "navbar-center hidden lg:flex items-center justify-center"] [
                      ul [Class "menu menu-horizontal px-1"] menuEntries
                      // Theme switcher for desktop
                      label [Class "swap swap-rotate ml-4"] [
                          input [Type "checkbox"; Class "theme-controller"; HtmlProperties.Custom ("data-toggle-theme", "business,corporate")]
                          i [Class "swap-on fa-solid fa-moon text-xl"] []
                          i [Class "swap-off fa-solid fa-sun text-xl"] []
                      ]
                  ]
                  // Mobile menu button and dropdown
                  div [Class "navbar-end lg:hidden"] [
                      div [Class "dropdown dropdown-end"] [
                          label [TabIndex 0; Class "btn btn-ghost"] [
                              i [Class "fa-solid fa-bars text-xl"] []
                          ]
                          ul [TabIndex 0; Class "dropdown-content menu menu-vertical mt-3 p-2 shadow bg-base-100 rounded-box w-52"] [
                              yield! menuEntries
                              li [] [
                                  label [Class "swap swap-rotate justify-center"] [
                                      input [Type "checkbox"; Class "theme-controller"; HtmlProperties.Custom ("data-toggle-theme", "business,corporate")]
                                      i [Class "swap-on fa-solid fa-moon text-xl"] []
                                      i [Class "swap-off fa-solid fa-sun text-xl"] []
                                  ]
                              ]
                          ]
                      ]
                  ]
              ]
          ]
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