#r "nuget: Fornax.Core, 0.15.1"

type SiteInfo = {
    title: string
    description: string
    postPageSize: int
}

let loader (projectRoot: string) (siteContent: SiteContents) =
    let siteInfo =
        { title = "SpeakEZ Technologies Home Page";
          description = "Intelligent Systems Designed for Privacy, Reliability, Speed and <br>The Bottom Line";
          postPageSize = 5 }
    siteContent.Add(siteInfo)

    siteContent
