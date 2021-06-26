using System.ComponentModel.DataAnnotations;

namespace Realmar.Jobbernetes.AdminWeb.Shared.Models
{
    public class AddJobModel
    {
        public AddJobModel()
        {
            SuccessProbability = 82;
            IncreaseMin        = 20;
            IncreaseMax        = 80;
        }

        [Required] public string Name { get; set; }

        [Required, Range(typeof(int), "0", "100")]
        public int SuccessProbability { get; set; }

        [Required, Range(typeof(int), "0", "0x7fffffff")]
        public int IncreaseMin { get; set; }

        [Required, Range(typeof(int), "0", "0x7fffffff")]
        public int IncreaseMax { get; set; }

        [Required] public string TopError { get; set; }
    }
}
