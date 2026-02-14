using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreJudge.Domain.Premitives
{
    public abstract class BaseEntity
    {
        protected BaseEntity(int id) => Id = id;
        protected BaseEntity() { }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; protected set; }
    }
}
