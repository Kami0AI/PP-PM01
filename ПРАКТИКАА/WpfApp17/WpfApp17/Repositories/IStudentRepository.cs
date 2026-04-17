using System.Collections.Generic;
using WpfApp17.Models;

namespace WpfApp17.Repositories
{
    public interface IStudentRepository
    {
        List<Student> GetAll();
        void Add(Student student);
        void Delete(Student student);
        bool IsOnline { get; }
    }
}