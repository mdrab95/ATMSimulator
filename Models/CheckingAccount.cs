using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace tut1.Models
{
    public class CheckingAccount
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [Column(TypeName ="varchar")]
        [Display(Name = "Account #")]
        [RegularExpression(@"\d{6,10}", ErrorMessage = "Account # must be between 6 and 10 digits")]
        public string AccountNumber { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        public string Name
        {
            get { return string.Format("{0} {1}", this.FirstName, this.LastName); }
        }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        public virtual ApplicationUser User { get; set; }
        
        [Required]
        public string ApplicationUserId { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}