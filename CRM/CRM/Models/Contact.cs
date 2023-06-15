using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [StringLength(60, MinimumLength = 2)]
        [Required(ErrorMessage = "Пожалуйста, введите ФИО")]
        public string FIO { get; set; }

        [RegularExpression(@"^(\d{11})$", ErrorMessage = "Пожалуйста, введите правильный номер телефона")]
        [Required(ErrorMessage = "Пожалуйста, введите номер телефона")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите вашу область")]
        public string Region { get; set; }

        [StringLength(60, MinimumLength = 2)]
        [Required(ErrorMessage = "Пожалуйста, введите город")]
        public string City { get; set; }

        public int IsDeleted { get; set; }
    }
}
