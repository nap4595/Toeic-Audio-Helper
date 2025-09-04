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
                PlayFile(path);
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
            PlayFile(seg.FilePath);
        }

        private void BtnPlayVocaLc_Click(object sender, RoutedEventArgs e)
        {
            var t = SelectedTest();
            if (_catalog.VocaLcByTest.TryGetValue(t, out var list) && list.Count > 0)
            {
                PlayFile(list[0].FilePath);
            }
            else MessageBox.Show($"Test {t:00} LC VOCA 파일을 찾을 수 없습니다.");
        }

        private void BtnPlayVocaRc_Click(object sender, RoutedEventArgs e)
        {
            var t = SelectedTest();
            if (_catalog.VocaRcByTest.TryGetValue(t, out var list) && list.Count > 0)
            {
                PlayFile(list[0].FilePath);
            }
            else MessageBox.Show($"Test {t:00} RC VOCA 파일을 찾을 수 없습니다.");
        }

        private void PlayFile(string path)
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
                Player.Play();
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("재생 중 오류: " + ex.Message);
            }
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source != null) Player.Play();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source != null) Player.Pause();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source != null) Player.Stop();
            _timer.Stop();
            SldPosition.Value = 0;
            LblNow.Text = "00:00";
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
            _timer.Stop();
            SldPosition.Value = 0;
            LblNow.Text = "00:00";
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
            if (_isSliding) return;
            // no-op: we drive slider from timer
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

        private static string FormatTime(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
            return $"{ts.Minutes:00}:{ts.Seconds:00}";
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
            if (string.IsNullOrWhiteSpace(_root))
            {
                LoadLastRoot();
            }
        }
    }
}
