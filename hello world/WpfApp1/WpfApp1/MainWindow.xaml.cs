using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Forms; // 添加这一行

namespace CameraCaptureTool
{
    public partial class MainWindow : Window
    {
        private CameraManager _cameraManager;
        private string _savePath;
        private int _currentCameraIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
            _cameraManager = new CameraManager();
            InitializeCameraList();
            _savePath = Path.Combine(Directory.GetCurrentDirectory(), "Captures");
        }

        private void InitializeCameraList()
        {
            try
            {
                string[] cameras = _cameraManager.GetCameraDevices();
                CameraComboBox.Items.Clear();

                foreach (string camera in cameras)
                {
                    CameraComboBox.Items.Add(camera);
                }

                if (cameras.Length > 0)
                {
                    CameraComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"初始化摄像头列表失败: {ex.Message}";
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CameraComboBox.SelectedIndex >= 0)
                {
                    _currentCameraIndex = CameraComboBox.SelectedIndex;
                    bool success = _cameraManager.StartCamera(_currentCameraIndex,
                        new System.Windows.Interop.WindowInteropHelper(this).Handle);

                    if (success)
                    {
                        StatusText.Text = "摄像头已启动";
                        StartButton.IsEnabled = false;
                        StopButton.IsEnabled = true;
                        CaptureButton.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"启动失败: {ex.Message}";
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _cameraManager.StopCamera();
            StatusText.Text = "摄像头已停止";
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            CaptureButton.IsEnabled = false;
        }

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 确保保存目录存在
                if (!Directory.Exists(_savePath))
                {
                    Directory.CreateDirectory(_savePath);
                }

                // 拍照
                var bitmap = _cameraManager.CaptureImage();
                if (bitmap != null)
                {
                    string fileName = $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                    string fullPath = Path.Combine(_savePath, fileName);

                    bitmap.Save(fullPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    bitmap.Dispose();

                    StatusText.Text = $"照片已保存: {fullPath}";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"拍照失败: {ex.Message}";
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // 使用 WPF 自带的对话框
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Title = "选择保存文件夹";
            dialog.Filter = "所有文件|*.*";
            dialog.FileName = "选择文件夹"; // 随便写个名字

            if (dialog.ShowDialog() == true)
            {
                _savePath = System.IO.Path.GetDirectoryName(dialog.FileName);
                StatusText.Text = $"保存路径: {_savePath}";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _cameraManager.StopCamera();
            base.OnClosed(e);
        }
    }
}