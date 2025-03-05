---
layout: post
title: Some nice post title
author: @houstonhaynes
published: 2020-02-19
---
# Introduction

Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi nisi diam, vehicula quis blandit id, suscipit sed libero. Proin at diam dolor. In hac habitasse platea dictumst. Donec quis dui vitae quam eleifend dignissim non sed libero. In hac habitasse platea dictumst. In ullamcorper mollis risus, a vulputate quam accumsan at. Donec sed felis sodales, blandit orci id, vulputate orci.

<!--more-->

```fsharp
open System

let config = {
    Generators = [
        {Script = "less.fsx"; Trigger = OnFileExt ".less"; OutputFile = ChangeExtension "css" }
        {Script = "sass.fsx"; Trigger = OnFileExt ".scss"; OutputFile = ChangeExtension "css" }
        {Script = "post.fsx"; Trigger = OnFilePredicate postPredicate; OutputFile = ChangeExtension "html" }
        {Script = "staticfile.fsx"; Trigger = OnFilePredicate staticPredicate; OutputFile = SameFileName }
        {Script = "index.fsx"; Trigger = Once; OutputFile = MultipleFiles id }
        {Script = "about.fsx"; Trigger = Once; OutputFile = NewFileName "about.html" }
        {Script = "contact.fsx"; Trigger = Once; OutputFile = NewFileName "contact.html" }
        {Script = "tailwind.fsx"; Trigger = OnFileExt ".css"; OutputFile = SameFileName }
    ]
}
```

Phasellus aliquam tellus eu augue vulputate laoreet. Nunc tincidunt sed mauris eu vestibulum. Quisque id ex eget erat elementum euismod vel nec ex. Nunc et blandit neque. Duis erat ex, facilisis non consectetur sit amet, consectetur mattis ex. Vestibulum quis ligula pharetra, semper nibh nec, porta augue. In placerat auctor risus, eu dictum purus iaculis et. Vivamus viverra sollicitudin augue, in sollicitudin leo malesuada non.

In hac habitasse platea dictumst. Quisque a diam egestas, ornare felis quis, gravida arcu. In vel tellus facilisis, rhoncus ligula sit amet, feugiat massa. Interdum et malesuada fames ac ante ipsum primis in faucibus. Curabitur varius interdum dolor, ut pretium augue egestas id. Aenean vulputate commodo nibh tristique egestas. Interdum et malesuada fames ac ante ipsum primis in faucibus. Vivamus elementum non mi sit amet lacinia.