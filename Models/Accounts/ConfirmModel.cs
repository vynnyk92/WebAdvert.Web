using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace WebAdvert.Web.Models.Accounts
{
    public class ConfirmModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }  
    }
}
