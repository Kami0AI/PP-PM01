using System;
using CsvHelper.Configuration.Attributes;

namespace WpfApp17.Models
{
    public class Student
    {
        [Name("ФИО")]
        public string FIO { get; set; }

        [Name("Специальность")]
        public string Specialty { get; set; }

        [Name("Тип")]
        public string Type { get; set; }

        [Name("Год")]
        public int Year { get; set; }

        [Name("Дата")]
        [Format("dd.MM.yyyy")] // <-- Явное указание формата даты
        public DateTime Date { get; set; }

        [Name("Балл%")]
        public double Percent { get; set; }

        [Ignore] // Оценка вычисляется автоматически, в CSV не читается/не пишется
        public string Grade => Percent >= 50 ? "Зачтено" : "Не зачтено";

        public Student() { }
    }
}