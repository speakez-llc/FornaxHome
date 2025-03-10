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
    icon: string
}

// CENTRAL DEFINITION OF NAVIGATION
// This is the single source of truth for standard navigation across all pages
let getStandardNavigation () =
    [
        { NavItem.title = "Home"; link = "/"; icon = "fa-solid fa-home text-xl"  }
        { NavItem.title = "Posts"; link = "/posts/index.html"; icon = "fa-solid fa-newspaper text-xl"  }
        { NavItem.title = "About"; link = "/about.html"; icon = "fa-solid fa-user text-xl" }
        { NavItem.title = "Contact"; link = "/contact.html"; icon = "fa-solid fa-envelope text-xl" }
    ]

// Creates a consistent navigation bar for all pages
let createNavBar (active: string) (ctx : SiteContents)  =
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
    let ttl, darkTheme, lightTheme =
      siteInfo
      |> Option.map (fun si -> si.title, si.darkTheme, si.lightTheme)
      |> Option.defaultValue ("", "dark", "light")
    let menuEntries =
        getStandardNavigation()
        |> List.map (fun p ->
            let cls = if p.title = active then "active" else ""
            li [] [
                a [Class cls; Href p.link] [
                    i [Class (p.icon + " mr-2")] [] 
                    !! p.title
                ]
            ])

    nav [Class "navbar sticky top-0 z-10 bg-base-100"] [ 
        div [Class "container flex justify-between mx-auto items-center w-full"] [
            div [Class "navbar-start item-left"] [
                a [Class "btn btn-ghost text-xl"; Href "/"] [
                    img [Src "/images/SpeakEZ_standard.png"; Alt "Logo"; Class "h-8 mr-2"]
                ]
            ]
            div [Class "lg:navbar-center hidden lg:flex"] [
                ul [Class "menu menu-horizontal"] menuEntries
            ]
            div [Class "navbar-end hidden lg:flex"] [
                label [Class "swap swap-rotate ml-4"] [
                    input [Type "checkbox"; Class "theme-controller"; Value lightTheme]
                    i [Class "swap-on fa-solid fa-moon text-xl"] []
                    i [Class "swap-off fa-solid fa-sun text-xl"] []
                ]
            ]
            div [Class "navbar-end lg:hidden justify-end flex"] [
                div [Class "dropdown dropdown-end"] [
                    label [TabIndex 0; Class "btn btn-ghost"] [
                        i [Class "fa-solid fa-bars text-xl"] []
                    ]
                    ul [TabIndex 0; Class "dropdown-content menu menu-vertical mt-3 p-2 shadow bg-base-100 rounded-box w-52"] [
                        yield! menuEntries
                        li [] [
                            div [Class "flex items-right"] [
                                label [Class "swap swap-rotate"] [
                                    input [Type "checkbox"; Class "theme-controller"; Value lightTheme]
                                    i [Class "swap-on fa-solid fa-moon text-xl"] []
                                    i [Class "swap-off fa-solid fa-sun text-xl"] []
                                ]
                                span [Class "ml-2 flex-grow"] [!! "Theme"]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// Core layout function that all generators should use
// Core layout function that all generators should use
let layout (ctx : SiteContents) active bodyCnt =
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
    let ttl, darkTheme, lightTheme =
      siteInfo
      |> Option.map (fun si -> si.title, si.darkTheme, si.lightTheme)
      |> Option.defaultValue ("", "dark", "light")

    // Pass the ctx argument to createNavBar
    let navBar = createNavBar active ctx

    html [HtmlProperties.Custom ("data-theme", darkTheme)] [ // Use corporate as the default theme
        head [] [
            meta [CharSet "utf-8"]
            meta [Name "viewport"; Content "width=device-width, initial-scale=1"]
            title [] [!! ttl]
            link [Rel "icon"; Type "image/ico"; Sizes "32x32"; Href "/images/favicon.ico"]
            link [Rel "stylesheet"; Href "https://fonts.googleapis.com/css?family=Varela+Round"]
            script [Src "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/prism.min.js"] []
            script [Src "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-javascript.min.js"] []
            script [Src "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-css.min.js"] []
            script [Src "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-markup.min.js"] []
            script [Src "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-fsharp.min.js"] []
            script [Src "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-csharp.min.js"] []
            script [Src "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-powershell.min.js"] []
            script [Src "https://cdn.jsdelivr.net/npm/prismjs@1.29.0/components/prism-bash.min.js"] []            
            script [Src "https://cdnjs.cloudflare.com/ajax/libs/mermaid/9.1.3/mermaid.min.js"] []
            script [] [!! "document.addEventListener('DOMContentLoaded', function() { Prism.highlightAll(); });"]
            link [Rel "stylesheet"; Type "text/css"; Href "/style/style.css"]
            script [Src "https://kit.fontawesome.com/3e50397676.js"; CrossOrigin "anonymous"] []
            script [] [!! (sprintf """
                // Theme initialization script with themes from siteInfo
                (function() {
                    // Theme configuration from F# SiteInfo
                    const darkTheme = '%s';
                    const lightTheme = '%s';
                    
                    // Get theme from localStorage or default to dark theme
                    let savedTheme = localStorage.getItem('theme') || darkTheme;
                    
                    // Function to apply theme
                    function applyTheme(theme) {
                        document.documentElement.setAttribute('data-theme', theme);
                        localStorage.setItem('theme', theme);
                    }
                    
                    // Apply saved theme
                    applyTheme(savedTheme);
                    
                    // Initialize theme toggles once DOM is loaded
                    document.addEventListener('DOMContentLoaded', function() {
                        const themeToggles = document.querySelectorAll('.theme-controller');
                        
                        // Update checkbox state based on current theme (checked = light theme)
                        themeToggles.forEach(toggle => {
                            toggle.checked = (savedTheme === lightTheme);
                        });

                        // Add event listeners to all theme toggles
                        themeToggles.forEach(toggle => {
                            toggle.addEventListener('change', function() {
                                const newTheme = this.checked ? lightTheme : darkTheme;
                                applyTheme(newTheme);
                                
                                // Sync all other toggles
                                themeToggles.forEach(otherToggle => {
                                    if (otherToggle !== this) {
                                        otherToggle.checked = this.checked;
                                    }
                                });
                            });
                        });
                    });
                })();
            """ darkTheme lightTheme)]
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
                // Use a CSS solution to hide the first h1/h2 in the content
                if useSummary then
                    !! post.summary
                else
                    div [Class "hide-first-heading"] [!! post.content]
            ]
        ]
    ]