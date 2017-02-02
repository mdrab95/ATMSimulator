using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace tut1.Models
{
    public class EmailFormModel
    {
        [Required]
        [Display(Name ="Name: ")]
        public string FromName { get; set; }

        [Required]
        [Display(Name = "Email: ")]
        public string FromEmail { get; set; }

        [Required]
        [Display(Name = "Message: ")]
        public string Message { get; set; }
    }
}