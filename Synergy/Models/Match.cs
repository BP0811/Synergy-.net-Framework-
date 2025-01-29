using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Synergy.Models
{
    public class Match
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public int MatchUserId { get; set; }
        [ForeignKey("MatchUserId")]
        public virtual User? MatchUser { get; set; }

        public DateTime MatchDate { get; set; } = DateTime.Now;
    }
}