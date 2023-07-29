using System;
using System.ComponentModel.DataAnnotations;

namespace WebApiTask.Models
{
    public class Resident
    {
        public int Id { get; set; } //Уникальный Id
        public string NumberLc { get; set; } //Номер ЛС (генерируется автоматически)
        public DateTime DateBegin { get; set; } //Дата открытия (для тестирования и удобства ввода устанавливается самое минимальное)
        public DateTime DateEnd { get; set; }  //Дата закрытия (для тестирования и удобства ввода устанавливается текущее)
        [Required(ErrorMessage = "Укажите правильный адрес проживающих")]
        public string Address { get; set; } //Адрес проживания
        [Range(18, 500, ErrorMessage = "Площадь должна быть в промежутке от 18 до 500")]
        public int Area {  get; set; } // площадь помещения
        [Range(0, 10, ErrorMessage = "Количество проживающих должно быть в промежутке от 0 до 10")]
        public int CountResidents { get; set; } //количество проживающих

        [Required(ErrorMessage = "Укажите имена жителей")]
        public string Names { get; set; } //Имена всех проживающих (ввод через ',')

        [Required(ErrorMessage = "Укажите возраст жителей")]
        public string Age { get; set; } //Возраст проживающих (ввод через ',')

        public string NamePayer { get; set; } //Имя плательщика (Первое имя из Names)
    }
}
