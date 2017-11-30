using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Sudoku
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        TextBox tb;                                 //文本框
        Grid g;                                     //布局控件
        MenuItem mi;                                //菜单
        Random ran = new Random();                  //随机数
        int[,] sudoku = new int[9, 9];               //存储所有数字
        bool notLegal = true;                      //所有数字是否合法
        int count;                              //每个宫产生多少个数字
        List<int> index = new List<int>();         //每个宫显示随机数的索引
        int random;                                 //生成的随机数
        List<int> row = new List<int>();        //一行
        List<List<int>> rows = new List<List<int>>();     //所有行
        List<int> col = new List<int>();        //一列
        List<List<int>> cols = new List<List<int>>();     //所有列
        List<int> palace = new List<int>();     //一个宫
        List<List<int>> palaces = new List<List<int>>();    //所有宫
        List<int> temp = new List<int>();       //所在宫与所在行和所在列的交集
        int currentRow;                      //当前行
        int currentCol;                     //当前列
        DispatcherTimer dt = new DispatcherTimer();        //计算时间线程
        bool isStart = true;      //是否开始
        int time;               //用时
        int textChanged = 0;        //输入的傎
        int hard1 = 3;          //难度下限
        int hard2 = 5;          //难度上限
        Thread t;               //计算数值线程
        bool isEnd = false;         //游戏是否结束
        FileStream fs;          //文件流
        StreamWriter sw;        //写入记录
        StreamReader sr;        //读取文件
        string readTag;            //读取的标记
        string readText;            //读取的文本
        string readString;          //读取到的全部字符串
        List<string> record = new List<string>();       //所有记录
        int recordCount;                    //记录条数
        ComboBox cb;
        Button b;
        List<int> request = new List<int>();
        List<List<int>> requestes = new List<List<int>>();
        bool right = false;                 //求解数字是否合法
        string tempString;

        #region Initialize
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Game Thread
        private void Dt_Tick(object sender, EventArgs e)
        {
            timer.Text = time / 60 / 60 + " 时 " + time / 60 % 60 + " 分 " + time % 60 + " 秒 ";
            time++;
        }
        #endregion

        #region Start Game
        public void StartGame()
        {
            //布局文本框
            //宫
            for (int i = 0; i < 9; i++)
            {
                g = gameGrid.Children[i] as Grid;
                g.Children.Clear();
                //开始游戏
                if (isStart)
                {
                    index.Clear();
                    for (int k = 0; k < 9; k++)
                    {
                        index.Add(k);
                    }
                    count = 9 - ran.Next(hard1, hard2);
                    for (int k = 0; k < count; k++)
                    {
                        index.RemoveAt(ran.Next(0, index.Count));
                    }
                }
                //宫内文本框
                for (int j = 0; j < 9; j++)
                {
                    tb = new TextBox();
                    //开始游戏
                    if (isStart)
                    {
                        tb.Tag = sudoku[i, j].ToString();
                        if (index.Contains(j))
                        {
                            tb.Text = tb.Tag.ToString();
                            tb.IsEnabled = false;
                        }
                        else
                        {
                            tb.Text = "";
                        }
                    }
                    //继续游戏
                    else
                    {
                        try
                        {
                            if (readTag != "")
                            {
                                tb.Tag = readTag.Substring(i * 9 + j, 1);
                            }
                            tb.Text = readText.Substring(i * 9 + j, 1);
                            if (tb.Text == "0")
                            {
                                tb.Text = "";
                            }
                            else
                            {
                                tb.IsEnabled = false;
                            }
                        }
                        catch
                        {
                            MessageBox.Show("文件读取失败", "异常", MessageBoxButton.OK, MessageBoxImage.Information);
                            for (int k = 0; k < 9; k++)
                            {
                                g = gameGrid.Children[k] as Grid;
                                g.Children.Clear();
                            }
                            return;
                        }
                    }
                    if (tb.IsEnabled)
                    {
                        tb.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        tb.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    Grid.SetColumn(tb, j % 3);
                    Grid.SetRow(tb, j / 3);
                    g.Children.Add(tb);
                }
            }
            dt.Start();
            //计算候选数字
            CalculateCandidateValues();
        }
        #endregion

        #region Click Menu
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            mi = sender as MenuItem;
            e.Handled = true;
            this.Height = 400;
            if (mainGrid.Children.Count > 3)
            {
                mainGrid.Children.RemoveAt(3);
            }
            if (mi.Header.ToString() == "开始")
            {
                isStart = true;
                StartGame();
                time = 0;
                status.Text = "进行中";
                if (hard1 == 3)
                {
                    harder.Text = "简单";
                }
                else if (hard1 == 2)
                {
                    harder.Text = "一般";
                }
                else if (hard1 == 1)
                {
                    harder.Text = "困难";
                }
                else
                {
                    harder.Text = "抓狂";
                }
            }
            else if (mi.Header.ToString() == "简单")
            {
                hard1 = 3;
                hard2 = 6;
                harder.Text = mi.Header.ToString();
            }
            else if (mi.Header.ToString() == "一般")
            {
                hard1 = 2;
                hard2 = 5;
                harder.Text = mi.Header.ToString();
            }
            else if (mi.Header.ToString() == "困难")
            {
                hard1 = 1;
                hard2 = 4;
                harder.Text = mi.Header.ToString();
            }
            else if (mi.Header.ToString() == "抓狂")
            {
                hard1 = 0;
                hard2 = 4;
                harder.Text = mi.Header.ToString();
            }
            else if (mi.Header.ToString() == "继续")
            {
                try
                {
                    isStart = false;
                    if (File.Exists(@"C:\Program Files (x86)\实用工具箱\game\sudokutemp.txt"))
                    {
                        sr = new StreamReader(@"C:\Program Files (x86)\实用工具箱\game\sudokutemp.txt", Encoding.Default);
                        readString = sr.ReadToEnd();
                        sr.Close();
                        readTag = readString.Split(';')[0];
                        Int32.TryParse(readString.Split(';')[1], out time);
                        harder.Text = readString.Split(';')[2];
                        readText = readString.Split(';')[4];
                        StartGame();
                        status.Text = "进行中";
                    }
                    else
                    {
                        MessageBox.Show("没有找到文件", "异常", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("异常，文件内容可能被修改！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else if (mi.Header.ToString() == "保存")
            {
                if (status.Text == "未开始")
                {
                    MessageBox.Show("游戏尚未开始，不能保存！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                SaveGame();
            }
            else if (mi.Header.ToString() == "查解")
            {
                if (status.Text == "进行中")
                {
                    dt.Stop();
                    for (int i = 0; i < 9; i++)
                    {
                        g = gameGrid.Children[i] as Grid;
                        for (int j = 0; j < 9; j++)
                        {
                            tb = g.Children[j] as TextBox;
                            if (tb.IsEnabled)
                            {
                                tb.Text = tb.Tag.ToString();
                            }
                        }
                    }
                    status.Text = "已结束";
                }
            }
            else if (mi.Header.ToString() == "数独")
            {
                Process.Start("http://baike.baidu.com/view/961.htm");
            }
            else if (mi.Header.ToString() == "操作")
            {
                MessageBox.Show("数独盘面是个九宫，每一宫又分为九个小格。在这八十一格中给出一定的已知数字和解题条件，利用逻辑和推理，在其他的空格上填入1-9的数字。使1-9每个数字在每一行、每一列和每一宫中都只出现一次，所以又称“九宫格”。", "说明", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (mi.Header.ToString() == "关于")
            {
                MessageBox.Show("作者：潘滔（Pantao）\r\nQQ：735817834\r\n\r\nPS：\r\n  （1）程序有任何BUG或者界面显示问题请及时提出，方便作者改进。\r\n  （2）有好的建议也强烈建议加QQ提出。\r\n\r\n注意：版权归作者所有。", "作者资料", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (mi.Header.ToString() == "编辑")
            {
                if (status.Text == "未开始")
                {
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            tb = new TextBox();
                            Grid.SetColumn(tb, j % 3);
                            Grid.SetRow(tb, j / 3);
                            (gameGrid.Children[i] as Grid).Children.Add(tb);
                        }
                    }
                    return;
                }
                status.Text = "未开始";
                for (int i = 0; i < 9; i++)
                {
                    g = gameGrid.Children[i] as Grid;
                    for (int j = 0; j < 9; j++)
                    {
                        tb = g.Children[j] as TextBox;
                        if (!tb.IsEnabled)
                        {
                            tb.IsEnabled = true;
                        }
                        tb.Text = "";
                        tb.ToolTip = "";
                        tb.Tag = "";
                        tb.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }
            else if (mi.Header.ToString() == "读取")
            {
                readTag = "";
                record.Clear();
                isStart = false;
                status.Text = "未开始";
                dt.Stop();
                if (File.Exists(@"C:\Program Files (x86)\实用工具箱\game\sudokusave.txt"))
                {
                    this.Height = 420;
                    WrapPanel wp = new WrapPanel { Height = 20 };
                    Grid.SetRow(wp, 3);
                    mainGrid.Children.Add(wp);
                    sr = new StreamReader(@"C:\Program Files (x86)\实用工具箱\game\sudokusave.txt", Encoding.Default);
                    readString = sr.ReadToEnd();
                    sr.Close();
                    cb = new ComboBox { Width = 200, Margin = new Thickness(30, 0, 0, 0), };
                    b = new Button
                    {
                        Content = "确定",
                        Width = 100,
                        Margin = new Thickness(30, 0, 0, 0),
                    };
                    b.Click += B_Click;
                    (mainGrid.Children[3] as WrapPanel).Children.Add(cb);
                    (mainGrid.Children[3] as WrapPanel).Children.Add(b);
                    recordCount = readString.Split('\n').Length - 1;
                    for (int i = 0; i < recordCount; i++)
                    {
                        record.Add(readString.Split('\n')[i]);
                        cb.Items.Add(record[i].Split(';')[0]);
                    }
                }
                else
                {
                    MessageBox.Show("未找到相关文件", "异常", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (mi.Header.ToString() == "求解")
            {
                if (status.Text != "未开始")
                {
                    return;
                }
                char[,] board = new char[9, 9];
                for (int i = 0; i < 9; i++)
                {
                    g = gameGrid.Children[i] as Grid;
                    for (int j = 0; j < 9; j++)
                    {
                        string s = (g.Children[j] as TextBox).Text;
                        board[i / 3 * 3 + j / 3, i % 3 * 3 + j % 3] = s.Length < 1 ? '.' : s[0];
                    }
                }
                if (!IsValidSudoku(board) || !Solve(board, 0, 0))
                {
                    MessageBox.Show("糟糕，没有找到解，这个数独不对哦！", "数独", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                StringBuilder test = new StringBuilder();
                for (int i = 0; i < 9; i++)
                {
                    g = gameGrid.Children[i] as Grid;
                    for (int j = 0; j < 9; j++)
                    {
                        tb = g.Children[j] as TextBox;
                        int row = 3 * (i / 3) + j / 3;
                        int col = 3 * (i % 3) + j % 3;
                        tb.Text = board[row, col].ToString();
                    }
                }
            }
            else if (mi.Header.ToString() == "验证")
            {
                if (tb == null)
                {
                    return;
                }
                CheckAvailable();
                if (right)
                {
                    dt.Stop();
                    status.Text = "已结束";
                    MessageBox.Show("完美，你太厉害啦", "你赢啦", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("没对哦，请仔细检查", "数独", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        #endregion

        #region Solve Sudoku
        public string[,] FormatToLocate(char[,] board)
        {
            string[,] c = new string[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    c[i, j] = board[3 * (i / 3) + j / 3, 3 * (i % 3) + j % 3].ToString();
                }
            }
            return c;
        }
        public string FormatSudoku(char[,] board)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    sb.Append(board[i, j] + " ");
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }
        public bool IsValidSudoku(char[,] board)
        {
            for (int i = 0; i < 9; i++)
            {
                HashSet<Char> rows = new HashSet<Char>();
                HashSet<Char> cols = new HashSet<Char>();
                HashSet<Char> cube = new HashSet<Char>();
                for (int j = 0; j < 9; j++)
                {
                    if (board[i, j] != '.' && !rows.Add(board[i, j]))
                        return false;
                    if (board[j, i] != '.' && !cols.Add(board[j, i]))
                        return false;
                    if (board[i / 3 * 3 + j / 3, i % 3 * 3 + j % 3] != '.'
                            && !cube.Add(board[i / 3 * 3 + j / 3, i % 3 * 3 + j % 3]))
                        return false;
                }
            }
            return true;
        }
        public bool Solve(char[,] board, int i, int j)
        {
            if (j == 9)
            {
                if (i == 8)
                    return true;
                i++;
                j = 0;
            }
            if (board[i, j] != '.')
            {
                return Solve(board, i, j + 1);
            }
            for (char k = '1'; k <= '9'; k++)
            {
                if (IsValid(board, i, j, k))
                {
                    board[i, j] = k;
                    if (Solve(board, i, j + 1))
                        return true;
                    else
                        board[i, j] = '.';
                }
            }
            return false;
        }

        public bool IsValid(char[,] board, int i, int j, char c)
        {

            for (int k = 0; k < 9; k++)
            {
                if (board[i, k] != '.' && board[i, k] == c)
                    return false;
                if (board[k, j] != '.' && board[k, j] == c)
                    return false;
                if (board[i / 3 * 3 + k / 3, j / 3 * 3 + k % 3] != '.'
                        && board[i / 3 * 3 + k / 3, j / 3 * 3 + k % 3] == c)
                    return false;

            }
            return true;
        }
        #endregion


        #region Button Click
        private void B_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                readText = record[cb.SelectedIndex].Split(';')[1];
                StartGame();
                dt.Stop();
                status.Text = "已结束";
            }
            catch
            {
                MessageBox.Show("读取文件失败，发生了一些未知错误！", "异常", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region Calculate Number
        public void CalculateNumber()
        {
            //计算数字
            while (true)
            {
                while (isStart)
                {
                    sudoku = new int[9, 9];
                    while (notLegal)
                    {
                        rows.Clear();
                        cols.Clear();
                        palaces.Clear();
                        for (int q = 0; q < 9; q++)
                        {
                            for (int p = 1; p < 10; p++)
                            {
                                row.Add(p);
                                col.Add(p);
                                palace.Add(p);
                            }
                            rows.Add(row);
                            cols.Add(col);
                            palaces.Add(palace);
                            row = new List<int>();
                            col = new List<int>();
                            palace = new List<int>();
                        }
                        for (int i = 0; i < 9; i++)
                        {
                            for (int j = 0; j < 9; j++)
                            {
                                temp.Clear();
                                currentRow = (i / 3) * 3 + j / 3;
                                currentCol = (i % 3) * 3 + j % 3;
                                for (int x = 0; x < palaces[i].Count; x++)
                                {
                                    if ((cols[currentCol].Contains(palaces[i][x])) && (rows[currentRow].Contains(palaces[i][x])))
                                    {
                                        random = palaces[i][x];
                                        temp.Add(random);
                                    }
                                }
                                try
                                {
                                    random = temp[ran.Next(0, temp.Count)];
                                    notLegal = false;
                                }
                                catch
                                {
                                    random = 0;
                                    notLegal = true;
                                    if (notLegal)
                                    {
                                        break;
                                    }
                                }
                                sudoku[i, j] = random;
                                if (palaces[i].Contains(random))
                                {
                                    palaces[i].Remove(random);
                                }
                                if (rows[currentRow].Contains(random))
                                {
                                    rows[currentRow].Remove(random);
                                }
                                if (cols[currentCol].Contains(random))
                                {
                                    cols[currentCol].Remove(random);
                                }
                            }
                            if (notLegal)
                            {
                                break;
                            }
                        }
                    }
                    notLegal = true;
                    isStart = false;
                }
                Thread.Sleep(2000);
            }
        }
        #endregion

        #region Text Changed to Check
        public void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            tb = sender as TextBox;
            if (tb.Text != "")
            {
                try
                {
                    textChanged = Int32.Parse(tb.Text);
                    if (textChanged == 0)
                    {
                        tb.Text = "";
                    }
                }
                catch
                {
                    tb.Text = "";
                }
            }
            e.Handled = true;
        }
        #endregion

        #region Mouse Wheel to Mention
        public void TextBox_MouseWheel(object sender, RoutedEventArgs e)
        {
            tb = sender as TextBox;
            if (tb.Tag != null)
            {
                tb.Text = tb.Tag.ToString();
            }
            e.Handled = true;
        }
        #endregion

        #region Windwos Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += Dt_Tick;
            t = new Thread(CalculateNumber)
            {
                IsBackground = true
            };
            t.Start();
        }
        #endregion

        #region Check Available
        public void CheckAvailable()
        {
            //检测是否合法
            //检测结束
            isEnd = true;
            for (int i = 0; i < 9; i++)
            {
                g = gameGrid.Children[i] as Grid;
                for (int j = 0; j < 9; j++)
                {
                    tb = g.Children[j] as TextBox;
                    if (tb.Text == "")
                    {
                        isEnd = false;
                        right = false;
                        return;
                    }
                }
            }
            right = true;
            //宫
            request.Clear();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    request.Add(Int32.Parse(((gameGrid.Children[i] as Grid).Children[j] as TextBox).Text));
                }
                for (int j = 1; j < 10; j++)
                {
                    if (!request.Contains(j))
                    {
                        right = false;
                        break;
                    }
                }
                request = new List<int>();
                if (!right)
                {
                    return;
                }
            }
            //行
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    request.Add(Int32.Parse(((gameGrid.Children[(i / 3) * 3 + j / 3] as Grid).Children[(i % 3) * 3 + j % 3] as TextBox).Text));
                }
                for (int j = 1; j < 10; j++)
                {
                    if (!request.Contains(j))
                    {
                        right = false;
                        return;
                    }
                }
                request = new List<int>();
                if (!right)
                {
                    return;
                }
            }
            //列
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    request.Add(Int32.Parse(((gameGrid.Children[(j / 3) * 3 + i / 3] as Grid).Children[(j % 3) * 3 + i % 3] as TextBox).Text));
                }
                for (int j = 1; j < 10; j++)
                {
                    if (!request.Contains(j))
                    {
                        right = false;
                        break;
                    }
                }
                request = new List<int>();
                if (!right)
                {
                    return;
                }
            }
        }
        #endregion

        #region Calculate Candidate Values
        public void CalculateCandidateValues()
        {
            //计算候选值
            requestes.Clear();
            //宫
            for (int i = 0; i < 9; i++)
            {
                g = gameGrid.Children[i] as Grid;
                for (int j = 0; j < 9; j++)
                {
                    tb = g.Children[j] as TextBox;
                    if (tb.Text != "")
                    {
                        if (tb.IsEnabled)
                        {
                            tb.IsEnabled = false;
                        }
                        request.Add(Int32.Parse(tb.Text));
                    }
                    else
                    {
                        for (int k = 1; k < 10; k++)
                        {
                            request.Add(k);
                        }
                        //检测宫
                        for (int k = 0; k < 9; k++)
                        {
                            tb = g.Children[k] as TextBox;
                            if (tb.Text != "" && request.Contains(Int32.Parse(tb.Text)))
                            {
                                request.Remove(Int32.Parse(tb.Text));
                            }
                        }
                        //检测行
                        for (int m = (i / 3) * 3; m < (i / 3 + 1) * 3; m++)
                        {
                            for (int n = (j / 3) * 3; n < (j / 3 + 1) * 3; n++)
                            {
                                tb = (gameGrid.Children[m] as Grid).Children[n] as TextBox;
                                if (tb.Text != "" && request.Contains(Int32.Parse(tb.Text)))
                                {
                                    request.Remove(Int32.Parse(tb.Text));
                                }
                            }
                        }
                        //检测列
                        for (int m = i % 3; m < 9; m += 3)
                        {
                            for (int n = j % 3; n < 9; n += 3)
                            {
                                tb = (gameGrid.Children[m] as Grid).Children[n] as TextBox;
                                if (tb.Text != "" && request.Contains(Int32.Parse(tb.Text)))
                                {
                                    request.Remove(Int32.Parse(tb.Text));
                                }
                            }
                        }
                    }
                    tempString = "候选值：";
                    for (int k = 0; k < request.Count; k++)
                    {
                        tempString += request[k] + " ";
                    }
                    (g.Children[j] as TextBox).ToolTip = tempString;
                    requestes.Add(request);
                    request = new List<int>();
                    tempString = "";
                }
            }
        }
        #endregion

        #region Windows Closing to Check Game Status
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (status.Text == "进行中")
            {
                dt.Stop();
                MessageBoxResult dr = MessageBox.Show("当前游戏尚未结束，是否保存？", "温馨提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (MessageBoxResult.Yes == dr)
                {
                    SaveGame();
                }
                else if (MessageBoxResult.Cancel == dr)
                {
                    e.Cancel = true;
                    dt.Start();
                }
                else
                {
                    //nothing
                }
            }
        }
        #endregion

        #region Save Game
        public void SaveGame()
        {
            if (!Directory.Exists(@"C:\Program Files (x86)\实用工具箱\game"))
            {
                Directory.CreateDirectory(@"C:\Program Files (x86)\实用工具箱\game");
            }
            if (!File.Exists(@"C:\Program Files (x86)\实用工具箱\game\sudokusave.txt"))
            {
                FileStream f = new FileStream(@"C:\Program Files (x86)\实用工具箱\game\sudokusave.txt", FileMode.Create);
                f.Close();
            }
            if (status.Text == "已结束")
            {
                //保存结束的游戏
                fs = new FileStream(@"C:\Program Files (x86)\实用工具箱\game\sudokusave.txt", FileMode.Append);
                sw = new StreamWriter(fs, Encoding.Unicode);
            }
            else
            {
                //保存进行中的游戏
                fs = new FileStream(@"C:\Program Files (x86)\实用工具箱\game\sudokutemp.txt", FileMode.Create);
                sw = new StreamWriter(fs, Encoding.Unicode);
                for (int i = 0; i < 9; i++)
                {
                    g = gameGrid.Children[i] as Grid;
                    for (int j = 0; j < 9; j++)
                    {
                        sw.Write((g.Children[j] as TextBox).Tag.ToString());
                    }
                }
                sw.Write(";");
                sw.Write(time + ";" + harder.Text + ";");
            }
            sw.Write(DateTime.Now.ToString() + ";");
            for (int i = 0; i < 9; i++)
            {
                g = gameGrid.Children[i] as Grid;
                for (int j = 0; j < 9; j++)
                {
                    tb = g.Children[j] as TextBox;
                    if (tb.Text == "")
                    {
                        sw.Write("0");
                    }
                    else
                    {
                        sw.Write(tb.Text);
                    }
                }
            }
            sw.Write(";\r\n");
            sw.Close();
            fs.Close();
            MessageBox.Show("游戏保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
    }
}
