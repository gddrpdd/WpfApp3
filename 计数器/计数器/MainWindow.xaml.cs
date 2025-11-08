using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Media;

namespace MultiFunctionCounter
{
    public partial class MainWindow : Window
    {
        // 字段声明区域 - 修复：添加下划线前缀保持一致性
        private int _currentCount = 0;
        private int _stepValue = 1;
        private bool _isCountdownMode = false;
        private int _countdownSeconds = 0;
        private int _remainingSeconds = 0;
        private DispatcherTimer _countdownTimer;

        // 修复：添加下划线前缀，并在构造函数中初始化
        private List<string> _operationLogs;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        /// <summary>
        /// 应用程序初始化方法 - 修复：确保所有字段都被初始化
        /// </summary>
        private void InitializeApplication()
        {
            // 修复：必须初始化 _operationLogs
            _operationLogs = new List<string>();

            // 初始化倒计时定时器
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += CountdownTimer_Tick;

            UpdateDisplay();
            AddLog("应用程序启动", 0); // 现在可以安全调用了
        }

        /// <summary>
        /// 添加操作日志 - 修复所有错误
        /// </summary>
        /// <param name="operation">操作描述</param>
        /// <param name="value">操作后的值</param>
        private void AddLog(string operation, int value)
        {
            // 修复1：确保_logs不为null
            if (_operationLogs == null)
                _operationLogs = new List<string>();

            // 修复2：正确的时间格式 HH:mm:ss
            string logEntry = $"{DateTime.Now:HH:mm:ss} - {operation} → {value}";

            // 添加到日志列表
            _operationLogs.Add(logEntry);

            // 修复3：完整的更新日志显示逻辑
            UpdateLogDisplay();
        }

        /// <summary>
        /// 更新日志显示 - 修复循环错误
        /// </summary>
        private void UpdateLogDisplay()
        {
            // 清空当前显示
            lstLog.Items.Clear();

            // 修复4：正确的循环逻辑 - 只显示最近10条
            int startIndex = Math.Max(0, _operationLogs.Count - 10);
            for (int i = startIndex; i < _operationLogs.Count; i++)
            {
                // 修复5：使用正确的列表项
                lstLog.Items.Add(_operationLogs[i]);
            }

            // 修复6：安全的滚动到最后
            if (lstLog.Items.Count > 0)
            {
                lstLog.ScrollIntoView(lstLog.Items[lstLog.Items.Count - 1]);
            }
        }

        // +1 按钮点击事件
        private void BtnIncrement_Click(object sender, RoutedEventArgs e)
        {
            if (_isCountdownMode)
            {
                ShowMessage("提示", "倒计时模式下不能手动计数！");
                return;
            }

            _currentCount += _stepValue;
            UpdateDisplay();
            AddLog($"增加 +{_stepValue}", _currentCount);
        }

        // -1 按钮点击事件
        private void BtnDecrement_Click(object sender, RoutedEventArgs e)
        {
            if (_isCountdownMode)
            {
                ShowMessage("提示", "倒计时模式下不能手动计数！");
                return;
            }

            _currentCount -= _stepValue;
            UpdateDisplay();
            AddLog($"减少 -{_stepValue}", _currentCount);
        }

        // 重置按钮点击事件
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            if (_countdownTimer != null && _countdownTimer.IsEnabled)
            {
                _countdownTimer.Stop();
                _isCountdownMode = false;
            }

            _currentCount = 0;
            UpdateDisplay();
            AddLog("重置计数器", _currentCount);
        }

        // 步进值选择改变事件
        private void CmbStep_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmbStep.SelectedItem is System.Windows.Controls.ComboBoxItem item)
            {
                if (int.TryParse(item.Content.ToString(), out int step))
                {
                    _stepValue = step;
                    AddLog($"步进值更改为 {_stepValue}", _currentCount);
                }
            }
        }

        // 开始倒计时按钮点击事件
        private void BtnStartCountdown_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtCountdown.Text, out int seconds) || seconds <= 0)
            {
                ShowMessage("输入错误", "请输入有效的正数秒数！");
                return;
            }

            _countdownSeconds = seconds;
            _remainingSeconds = seconds;
            _isCountdownMode = true;
            _currentCount = seconds;

            UpdateDisplay();
            _countdownTimer.Start();
            AddLog($"开始倒计时: {seconds}秒", _currentCount);
        }

        // 倒计时定时器触发事件
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            _remainingSeconds--;
            _currentCount = _remainingSeconds;

            UpdateDisplay();

            if (_remainingSeconds <= 0)
            {
                _countdownTimer.Stop();
                _isCountdownMode = false;
                ShowMessage("倒计时完成", "倒计时已结束！");
                PlayAlertSound();
                AddLog("倒计时完成", 0);
            }
        }

        /// <summary>
        /// 更新界面显示
        /// </summary>
        private void UpdateDisplay()
        {
            tbCount.Text = _currentCount.ToString();
            tbMode.Text = _isCountdownMode ? "当前模式：倒计时" : "当前模式：普通计数";

            if (_isCountdownMode)
            {
                TimeSpan timeRemaining = TimeSpan.FromSeconds(_remainingSeconds);
                tbRemainingTime.Text = $"剩余时间: {timeRemaining:mm\\:ss}";
            }
            else
            {
                tbRemainingTime.Text = "";
            }

            btnIncrement.IsEnabled = !_isCountdownMode;
            btnDecrement.IsEnabled = !_isCountdownMode;
            btnStartCountdown.IsEnabled = !_isCountdownMode;
        }

        /// <summary>
        /// 显示消息对话框
        /// </summary>
        private void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// 播放提示音
        /// </summary>
        private void PlayAlertSound()
        {
            try
            {
                SystemSounds.Beep.Play();
            }
            catch (Exception ex)
            {
                AddLog($"声音播放失败: {ex.Message}", _currentCount);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_countdownTimer != null)
            {
                _countdownTimer.Stop();
            }
            base.OnClosed(e);
        }
    }
}