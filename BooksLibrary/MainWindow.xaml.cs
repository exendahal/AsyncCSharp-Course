using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BooksLibrary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SynchronizationContext _context;
        public MainWindow()
        {
            InitializeComponent();

            //var context = SynchronizationContext.Current;
            //context.Send(state => { FindBook.Content = "CLICK!"; }, null);
            // create a sync context for this thread
            //_context = new SynchronizationContext();
            // set this context for this thread.
            //SynchronizationContext.SetSynchronizationContext(_context);
            _context = SynchronizationContext.Current;
            //_context.Post();
        }

        private void FindBook_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                _context.Send(state => { FindBook.Content = "BOOM!"; }, null);

                /*
                var context = SynchronizationContext.Current;
                if (context != null)
                {
                    context.Send(state => { FindBook.Content = "BOOM!"; }, null);
                }*/

                Book result = BookStorage.Find(9787532706068);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    BookTitle.Text = result.Title;
                });
            });            
        }
    }

    internal class BookStorage
    {
        public static Book Find(long isbn)
        {
            //emulate long operation
            Thread.Sleep(5000);
            return new Book("Leo Tolstoy", "War and Peace");
        }
    }

    class Book
    {
        public string AuthorName { get; }
        public string Title { get; }

        public Book(string authorName, string title)
        {
            AuthorName = authorName;
            Title = title;
        }
    }
}
