using System;
using System.Collections.Generic;
using System.IO;
using Google.Cloud.Firestore;
using WpfApp17.Models;

namespace WpfApp17.Repositories
{
    public class FirebaseStudentRepository : IStudentRepository
    {
        private readonly FirestoreDb _db;
        private const string CollectionName = "exam_results";
        public bool IsOnline { get; private set; } = true;

        public FirebaseStudentRepository(string projectId)
        {
            var keyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-key.json");
            if (File.Exists(keyPath))
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyPath);
            _db = FirestoreDb.Create(projectId);
        }

        public List<Student> GetAll()
        {
            try
            {
                var snapshot = _db.Collection(CollectionName).GetSnapshotAsync().Result;
                IsOnline = true;
                return ParseSnapshot(snapshot);
            }
            catch
            {
                IsOnline = false;
                return new List<Student>();
            }
        }

        public void Add(Student student)
        {
            try
            {
                var docRef = _db.Collection(CollectionName).Document();
                docRef.SetAsync(new Dictionary<string, object>
                {
                    ["fio"] = student.FIO,
                    ["specialty"] = student.Specialty,
                    ["type"] = student.Type,
                    ["year"] = student.Year,
                    ["date"] = student.Date.ToString("O"),
                    ["percent"] = student.Percent
                }).Wait();
                IsOnline = true;
            }
            catch
            {
                IsOnline = false;
                throw new InvalidOperationException("Нет подключения к Firebase.");
            }
        }

        public void Delete(Student student)
        {
            try
            {
                var query = _db.Collection(CollectionName).WhereEqualTo("fio", student.FIO);
                var snapshot = query.GetSnapshotAsync().Result;
                foreach (var doc in snapshot.Documents)
                    doc.Reference.DeleteAsync().Wait();
            }
            catch { IsOnline = false; }
        }

        private List<Student> ParseSnapshot(QuerySnapshot snapshot)
        {
            var list = new List<Student>();
            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();
                list.Add(new Student
                {
                    FIO = data["fio"].ToString(),
                    Specialty = data["specialty"].ToString(),
                    Type = data["type"].ToString(),
                    Year = Convert.ToInt32(data["year"]),
                    Date = DateTime.Parse(data["date"].ToString()),
                    Percent = Convert.ToDouble(data["percent"])
                });
            }
            return list;
        }
    }
}