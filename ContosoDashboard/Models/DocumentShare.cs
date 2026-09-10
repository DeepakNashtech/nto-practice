using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentShare
{
    [Key]
    public int DocumentShareId { get; set; }

    public int DocumentId { get; set; }

    public int SharedWithUserId { get; set; }

    public int SharedByUserId { get; set; }

    public DateTime SharedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string Permission { get; set; } = "Read";

    // Navigation properties
    [ForeignKey("DocumentId")]
    public virtual Document? Document { get; set; }

    [ForeignKey("SharedWithUserId")]
    public virtual User? SharedWithUser { get; set; }

    [ForeignKey("SharedByUserId")]
    public virtual User? SharedByUser { get; set; }
}
