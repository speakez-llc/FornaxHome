#r "nuget: Fornax.Core, 0.15.1"

/// Common types used throughout the site
module Types =
    /// Represents a page on the site
    type Page = {
        title: string
        link: string
        content: string
        file: string
    }