using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer= new DispatcherTimer();
            timer.Interval= TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick; 
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.timeTestBox.Text= DateTime.Now.ToString();
            //throw new NotImplementedException();//事件处理器
        }
    }
}