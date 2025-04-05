#r "nuget: Fornax.Core, 0.15.1"

type SiteInfo = {
    title: string
    description: string
    postPageSize: int
    lightTheme: string
    darkTheme: string
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    let siteInfo =
        { title = "SpeakEZ Technologies Home Page";
          description = "Intelligent Systems Designed for Reliability, Privacy, Performance & The Bottom Line";
          postPageSize = 5;
          lightTheme = "nord";
          darkTheme = "coporate" }
    siteContent.Add(siteInfo)

    siteContent
