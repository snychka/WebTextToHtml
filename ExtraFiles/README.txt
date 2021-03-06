WebTextToHtml (vim2Html)
=============

Copyright Stefan Nychka, BSD 3-Clause license, LICENSE.txt

The main purpose of this code is to demonstrate my ability to program an ASP.NET
MVC web application, secondarily to convert vim text to html.  There may be
better programs that accept such text, consider it markup, and produce the
corresponding html.

TOC
- Conversion Explanation
- Running Code
- Environment
- vim Settings

Conversion Explanation
======================

  vim2Html takes a text file, most conveniently created by vim (see *vim
  Settings for details), and creates corresponding html.  It is easiest
  explained via some examples of the resulting html in the <body> tag.  And, you
  can try it out to see!

  Example 1 text

  a paragraph

    ----------------------------------------

    Example 1 html

    <p>
    a paragraph
    </p>

    ----------------------------------------

  Example 2 text

    - a list and bullet

    ----------------------------------------
    Example 2 html

    <ul>
      <li>
      a list and bullet
      </li>
    </ul>
    ----------------------------------------

  Example 3 text

    a paragraph
    - a list and bullet
      - an indented bullet
      - 2nd indented bullet
    - 2nd outer bullet
    - a longer line with a url, http://www.somesite.com . note how
      subsequent newlines in a bullet should be sufficiently indented
    a new paragraph

    2 subsequent paragraphs must be separated by a space

    ----------------------------------------

    Example 3 text

    <p>
    a paragraph
    </p>
    <ul>
      <li>
      a list and bullet
      <ul>
        <li>
        an indented bullet
        </li>
        <li>
        2nd indented bullet
        </li>
      </ul>
      </li>
      <li>
      2nd outer bullet
      </li>
      <li>
      a longer line with a url, <a href="http://www.somesite.com," target="_blank">http://www.somesite.com</a> . note how
      subsequent newlines in a bullet should be sufficiently indented
      </li>
    </ul>
    <p>
    a new paragraph
    </p>
    <p>
    2 subsequent paragraphs must be separated by a space
    </p>
    ----------------------------------------

Running Code
============

  See the Environment section for further information

  To run using Visual Studio (VS) using IIS Express
  - Unzip and open in Visual
  - Run code ... that's basically it!
  - Note
    - will run on https://localhost:44300
    - may need to manually go to https://localhost:44300
    - using IIS Expess, only tested in Debug mode
    - will create an empty Localdb db named Text2HtmlContext


Environment
===========

  General Settings
  - Windows 8, IE 10
    - IE10 Standards mode
    - default Internet and Local Internet security settings
  - SqlExpress v11.0.3128

  IIS Express
  - used VS 12, yet project was more up-to-date (EF 6, MVC 5)
    - Debug mode
  - Localdb
  - https://localhost:44300

  Tested a web package deploy to IIS
  - IIS 8
    - directly on localhost, Default Web Site
  - SqlExpress
    - ssl enabled, self-signed cert
  - in SSMS, prior to running
    - had empty SqlExpress db called Text2HtmlRelease
    - added Login 'IIS APPPOOL\DefaultAppPool'
      - gave it reader, writer and ddladmin roles

vim Settings
============

  vim is a text editor used on Linux-based systems, http://www.vim.org

  See Conversion Explanation section for some examples

  important settings
  - autoindent shiftwidth=2 shiftwidth=2 tabstop=2 expandtab

  complete settings

    autoindent          ignorecase          scroll=11           textwidth=80
    backspace=2         modelines=0         shiftwidth=2        ttyfast
    expandtab           modified            softtabstop=2       ttymouse=xterm
    helplang=en         pastetoggle=<C-P>   tabstop=2           window=0
    fileencoding=utf-8
    fileencodings=ucs-bom,utf-8,default,latin1
    wildmode=longest,list
