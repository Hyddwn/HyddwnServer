Help
=============================================================================

Each page on this Wiki is a file in the Wiki's "page" folder. By simply
writing new pages in [Markdown (CommonMark) format](http://spec.commonmark.org/0.26/)
and linking to them somewhere on the Wiki, you can "add" them to it.
Custom pages should go into the [user folder](https://github.com/aura-project/aura/wiki/User-folder)
(user/web/wiki/pages), to avoid conflicts on updating, while changes and
improvements to existing pages should be pushed to the [main repository](https://github.com/aura-project/aura),
for the benefit of all Aura users.

__TOC__

URL
------------------------------------------------------------------------------

The attribute after the question mark in the URL becomes the name of the
page you would like to see, as you can see in the address bar right now.

```
http://your-ip/wiki/?Help
```

The Wiki then looks for a page that has an [H1 header](http://www.w3schools.com/tags/tag_hn.asp)
with the text "Help" and opens it. If no page was found, it shows the text
"Page not found", with brief instructions on how to add it.

An H1 header in Markdown can be written in one of the following ways:

```
Help
======================

# Help

<h1>Help</h1>
```

Spaces may be replaced by underscores, for cleaner URLs.

```
Generation 1
======================

http://your-ip/wiki/?Generation_1
```

Format
-----------------------------------------------------------------------------

>Markdown is a plain text format for writing structured documents, based on
>conventions used for indicating formatting in email and usenet posts.
>It was developed in 2004 by John Gruber. ([commonmark.org](http://spec.commonmark.org/0.26/#what-is-markdown-))

Markdown was chosen for this Wiki for its wide-spread use and its simple
design. Everybody can easily pick it up and write new pages in it, without
learning a new kind of language first. The pages can also be displayed by
any Markdown parser, without the need of this Wiki, or even be read in a
plain text editor, due to Markdown's natural design.

For more information on Markdown and available codes, head over to
the [CommonMark page](http://spec.commonmark.org/0.26/), specifically its
[Help page](http://commonmark.org/help/).

Feature specific text
-----------------------------------------------------------------------------

The purpose of this Wiki is not to just have your own little Wiki, but to
have a Wiki that adjusts itself to the features enabled on the server.
For example, in the G1 quest "Bind Magic", the number of zombies you have
to kill depends on the enabled generations, or rather their features.
In the beginning that number was 50, then it was reduced to 10, and nowadays
it's only 1.

If the wiki just said "X", or listed all possible values, it would become
a little chaotic or confusing. Instead, you can filter which information
are displayed via CSS, like in the following example.

```
1. Talk to Dougal. His owl will soon deliver a quest.
2. Kill
   <span data-feature="!EasyBinding1"><span data-feature="!EasyBinding2">50</span></span>
   <span data-feature="EasyBinding1"><span data-feature="!EasyBinding2">10</span></span>
   <span data-feature="EasyBinding2">1</span>
   zombies in graveyard area.
3. After clearing the quest, one can revive near Dougal
   when knocked unconscious.
```

If you wrap something in a tag with the data-feature attribute it will only
be visible if the feature is enabled. You can also negate it, hiding it if
the feature is enabled. In the following example only one line will be
displayed, depending on whether the EmainMacha feature is enabled or not.

```
<span data-feature="EmainMacha">Emain Macha is a town west of Dunbarton.</span>
<span data-feature="!EmainMacha">Emain what now?</span>
```

This is the big advantage of this Wiki over others, which only ever reflect
the latest updates of Mabinogi.

Table of contents
-----------------------------------------------------------------------------

The table of contents (TOC) is a list of headers on a page that you can jump
to by clicking on them. It's not inserted by default, but you can add it
to a page by placing the following code somewhere on it:

```
__TOC__
```

For example:

```
Page Title
=============================================================================

Description.

__TOC__

First header
-----------------------------------------------------------------------------
```

By default only level 1 headers (H2) are visible in the table, so we can
use H3 and higher without them blowing up the TOC. The others are hidden
via CSS. To make them appear on one page only, you need to place CSS code
on it to make that happen. For example, to display level 2 headers (H3)
you would change the respective display property to `block`.

```
End of page text.

<style>
	.toc-level2 {
		display: block;
	}
</style>
```

For more examples, just look at the existing pages.
