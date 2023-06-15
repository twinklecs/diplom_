using System;
using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class User
    {
        public int Id { get; set; }

        [StringLength(60, MinimumLength = 2)]
        [Required(ErrorMessage = "Пожалуйста, введите имя")]
        public string Name { get; set; }

        [StringLength(60, MinimumLength = 2)]
        [Required(ErrorMessage = "Пожалуйста, введите фамилию")]
        public string Surname { get; set; }

        [DataType(DataType.Date)]

        [Required(ErrorMessage = "Пожалуйста, введите Дату Рождения")]
        public DateTime DateOfBirth { get; set; }

        [StringLength(60, MinimumLength = 3)]
        [Required(ErrorMessage = "Пожалуйста, введите логин")]
        public string Login { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Пожалуйста, введите пароль")]
        public string Password { get; set; }

        [Range(1, 3)]
        [Required(ErrorMessage = "Пожалуйста, введите идентификатор роли")]
        public int RoleId { get; set; }

        public int IsDeleted { get; set; }
    }
}