#r "nuget: Fornax.Core, 0.15.1"

open System.IO
open System.Diagnostics
open System

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    let generatorDir = projectRoot
    let baseStylePath = Path.Combine(generatorDir, "style", "style.css")

    try
        // Run tailwind to get CSS content
        let psi = new ProcessStartInfo("cmd", $"/c npx tailwindcss -i {baseStylePath}")
        psi.WorkingDirectory <- generatorDir
        psi.UseShellExecute <- false
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        psi.CreateNoWindow <- true
        
        use proc = Process.Start(psi)
        
        // Capture CSS output
        use stdOutReader = proc.StandardOutput
        use stdErrReader = proc.StandardError
        let cssContent = stdOutReader.ReadToEnd()
        let stdErr = stdErrReader.ReadToEnd()
        proc.WaitForExit()
        
        // Print diagnostics
        printfn "CSS content length: %d bytes" cssContent.Length
        
        // KEY INSIGHT: Instead of writing the file ourselves, RETURN the CSS content
        // Fornax will handle writing it to the target file
        cssContent
        
    with e ->
        printfn "Error generating CSS: %s" e.Message
        // Return empty CSS in case of error
        ""