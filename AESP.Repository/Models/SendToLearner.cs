using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class SendToLearner
    {
        [Key]
        public Guid SendToLearnerId { get; set; }


        public Guid ContentLibraryId { get; set; }
        [ForeignKey(nameof(ContentLibraryId))]
        public ContentLibrary ContentLibrary { get; set; } = null!;

        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; } = null!; // ✅ sửa lại tên

    }
}
