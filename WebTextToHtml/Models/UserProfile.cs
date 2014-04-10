// Copyright Stefan Nychka, BSD 3-Clause license, COPYRIGHT.txt
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebTextToHtml.Models
{
   
    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        [StringLength(255, ErrorMessage = "Name must be 255 characters or less")]
        public string UserName { get; set; }
    }


}