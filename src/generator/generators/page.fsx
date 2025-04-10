#r "nuget: Fornax.Core, 0.15.1"
#load "layout.fsx" 


open Html
open System.IO
open System.Text.RegularExpressions

// Process custom shortcodes in content
let processShortcodes (content: string) =
    // Contact form shortcode
    let contactFormPattern = "{{contact_form}}"
    
    // Generate contact form using the HTML DSL instead of a string - WITHOUT the card structure
    let generateContactForm () =
        form [Action "/api/contact"; Method "post"] [
            div [Class "form-control w-full"] [
                label [Class "label"] [
                    span [Class "label-text"] [!!"Name"]
                ]
                input [Type "text"; Name "name"; Class "input input-bordered w-full"; Required true]
            ]
            
            div [Class "form-control w-full mt-4"] [
                label [Class "label"] [
                    span [Class "label-text"] [!!"Email"]
                ]
                input [Type "email"; Name "email"; Class "input input-bordered w-full"; Required true]
            ]
            
            div [Class "form-control w-full mt-4"] [
                label [Class "label"] [
                    span [Class "label-text"] [!!"Message"]
                ]
                textarea [Name "message"; Class "textarea textarea-bordered h-24"; Required true] []
            ]
            
            div [Class "form-control mt-6"] [
                button [Type "submit"; Class "btn btn-primary"] [!!"Send Message"]
            ]
        ]
        |> HtmlElement.ToString
    
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
    let withContactForm = Regex.Replace(content, contactFormPattern, generateContactForm())
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
            div [Class "card bg-base-100 shadow-xl"] [
                div [Class "card-body"] [
                    div [Class "prose prose-lg prose-headings:font-bold prose-h1:text-2xl prose-h2:text-xl prose-a:text-blue-600 prose-ul:list-disc prose-ul:ml-4 !max-w-none"] [
                        !!processedContent
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