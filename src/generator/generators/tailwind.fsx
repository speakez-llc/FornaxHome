#r "../_lib/Fornax.Core.dll"
#r "nuget: SharpScss, 2.0.0"

open System.IO

let processFile (projectRoot: string, filePath: string) =
    let content = File.ReadAllText filePath
    
    // Run tailwind CLI (assuming it's installed)
    let tempInputFile = Path.Combine(projectRoot, "_public/temp-input.css")
    let outputFile = Path.Combine(projectRoot, "_public", filePath.Substring(projectRoot.Length).TrimStart('\\', '/'))
    
    File.WriteAllText(tempInputFile, content)
    
    let exitCode = 
        System.Diagnostics.Process.Start(
            "npx", 
            sprintf "tailwindcss -i %s -o %s" tempInputFile outputFile
        ).WaitForExit()
    
    File.Delete(tempInputFile)
    
    // Return the processed content
    File.ReadAllText outputFile