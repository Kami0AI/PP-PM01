using System;
using System.Threading.Tasks;
using System.Windows;
using WpfApp17.Repositories;
using WpfApp17.ViewModels;

namespace WpfApp17
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            const string projectId = "pm01-5712b";

            var firebaseRepo = new FirebaseStudentRepository(projectId);
            var localRepo = new LocalStudentRepository();

            //Проверяем доступность Firebase(ждём 3 сек)
            bool useCloud = false;
            try
            {
                var task = Task.Run(() => firebaseRepo.GetAll());
                task.Wait(3000);
                useCloud = task.Status == TaskStatus.RanToCompletion && firebaseRepo.IsOnline;
            }
            catch { useCloud = false; }

            //Выбираем активный репозиторий
            IStudentRepository activeRepo;
            if (useCloud)
                activeRepo = (IStudentRepository)firebaseRepo;
            else
                activeRepo = (IStudentRepository)localRepo;

            var viewModel = new MainViewModel(activeRepo);
            var mainWindow = new MainWindow { DataContext = viewModel };
            mainWindow.Show();
        }
    }
}