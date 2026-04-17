using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;
using WpfApp17.Models;

namespace WpfApp17.Services
{
    public class ReportService
    {
        public List<Student> ImportFromCsv(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }))
            {
                csv.Read();
                csv.ReadHeader();
                var list = new List<Student>();
                while (csv.Read())
                {
                    list.Add(new Student
                    {
                        FIO = csv.GetField<string>("ФИО"),
                        Specialty = csv.GetField<string>("Специальность"),
                        Type = csv.GetField<string>("Тип"),
                        Year = csv.GetField<int>("Год"),
                        Date = DateTime.Parse(csv.GetField<string>("Дата")),
                        Percent = csv.GetField<double>("Балл%")
                    });
                }
                return list;
            }
        }

        public void Export(List<Student> students, string path, bool isExcel)
        {
            if (isExcel)
            {
                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Результаты");
                    ws.Cell(1, 1).Value = "ФИО";
                    ws.Cell(1, 2).Value = "Специальность";
                    ws.Cell(1, 3).Value = "Тип";
                    ws.Cell(1, 4).Value = "Год";
                    ws.Cell(1, 5).Value = "Дата";
                    ws.Cell(1, 6).Value = "Балл %";
                    ws.Cell(1, 7).Value = "Оценка";

                    int row = 2;
                    foreach (var s in students)
                    {
                        ws.Cell(row, 1).Value = s.FIO;
                        ws.Cell(row, 2).Value = s.Specialty;
                        ws.Cell(row, 3).Value = s.Type;
                        ws.Cell(row, 4).Value = s.Year;
                        ws.Cell(row, 5).Value = s.Date.ToString("dd.MM.yyyy");
                        ws.Cell(row, 6).Value = s.Percent;
                        ws.Cell(row, 7).Value = s.Grade;
                        row++;
                    }
                    wb.SaveAs(path);
                }
            }
            else
            {
                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }))
                {
                    csv.WriteField("ФИО");
                    csv.WriteField("Специальность");
                    csv.WriteField("Тип");
                    csv.WriteField("Год");
                    csv.WriteField("Дата");
                    csv.WriteField("Балл%");
                    csv.WriteField("Оценка");
                    csv.NextRecord();

                    foreach (var s in students)
                    {
                        csv.WriteField(s.FIO);
                        csv.WriteField(s.Specialty);
                        csv.WriteField(s.Type);
                        csv.WriteField(s.Year);
                        csv.WriteField(s.Date.ToString("dd.MM.yyyy"));
                        csv.WriteField(s.Percent);
                        csv.WriteField(s.Grade);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}