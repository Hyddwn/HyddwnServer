Help
=============================================================================

Each page on this Wiki is a file in the Wiki's "page" folder. By simply
writing new pages in [Markdown (CommonMark) format](http://spec.commonmark.org/0.26/)
and linking to them somewhere on the Wiki, you can "add" them to it.
Custom pages should go into the [user folder](https://github.com/aura-project/aura/wiki/User-folder)
(user/web/wiki/pages), to avoid conflicts on updating, while changes and
improvements to existing pages should be pushed to the [main repository](https://github.com/aura-project/aura),
for the benefit of all Aura users.

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
