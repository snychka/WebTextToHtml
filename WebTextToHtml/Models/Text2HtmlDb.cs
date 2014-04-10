// Copyright Stefan Nychka, BSD 3-Clause license, COPYRIGHT.txt
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebTextToHtml.Models
{
    public class Text2HtmlDb : DbContext
    {
        public Text2HtmlDb() : base("Text2HtmlContext") { }
        public DbSet<FileContents> Files { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}