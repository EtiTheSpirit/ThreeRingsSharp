---
name: Report a Conversion Problem or Buggy Model
about: Use this if the model you exported isn't quite right, like the mesh is broken or missing parts in your modeling software.
title: "(Briefly explain what broke in a few words)"
labels: [
  "bug",
  "conversion"
]
assignees: EtiTheSpirit

---

#############################################################################
### Requirement: Is it already going to be fixed? (Erase after you read this) ###
Check ...
- This page! Search for your issue first and see if it hasn't been sent by someone else yet.
- Next Update Plans at https://github.com/EtiTheSpirit/ThreeRingsSharp/projects/3?fullscreen=true

Ensure that ...
- You are reporting an issue **that you noticed in your 3D modeling software.** If the error is in ThreeRingsSharp itself, use the "Report a Different Kind of Bug" option.
- You are on the latest version of TRS. If you're on an old version, this might already be fixed in a new version!

Did someone else already report your issue? Don't post this. Upvote their report instead.
#############################################################################


### What's happening?
Describe what's going on here, and be clear about it! It's helpful if you send the `latest.log` file in the same folder as the exe too (This is created from Beta 0.1.1 onward)

### What model?
Arguably the most important piece of information: What model is breaking? Provide the file path to the model relative to the `rsrc` folder, for example: `rsrc/character/npc/monster/gremlin/null/model.dat`

### What settings?
Does the model have any customizable attributes (blue text in the lower data tree)? If so, did you change any of this data? What was the original value? What did you set it to? Does it work fine if you use the original value?

### How do I make it happen?
Describe the EXACT steps needed to cause this issue here in clear detail. Think of it like if you were trying to teach your grandma how to break my program.
