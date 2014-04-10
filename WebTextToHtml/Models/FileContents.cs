// Copyright Stefan Nychka, BSD 3-Clause license, COPYRIGHT.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTextToHtml.Models
{
    public class FileContents
    {
        public int Id { get; set; }
        public string Contents { get; set; }
        public DateTime DateModified { get; set; }
        public int UserId { get; set; }
    }
} 