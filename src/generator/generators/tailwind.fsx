#r "nuget: Fornax.Core, 0.15.1"

open System
open System.IO
open System.Diagnostics

// The key parts to modify in tailwind.fsx:

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    let generatorDir = projectRoot
    let outputPath = Path.Combine(generatorDir, "_public", Path.GetDirectoryName(page), Path.GetFileName(page))
    
    // Make sure output directory exists
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)) |> ignore
    
    try
        // Use the simplest approach with postcss-cli
        let psi = new ProcessStartInfo()
        psi.FileName <- "cmd"
        psi.Arguments <- sprintf "/c postcss %s -o %s" page outputPath
        psi.WorkingDirectory <- generatorDir
        psi.UseShellExecute <- false
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        
        // Log the command being executed
        printfn "Executing: postcss %s -o %s" page outputPath
        
        // Start the process
        use proc = Process.Start(psi)
        
        // Capture and log output
        use stdOutReader = proc.StandardOutput
        let cssOutput = stdOutReader.ReadToEnd()
        
        use stdErrReader = proc.StandardError
        let stdErr = stdErrReader.ReadToEnd()
        proc.WaitForExit()
        
        if proc.ExitCode <> 0 then
            printfn "PostCSS process failed with exit code %d" proc.ExitCode
            printfn "Error output: %s" stdErr
            ""
        else
            // Read the file we just created
            if File.Exists(outputPath) then
                File.ReadAllText(outputPath)
            else
                printfn "Warning: Output file not found at %s" outputPath
                ""
    with e ->
        printfn "Error: %s" e.Message
        ""