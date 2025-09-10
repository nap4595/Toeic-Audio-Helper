using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.IO;
using System.Diagnostics;

namespace ToeicAudioHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private bool _isSliding = false;
        private string? _root;
        private AudioCatalog _catalog = new AudioCatalog();
        private string? _currentFile;
        private bool _repeatEnabled = false;
        private string _themeName = "dark"; // default
        private bool _isPlaying = false;
        private bool _isFullTestMode = true; // true: Full Test mode, false: Question mode

        public MainWindow()
        {
            InitializeComponent();
            _timer.Interval = TimeSpan.FromMilliseconds(200);
            _timer.Tick += Timer_Tick;
            UpdateStatus("폴더를 먼저 선택하세요.");
        }

        // Simple custom chrome
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }
        private void BtnMin_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnMax_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void TglThemeMode_Click(object sender, RoutedEventArgs e)
        {
            var next = (_themeName == "dark") ? "light" : "dark";
            ApplyTheme(next);
            SaveTheme(next);
            _themeName = next;
            UpdateThemeToggleGlyph();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // 즉시 이벤트 처리를 방지하여 다른 컨트롤로 전파되지 않도록 함
            switch (e.Key)
            {
                case Key.Space:
                    e.Handled = true; // 먼저 이벤트 전파 방지
                    TogglePlayPause();
                    break;
                case Key.Left:
                    e.Handled = true;
                    BtnSeekBack_Click(this, new RoutedEventArgs());
                    break;
                case Key.Right:
                    e.Handled = true;
                    BtnSeekForward_Click(this, new RoutedEventArgs());
                    break;
                case Key.Tab:
                    e.Handled = true;
                    ToggleMode();
                    break;
                case Key.Up:
                    e.Handled = true;
                    HandleUpArrow();
                    break;
                case Key.Down:
                    e.Handled = true;
                    HandleDownArrow();
                    break;
                case Key.Enter:
                    e.Handled = true;
                    HandleEnterKey();
                    break;
            }
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "음원 루트 폴더를 선택하세요";
            if (!string.IsNullOrWhiteSpace(_root) && Directory.Exists(_root))
            {
                dlg.SelectedPath = _root;
            }
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _root = dlg.SelectedPath;
                TxtRoot.Text = _root;
                SaveLastRoot(_root);
                Rescan();
            }
        }

        private void BtnRescan_Click(object sender, RoutedEventArgs e) => Rescan();

        private void Rescan()
        {
            if (string.IsNullOrWhiteSpace(_root) || !Directory.Exists(_root))
            {
                UpdateStatus("유효한 폴더가 아닙니다.");
                return;
            }
            UpdateStatus("검색 중...");
            _catalog = FileScanner.Scan(_root);

            int overall = _catalog.OverallByTest.Count;
            int qSegments = _catalog.QuestionsByTest.Values.Sum(l => l.Count);
            int lc = _catalog.VocaLcByTest.Count;
            int rc = _catalog.VocaRcByTest.Count;
            UpdateStatus($"전체:{overall}개, 문항세그먼트:{qSegments}개, VOCA LC:{lc}개, VOCA RC:{rc}개 찾음");

            // compact UI: no list output
        }

        private int SelectedTest()
        {
            var item = (ComboBoxItem)CmbTest.SelectedItem;
            return int.Parse((string)item.Content);
        }

        private void BtnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            var t = SelectedTest();
            if (_catalog.OverallByTest.TryGetValue(t, out var path))
            {
                LoadFile(path);
            }
            else
            {
                MessageBox.Show($"Test {t:00} 전체 파일을 찾을 수 없습니다.", "안내", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnPlayPart_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtQuestion.Text, out var q))
            {
                MessageBox.Show("문항 번호를 숫자로 입력하세요.");
                return;
            }
            var t = SelectedTest();
            if (!_catalog.QuestionsByTest.TryGetValue(t, out var list))
            {
                MessageBox.Show($"Test {t:00} 문항 파일 정보를 찾을 수 없습니다.");
                return;
            }
            var seg = list.FirstOrDefault(s => s.Contains(q));
            if (seg == null)
            {
                MessageBox.Show($"Test {t:00} - 문항 {q}에 해당하는 파일이 없습니다.");
                return;
            }
            LoadFile(seg.FilePath);
        }

        private void BtnPlayVocaLc_Click(object sender, RoutedEventArgs e)
        {
            var t = SelectedTest();
            if (_catalog.VocaLcByTest.TryGetValue(t, out var list) && list.Count > 0)
            {
                LoadFile(list[0].FilePath);
            }
            else MessageBox.Show($"Test {t:00} LC VOCA 파일을 찾을 수 없습니다.");
        }

        private void BtnPlayVocaRc_Click(object sender, RoutedEventArgs e)
        {
            var t = SelectedTest();
            if (_catalog.VocaRcByTest.TryGetValue(t, out var list) && list.Count > 0)
            {
                LoadFile(list[0].FilePath);
            }
            else MessageBox.Show($"Test {t:00} RC VOCA 파일을 찾을 수 없습니다.");
        }

        private void LoadFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    MessageBox.Show("파일이 존재하지 않습니다: " + path);
                    return;
                }
                _currentFile = path;
                TxtNowPlaying.Text = System.IO.Path.GetFileName(path);
                Player.Stop();
                Player.Source = new Uri(path);
                // 파일만 로드하고 재생하지 않음
                StopPositionTimer();
                _isPlaying = false;
                BtnPlayPause.Content = "\uE768"; // Play icon
                SldPosition.Value = 0;
                LblNow.Text = "00:00";
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일 로드 중 오류: " + ex.Message);
            }
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            TogglePlayPause();
        }

        private void TogglePlayPause()
        {
            if (Player.Source == null) return;
            
            if (_isPlaying)
            {
                Player.Pause();
                StopPositionTimer();
                _isPlaying = false;
                BtnPlayPause.Content = "\uE768"; // Play icon
            }
            else
            {
                _isSliding = false;
                Player.Play();
                StartPositionTimer();
                _isPlaying = true;
                BtnPlayPause.Content = "\uE769"; // Pause icon
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source != null)
            {
                try { Player.Stop(); Player.Position = TimeSpan.Zero; } catch { }
            }
            StopPositionTimer();
            SldPosition.Value = 0;
            LblNow.Text = "00:00";
            _isPlaying = false;
            BtnPlayPause.Content = "\uE768"; // Play icon
        }

        private void BtnSeekBack_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source == null) return;
            try
            {
                var target = Player.Position - TimeSpan.FromSeconds(5);
                if (target < TimeSpan.Zero) target = TimeSpan.Zero;
                Player.Position = target;
                SldPosition.Value = target.TotalSeconds;
                LblNow.Text = FormatTime(target);
            }
            catch { }
        }

        private void BtnSeekForward_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source == null) return;
            try
            {
                var maxSeconds = SldPosition.Maximum > 0 ? SldPosition.Maximum : (Player.NaturalDuration.HasTimeSpan ? Player.NaturalDuration.TimeSpan.TotalSeconds : 0);
                var target = Player.Position + TimeSpan.FromSeconds(5);
                if (maxSeconds > 0 && target.TotalSeconds > maxSeconds)
                    target = TimeSpan.FromSeconds(maxSeconds);
                Player.Position = target;
                SldPosition.Value = target.TotalSeconds;
                LblNow.Text = FormatTime(target);
            }
            catch { }
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan)
            {
                var total = Player.NaturalDuration.TimeSpan;
                SldPosition.Maximum = total.TotalSeconds;
                LblTotal.Text = FormatTime(total);
            }
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (_repeatEnabled && Player.Source != null)
            {
                try
                {
                    Player.Position = TimeSpan.Zero;
                    Player.Play();
                    StartPositionTimer();
                    _isPlaying = true;
                    BtnPlayPause.Content = "\uE769"; // Pause icon
                }
                catch { }
                return;
            }
            StopPositionTimer();
            try { Player.Stop(); Player.Position = TimeSpan.Zero; } catch { }
            SldPosition.Value = 0;
            LblNow.Text = "00:00";
            _isPlaying = false;
            BtnPlayPause.Content = "\uE768"; // Play icon
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (_isSliding) return;
                var pos = Player.Position;
                SldPosition.Value = pos.TotalSeconds;
                LblNow.Text = FormatTime(pos);
            }
            catch { }
        }

        private void SldPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Always reflect slider value to current time label for instant feedback
            try
            {
                var seconds = SldPosition.Value;
                LblNow.Text = FormatTime(TimeSpan.FromSeconds(seconds));
            }
            catch { }
        }

        private void SldPosition_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Jump to clicked position immediately
                var pos = e.GetPosition(SldPosition);
                var ratio = Math.Max(0, Math.Min(1, pos.X / Math.Max(1, SldPosition.ActualWidth)));
                var newVal = ratio * SldPosition.Maximum;
                SldPosition.Value = newVal;
                Player.Position = TimeSpan.FromSeconds(newVal);
                e.Handled = true; // prevent small-step default behavior
            }
            finally
            {
                _isSliding = true; // allow drag continuation if user holds
            }
        }
        private void SldPosition_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var seconds = SldPosition.Value;
                Player.Position = TimeSpan.FromSeconds(seconds);
            }
            finally
            {
                _isSliding = false;
            }
        }

        private void TxtQuestion_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void BtnQUp_Click(object sender, RoutedEventArgs e) => AdjustQuestion(+1);
        private void BtnQDown_Click(object sender, RoutedEventArgs e) => AdjustQuestion(-1);

        private void AdjustQuestion(int delta)
        {
            int current = 0;
            if (!int.TryParse(TxtQuestion.Text, out current)) current = 0;

            int minQ = 1, maxQ = 999;
            try
            {
                var t = SelectedTest();
                if (_catalog.QuestionsByTest.TryGetValue(t, out var list) && list.Count > 0)
                {
                    minQ = list.Min(s => s.Start);
                    maxQ = list.Max(s => s.End);
                }
            }
            catch { }

            var next = Math.Max(minQ, Math.Min(maxQ, current + delta));
            TxtQuestion.Text = PadQuestion(next);
        }

        private void TxtQuestion_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtQuestion.Text, out var q))
            {
                TxtQuestion.Text = PadQuestion(q);
            }
        }

        private void TxtQuestion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(TxtQuestion.Text, out var q))
                {
                    TxtQuestion.Text = PadQuestion(q);
                }
                e.Handled = true;
            }
        }

        private static string PadQuestion(int q)
        {
            return q.ToString("000");
        }

        private void TglRepeat_Checked(object sender, RoutedEventArgs e)
        {
            _repeatEnabled = true;
        }

        private void TglRepeat_Unchecked(object sender, RoutedEventArgs e)
        {
            _repeatEnabled = false;
        }

        private void TglMode_Checked(object sender, RoutedEventArgs e)
        {
            _isFullTestMode = true;
            TglMode.Content = "Full Test";
        }

        private void TglMode_Unchecked(object sender, RoutedEventArgs e)
        {
            _isFullTestMode = false;
            TglMode.Content = "Question";
        }

        private void ToggleMode()
        {
            TglMode.IsChecked = !TglMode.IsChecked;
        }

        private void HandleUpArrow()
        {
            if (_isFullTestMode)
            {
                // Full Test 모드: Test 번호 1 증가
                var currentIndex = CmbTest.SelectedIndex;
                if (currentIndex < CmbTest.Items.Count - 1)
                {
                    CmbTest.SelectedIndex = currentIndex + 1;
                }
            }
            else
            {
                // Question 모드: 문항 번호 1 증가
                BtnQUp_Click(this, new RoutedEventArgs());
            }
        }

        private void HandleDownArrow()
        {
            if (_isFullTestMode)
            {
                // Full Test 모드: Test 번호 1 감소
                var currentIndex = CmbTest.SelectedIndex;
                if (currentIndex > 0)
                {
                    CmbTest.SelectedIndex = currentIndex - 1;
                }
            }
            else
            {
                // Question 모드: 문항 번호 1 감소
                BtnQDown_Click(this, new RoutedEventArgs());
            }
        }

        private void HandleEnterKey()
        {
            if (_isFullTestMode)
            {
                // Full Test 모드: LOAD Full Test
                BtnPlayAll_Click(this, new RoutedEventArgs());
            }
            else
            {
                // Question 모드: LOAD Question
                BtnPlayPart_Click(this, new RoutedEventArgs());
            }
        }

        private static string FormatTime(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
            return $"{ts.Minutes:00}:{ts.Seconds:00}";
        }

        // Centralized timer control
        private void StartPositionTimer()
        {
            if (!_timer.IsEnabled) _timer.Start();
        }
        private void StopPositionTimer()
        {
            if (_timer.IsEnabled) _timer.Stop();
        }

        private void UpdateStatus(string text)
        {
            TxtStatus.Text = text;
        }

        // --- Persist last used folder ---
        private static string GetConfigDir()
        {
            var dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ToeicAudioHelper");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }
        private static string GetLastPathFile() => System.IO.Path.Combine(GetConfigDir(), "lastpath.txt");
        private static void SaveLastRoot(string path)
        {
            try { File.WriteAllText(GetLastPathFile(), path ?? string.Empty); } catch { }
        }
        private void LoadLastRoot()
        {
            try
            {
                var f = GetLastPathFile();
                if (File.Exists(f))
                {
                    var p = File.ReadAllText(f).Trim();
                    if (!string.IsNullOrWhiteSpace(p) && Directory.Exists(p))
                    {
                        _root = p;
                        TxtRoot.Text = _root;
                        Rescan();
                    }
                }
            }
            catch { }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            // Load theme preference
            try
            {
                var t = LoadTheme();
                if (t == "dark" || t == "light")
                {
                    ApplyTheme(t);
                    _themeName = t;
                    UpdateThemeToggleGlyph();
                }
                else
                {
                    ApplyTheme(_themeName);
                    UpdateThemeToggleGlyph();
                }
            }
            catch { }
            if (string.IsNullOrWhiteSpace(_root))
            {
                LoadLastRoot();
            }
        }
        private void UpdateThemeToggleGlyph() { }


        private void ApplyTheme(string name)
        {
            try
            {
                var dicts = Application.Current.Resources.MergedDictionaries;
                // Remove existing theme dicts
                for (int i = dicts.Count - 1; i >= 0; i--)
                {
                    var src = dicts[i].Source?.ToString() ?? string.Empty;
                    if (src.Contains("Themes/Theme.Dark.xaml") || src.Contains("Themes/Theme.Light.xaml"))
                        dicts.RemoveAt(i);
                }
                var uri = new Uri(name == "light" ? "UiKit.Wpf;component/Themes/Theme.Light.xaml" : "UiKit.Wpf;component/Themes/Theme.Dark.xaml", UriKind.RelativeOrAbsolute);
                dicts.Add(new ResourceDictionary { Source = uri });
            }
            catch { }
        }

        private static string GetThemeFile() => System.IO.Path.Combine(GetConfigDir(), "theme.txt");
        private static void SaveTheme(string name)
        {
            try { File.WriteAllText(GetThemeFile(), name ?? "dark"); } catch { }
        }
        private static string LoadTheme()
        {
            try
            {
                var f = GetThemeFile();
                if (File.Exists(f)) return File.ReadAllText(f).Trim();
            }
            catch { }
            return "dark";
        }
    }
}

