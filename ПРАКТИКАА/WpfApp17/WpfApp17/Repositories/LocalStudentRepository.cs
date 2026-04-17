using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using WpfApp17.Models;

namespace WpfApp17.Repositories
{
    public class LocalStudentRepository : IStudentRepository
    {
        private readonly string _filePath = "data.csv";
        public bool IsOnline => true;

        public List<Student> GetAll()
        {
            if (!File.Exists(_filePath)) return new List<Student>();
            using (var reader = new StreamReader(_filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }))
                return csv.GetRecords<Student>().ToList();
        }

        public void Add(Student student)
        {
            var students = GetAll();
            if (students.Any(s => s.FIO.Equals(student.FIO, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Студент уже существует");
            students.Add(student);
            Save(students);
        }

        public void Delete(Student student)
        {
            var students = GetAll();
            students.RemoveAll(s => s.FIO == student.FIO);
            Save(students);
        }

        private void Save(List<Student> students)
        {
            using (var writer = new StreamWriter(_filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }))
                csv.WriteRecords(students);
        }
    }
}