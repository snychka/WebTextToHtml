WebTextToHtml (vim2Html)
=============

Copyright Stefan Nychka, BSD 3-Clause license, LICENSE.txt

The main purpose of this code is to demonstrate my ability to program an ASP.NET
MVC web application, secondarily to convert vim text to html.  There may be
better programs that accept such text, consider it markup, and produce the
corresponding html.

TOC
- Conversion Explanation
- Running Code (short)
- Running Code (detailed)
- Environment (what to install and configuration settings)
- vim Settings
- Improvements

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

Running Code (short)
====================

  See the 'Running Code (detailed)' section for further info.

  - Unzip the project, and locate WebTextToHtml 2 dirs. in
  - Run Visual Studio (VS) as an Admin, and open WebTextToHtml.sln
  - Ensure no LocalDb named Text2HtmlContext exists
  - In VS, press F5 to run the code.
  - In IE, change the url to https://<span></span>localhost:44300
  - Ignore the SSL err. msg. and continue to the website ... that's basically it!

Running Code (detailed)
=======================

  First, see the 'Environment' section for further information what to install
  and configuration settings.

  - Unzip the project
    - Note the location of the WebTextToHtml.sln file, a couple directories in
  - Open Visual Studio (VS) as an Admin, then open WebTextToHtml.sln
    - Running software as an Admin:
      http://www.eightforums.com/tutorials/9564-run-administrator-windows-8-a.html
    - File >> Open >> Project/Solution..., navigate to and open
      WebTextToHtml.sln
    - Remember the SSL URL in the WebTextToHtml project's properties dialogue
    - To view the project properties, go to Hanselman's blog on IIS
      SSL, http://www.hanselman.com/blog/WorkingWithSSLAtDevelopmentTimeIsEasierWithIISExpress.aspx,
      and search for "properties dialog"
  - Ensure no LocalDb named Text2HtmlContext exists.  Can rename if it does
    - Connecting to LocalDb via SSMS:
      http://geekswithblogs.net/MagnusKarlsson/archive/2012/11/18/connect-to-localdb-using-sql-server-management-studio.aspx
    - Renaming a db:  http://msdn.microsoft.com/en-us/library/ms345378%28v=sql.110%29.aspx
  - In VS, press F5 to run the code in Debug mode.  IE should open up, but to a
    non-ssl url, likely saying "This page can't be displayed"
  - Change the url in IE to the SSL URL you remembered, which is *likely*
    http://localhost:44300
  - Ignore the SSl err. msg and continue to the website.


Environment (what to install and configuration settings)
========================================================

  Software installed
  - Windows 8
  - IE 10
  - Sql Server Express 2012 (v11.0.3128)
    - Installation (sqlexpress):  http://www.microsoft.com/en-ca/download/details.aspx?id=29062
    - Install LocalDb:  http://msdn.microsoft.com/en-CA/library/hh510202%28v=sql.110%29.aspx
    - Ensure SSMS is installed
  - Visual Studio 2012 Pro. (VS)
    - Installation (VS):  http://msdn.microsoft.com/en-us/library/e2h7fzkw%28v=vs.110%29.aspx
    - other versions will likely work
  - IIS Express (included with VS)

  IE/IIS settings
  - IE10 Standards mode
  - default Internet and Local Internet security settings
  - web site at https://localhost:44300

  VS settings
  - used VS '12 pro, yet project was more up-to-date (EF 6, MVC 5)
  - Debug mode

  LocalDb
  - Localdb, no existing Text2HtmlContext db

  Tested a web package deploy to IIS
  - 1st set up db
    - had empty SqlExpress db called Text2HtmlRelease
    - added Login &quot;IIS APPPOOL\DefaultAppPool&quot;
      - gave it reader, writer and ddladmin roles
  - IIS 8
    - directly on https://<span></span>localhost, Default Web Site
    - ssl enabled, self-signed cert

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

Improvements
===========

Many planned improvements to pick away at.  In no particular order, and not necessarily a complete list:

- make directly available a deployment package
  - have it install on a web site other than the Default
- put the site up somewhere, likely Azure
  - create a legit. cert.
- allow users to store multiple documents
- secure the connection strings
- consolidate the project names
- redesign using models as opposed to sessions (maybe)
  - ensure no over-posting
- improve the parsing errors
- ensure any text in textarea remains after 'Upload and Convert' is clicked with no file chosen
- allow user to set different whitespace (tabs, amount)
- allow to set title (in html) of downloaded document
- store files on file system, and only store loc. and likely checksum in db
- test on multiple browsers
- enforce better pswd. rules by instead using SqlMembershipProvider
- read in tornado text from file, as opposed to hardcoding it in View (maybe)
- use interfaces to allow for IoC and unit testing
- figure out more portable version of wrap=off
