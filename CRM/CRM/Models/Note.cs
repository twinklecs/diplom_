using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Note
    {
        public int Id { get; set; }

        [StringLength(500)]
        [Required(ErrorMessage = "Пожалуйста, введите содержание заметки")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите идентификационный номер контакта")]
        public int ContactId { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите идентификатор пользователя")]
        public int UserId { get; set; }

        public int IsDeleted { get; set; }
    }
}
