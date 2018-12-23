using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using System.IO.IsolatedStorage;

namespace ZClock
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private DispatcherTimer _dtimer;

        /// <summary>
        /// 应用程序名
        /// </summary>
        private const string APP_NAME = "ZClock";

        public MainWindow()
        {
            InitializeComponent();

            //初始化计时器
            _dtimer = new DispatcherTimer();
            _dtimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _dtimer.Tick += _dtimer_Tick;
            _dtimer.Start();
        }

        private void winClock_Loaded(object sender, RoutedEventArgs e)
        {
            ReadWindowPosition();
            this.cbAutoStart.IsChecked = true;
            cbAutoStart_Click(null, null);
        }

        private void SaveWindowPosition()
        {
            try
            {
                double x = this.Left, y = this.Top;
                IsolatedStorageFile isFile = IsolatedStorageFile.GetUserStoreForDomain();
                IsolatedStorageFileStream isStream = new IsolatedStorageFileStream(APP_NAME, FileMode.Create, FileAccess.Write);
                using (StreamWriter writer = new StreamWriter(isStream))
                {
                    writer.WriteLine(string.Format("{0},{1}", x, y));
                }
                isStream.Close();
                isFile.Close();
            }
            catch (Exception) { }
        }

        private void ReadWindowPosition()
        {
            try
            {
                double recordLeft=0,recordTop=0;
                IsolatedStorageFile isFile = IsolatedStorageFile.GetUserStoreForDomain();
                IsolatedStorageFileStream isStream = new IsolatedStorageFileStream(APP_NAME, FileMode.Open, isFile);
                using (StreamReader reader = new StreamReader(isStream))
                {
                    string[] strLoc = reader.ReadLine().Split(',');
                    recordLeft = double.Parse(strLoc[0]);
                    recordTop = double.Parse(strLoc[1]);
                }

                if (System.Windows.Forms.Screen.AllScreens.Length>1)
                {//有多个屏幕时
                    this.Left = recordLeft;
                    this.Top = recordTop;
                }
                else
                {
                    double workWidth = SystemParameters.WorkArea.Width,//得到屏幕工作区域宽度
                        workHeight = SystemParameters.WorkArea.Height;//得到屏幕工作区域高度
                    this.Left = recordLeft <= workWidth - this.Width ? recordLeft : workWidth / 2;
                    this.Top = recordTop <= workHeight - this.Height ? recordTop : workHeight / 2;
                }

                isStream.Close();
                isFile.Close();
            }
            catch (Exception) { }
        }

        private void _dtimer_Tick(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;
            this.labTime.Content=time.ToString("HH:mm:ss");
            this.labDate.Content = time.ToString("yyyy年MM月dd日");
            this.labDayOfYear.Content = string.Format("本年第{0}天", time.DayOfYear);
            switch (time.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    this.labWeek.Content = "周日";
                    break;
                case DayOfWeek.Monday:
                    this.labWeek.Content = "周一";
                    break;
                case DayOfWeek.Tuesday:
                    this.labWeek.Content = "周二";
                    break;
                case DayOfWeek.Wednesday:
                    this.labWeek.Content = "周三";
                    break;
                case DayOfWeek.Thursday:
                    this.labWeek.Content = "周四";
                    break;
                case DayOfWeek.Friday:
                    this.labWeek.Content = "周五";
                    break;
                case DayOfWeek.Saturday:
                    this.labWeek.Content = "周六";
                    break;
            }
        }

        private void winClock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }

            SaveWindowPosition();
        }

        private void menuItemClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbAutoStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string execPath = Process.GetCurrentProcess().MainModule.FileName; 
                RegistryKey rkCurrentUser = Registry.CurrentUser;
                RegistryKey rkEun = rkCurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
                if ((bool)this.cbAutoStart.IsChecked)
                {
                    rkEun.SetValue(APP_NAME, execPath);
                }
                else {
                    rkEun.DeleteValue(APP_NAME, false);
                }
                rkEun.Close();
                rkCurrentUser.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作注册表失败！！\n"+ex.Message);
            }
        }

    }
}
