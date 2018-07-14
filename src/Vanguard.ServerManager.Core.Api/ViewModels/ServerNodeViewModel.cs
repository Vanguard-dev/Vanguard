using System.ComponentModel.DataAnnotations;

namespace Vanguard.ServerManager.Core.Api
{
    public class ServerNodeViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "PublicKey")]
        public string PublicKey { get; set; }

        [Display(Name = "UserId")]
        public string UserId { get; set; }
    }
}