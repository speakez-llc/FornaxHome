#r "nuget: Fornax.Core, 0.15.1"
#load "layout.fsx" 


open Html
open System.IO
open System.Text.RegularExpressions


// Process custom shortcodes in content
let processShortcodes (content: string) =
    // Contact form shortcode
    let contactFormPattern = "{{contact_form}}"
    let contactFormHtml = """
<div class="card bg-base-100 shadow-xl">
  <div class="card-body">
    <form action="/api/contact" method="post">
      <div class="form-control w-full">
        <label class="label">
          <span class="label-text">Name</span>
        </label>
        <input type="text" name="name" class="input input-bordered w-full" required />
      </div>
      
      <div class="form-control w-full mt-4">
        <label class="label">
          <span class="label-text">Email</span>
        </label>
        <input type="email" name="email" class="input input-bordered w-full" required />
      </div>
      
      <div class="form-control w-full mt-4">
        <label class="label">
          <span class="label-text">Message</span>
        </label>
        <textarea name="message" class="textarea textarea-bordered h-24" required></textarea>
      </div>
      
      <div class="form-control mt-6">
        <button type="submit" class="btn btn-primary w-full">Send Message</button>
      </div>
    </form>
  </div>
</div>
"""

    // Alert shortcode
    let alertPattern = "{{alert (.*?)}}"
    let alertReplacer (m: Match) =
        let message = m.Groups.[1].Value
        sprintf """<div class="alert alert-info shadow-lg">
  <div>
    <span>%s</span>
  </div>
</div>""" message
    
    // Apply replacements
    let withContactForm = Regex.Replace(content, contactFormPattern, contactFormHtml)
    let withAlerts = Regex.Replace(withContactForm, alertPattern, alertReplacer)
    
    withAlerts

let generate' (ctx : SiteContents) (page: string) =
    
    // Get all available pages for debugging
    let allPages = 
        ctx.TryGetValues<Page>() 
        |> Option.defaultValue Seq.empty 
        |> Seq.toList
    
    // Try to find the page by comparing just the filename part
    let pageOption = 
        allPages 
        |> Seq.tryFind (fun p -> 
            // Compare just the filename without path or with path considered
            let pageFile = Path.GetFileName(page)
            let pFile = Path.GetFileName(p.file)
            pFile = pageFile || p.file = page
        )
    
    match pageOption with
    | Some pageData ->
        // Process any shortcodes in the content
        let processedContent = processShortcodes pageData.content
        
        // Use Layout.layout which includes the navigation bar
        Layout.layout ctx pageData.title [
            section [Class "hero bg-primary text-primary-content py-24"] [
                div [Class "hero-content text-center"] [
                    div [Class "max-w-md"] [
                        let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
                        let desc =
                            siteInfo
                            |> Option.map (fun si -> si.description)
                            |> Option.defaultValue ""
                        h1 [Class "text-4xl font-bold accent"] [!!desc]
                    ]
                ]
            ]
            div [Class "container mx-auto px-4"] [
                section [Class "py-8"] [
                    div [Class "max-w-3xl mx-auto"] [
                        div [Class "card bg-base-100 shadow-xl"] [
                            div [Class "card-body"] [
                                div [Class "prose prose-lg prose-headings:font-bold prose-h1:text-2xl prose-h2:text-xl prose-a:text-blue-600 prose-ul:list-disc prose-ul:ml-4 !max-w-none"] [
                                    !!processedContent
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    | None ->
        // Page not found
        printfn "Warning: Page '%s' not found in loaded content" page
        Layout.layout ctx "Page Not Found" [
            div [Class "container mx-auto px-4 py-8"] [
                div [Class "card bg-warning text-warning-content max-w-md mx-auto"] [
                    div [Class "card-body"] [
                        h2 [Class "card-title"] [!!"Page Not Found"]
                        p [] [!!(sprintf "The page '%s' could not be found." page)]
                        a [Class "btn"; Href "/"] [!!"Return Home"]
                    ]
                ]
            ]
        ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    try
        let rendered = generate' ctx page
        Layout.render ctx rendered
    with ex ->
        printfn "Error in page generator: %s" ex.Message
        
        // Use Layout.layout for error page too
        let errorPage = Layout.layout ctx "Error" [
            div [Class "container mx-auto px-4 py-8"] [
                div [Class "card bg-error text-error-content max-w-md mx-auto"] [
                    div [Class "card-body"] [
                        h2 [Class "card-title"] [!!"Error Generating Page"]
                        p [] [!!"There was an error generating this page. Please check the console for details."]
                    ]
                ]
            ]
        ]
        
        Layout.render ctx errorPage