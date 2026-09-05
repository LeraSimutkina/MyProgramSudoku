using SudokuEcosystem.Models;
using SudokuEcosystem.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;


namespace SudokuEcosystem.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private ApiService _apiService;
        private ObservableCollection<ObservableCollection<CellViewModel>> _cells;
        private string _timeDisplay = "00:00:00";
        private DateTime _startTime;
        private Timer _gameTimer;
       
        private bool _isGameCompleted;
        private bool _isPaused;
        private int _boardSize = 9;
        private int _selectedNumber;
        private CellViewModel _selectedCell;
        private TimeSpan _elapsedTimeBeforePause;
        private string _currentDifficulty = "Easy";
        private int[,] _solution = new int[9, 9];
        private Random _random = new Random();

        // НОВЫЕ ПОЛЯ ДЛЯ ЖИЗНЕЙ
        private int _lives = 4;
        private bool _isGameOver;

        public ObservableCollection<ObservableCollection<CellViewModel>> Cells
        {
            get => _cells;
            set { _cells = value; OnPropertyChanged(); }
        }

        public string TimeDisplay
        {
            get => _timeDisplay;
            set { _timeDisplay = value; OnPropertyChanged(); }
        }

        public int Lives
        {
            get => _lives;
            set { _lives = value; OnPropertyChanged(); }
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            set { _isGameOver = value; OnPropertyChanged(); }
        }


        private int CalculateScore()
        {
            int score;

            switch (_currentDifficulty)
            {
                case "Easy":
                    score = 100;
                    break;

                case "Medium":
                    score = 200;
                    break;

                case "Hard":
                    score = 300;
                    break;

                case "Expert":
                    score = 500;
                    break;

                default:
                    score = 100;
                    break;
            }

            int elapsedSeconds =
     (int)(DateTime.Now - _startTime).TotalSeconds;

            if (elapsedSeconds <= 300)
                score += 300;
            else if (elapsedSeconds <= 600)
                score += 200;
            else if (elapsedSeconds <= 1200)
                score += 100;

            int lostLives = 4 - Lives;

            score -= lostLives * 25;

            return Math.Max(score, 0);
        }


        private bool FillBoard(int[,] board)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (board[row, col] == 0)
                    {
                        List<int> numbers = Enumerable.Range(1, 9)
                                                      .OrderBy(x => _random.Next())
                                                      .ToList();

                        foreach (int num in numbers)
                        {
                            if (IsValid(board, row, col, num))
                            {
                                board[row, col] = num;

                                if (FillBoard(board))
                                    return true;

                                board[row, col] = 0;
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }


        private bool IsValid(int[,] board, int row, int col, int num)
        {
            for (int x = 0; x < 9; x++)
            {
                if (board[row, x] == num)
                    return false;

                if (board[x, col] == num)
                    return false;
            }

            int startRow = row - row % 3;
            int startCol = col - col % 3;

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (board[startRow + r, startCol + c] == num)
                        return false;
                }
            }

            return true;
        }

        private bool _internalUpdate;

        public ICommand SelectNumberCommand { get; }
        public ICommand ClearCellCommand { get; }
        public ICommand NewGameCommand { get; }
        public ICommand CheckSolutionCommand { get; }
        public ICommand ShowHintCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }

        public event Action<bool> PauseStateChanged;
        public event Action GameOverRequested; // НОВОЕ СОБЫТИЕ ДЛЯ ВОЗВРАТА В ГЛАВНОЕ МЕНЮ

        public GameViewModel()
        {
            _apiService = App.Api;
            SelectNumberCommand = new RelayCommand(SelectNumber);
            ClearCellCommand = new RelayCommand(ClearCell);
            NewGameCommand = new RelayCommand(_ => NewGame());
            CheckSolutionCommand = new RelayCommand(_ => CheckSolution());
            ShowHintCommand = new RelayCommand(_ => ShowHint());
            PauseCommand = new RelayCommand(_ => PauseGame());
            ResumeCommand = new RelayCommand(_ => ResumeGame());

            NewGame();
        }

        public void SetSelectedNumber(int number)
        {
            _selectedNumber = number;

            if (_selectedCell != null)
            {
                SetCellValue(_selectedCell, number);
            }
        }
        public void SetSelectedCell(CellViewModel cell)
        {
            _selectedCell = cell;
        }
        public void SetDifficulty(string difficulty)
        {
            _currentDifficulty = difficulty;
            NewGame();
        }

        private void NewGame()
        {
            _isGameOver = false;
            _lives = 4;
            _isGameCompleted = false;
            _isPaused = false;

            OnPropertyChanged(nameof(Lives));
            OnPropertyChanged(nameof(IsGameOver));

            InitializeEmptyBoard();
            FillRandomNumbers();
            StartTimer();
        }

        private void InitializeEmptyBoard()
        {
            Cells = new ObservableCollection<ObservableCollection<CellViewModel>>();
            for (int i = 0; i < _boardSize; i++)
            {
                var row = new ObservableCollection<CellViewModel>();
                for (int j = 0; j < _boardSize; j++)
                {
                    row.Add(new CellViewModel(new Cell { Value = 0, IsFixed = false, Row = i, Col = j }, this));
                }
                Cells.Add(row);
            }
        }
        private void GenerateSudokuSolution()
        {
            _solution = new int[9, 9];
            FillBoard(_solution);
        }

        private void FillRandomNumbers()
        {
            GenerateSudokuSolution();

            int[,] puzzle = (int[,])_solution.Clone();

            int cellsToRemove;

            switch (_currentDifficulty)
            {
                case "Easy":
                    cellsToRemove = 35;
                    break;

                case "Medium":
                    cellsToRemove = 45;
                    break;

                case "Hard":
                    cellsToRemove = 55;
                    break;

                default:
                    cellsToRemove = 35;
                    break;
            }

            while (cellsToRemove > 0)
            {
                int row = _random.Next(9);
                int col = _random.Next(9);

                if (puzzle[row, col] != 0)
                {
                    puzzle[row, col] = 0;
                    cellsToRemove--;
                }
            }

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    Cells[row][col].Value = puzzle[row, col];
                    Cells[row][col].IsFixed = puzzle[row, col] != 0;
                }
            }
        }

        private void StartTimer()
        {
            _gameTimer?.Stop();
            _gameTimer = new Timer(1000);
            _gameTimer.Elapsed += (s, e) =>
            {
                if (!_isPaused && !_isGameCompleted && !_isGameOver)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var elapsed = DateTime.Now - _startTime;
                        TimeDisplay = elapsed.ToString(@"hh\:mm\:ss");
                    });
                }
            };
            _startTime = DateTime.Now;
            _gameTimer.Start();
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                OnPropertyChanged();
            }
        }

        public bool IsGameCompleted
        {
            get => _isGameCompleted;
            set
            {
                _isGameCompleted = value;
                OnPropertyChanged();
            }
        }

        private void PauseGame()
        {
            if (_isGameCompleted || _isGameOver) return;

            IsPaused = true;
            _gameTimer?.Stop();

            _elapsedTimeBeforePause = DateTime.Now - _startTime;

            PauseStateChanged?.Invoke(true);
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;

            IsPaused = false;

            _startTime = DateTime.Now - _elapsedTimeBeforePause;

            _gameTimer?.Start();

            PauseStateChanged?.Invoke(false);
        }

        private void SelectNumber(object parameter)
        {
            if (_isGameCompleted || _isPaused || _isGameOver) return;

            if (parameter is int num)
            {
                _selectedNumber = num;
            }
            else if (parameter is string str && int.TryParse(str, out int parsedNum))
            {
                _selectedNumber = parsedNum;
            }
        }

        private void ClearCell(object parameter)
        {
            if (_isGameCompleted || _isPaused || _isGameOver) return;

            if (parameter is CellViewModel cell && !cell.IsFixed)
            {
                cell.Value = 0;
            }
        }

        // ОСНОВНОЙ МЕТОД — ПРОВЕРКА ПРИ ВСТАВКЕ ЦИФРЫ
        public async void SetCellValue(CellViewModel cell, int value)
        {
            if (cell.IsFixed || _isGameCompleted || _isPaused || _isGameOver) return;

            int finalValue = value == 0 ? _selectedNumber : value;

            if (finalValue >= 1 && finalValue <= _boardSize)
            {
                // Проверяем, правильная ли цифра
                bool isCorrect = _solution[cell.Row, cell.Col] == finalValue;

                if (isCorrect)
                {
                    cell.Value = finalValue;
                    cell.IsWrong = false;

                    if (CheckCompletion())
                    {
                        _isGameCompleted = true;
                        _gameTimer.Stop();

                        // 1. Считаем ЧЕСТНЫЙ счёт один раз
                        int score = CalculateScore();
                        int timeSeconds = (int)(DateTime.Now - _startTime).TotalSeconds;

                        // 2. Отправляем именно его на сервер
                        await SaveProgressToApi(score, timeSeconds);

                        MessageBox.Show(
                            $"Судоку успешно решено!\n\nВаш счёт: {score}\nВремя: {TimeDisplay}",
                            "Победа!",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                else
                {
                    cell.IsWrong = true;
                    cell.Value = finalValue;

                    Lives--;
                    OnPropertyChanged(nameof(Lives));

                    if (Lives <= 0)
                    {
                        GameOver();
                    }
                    else
                    {
                        var timer = new Timer(1000);
                        timer.Elapsed += (s, e) =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                cell.IsWrong = false;
                                cell.Value = 0;
                            });

                            timer.Stop();
                            timer.Dispose();
                        };

                        timer.Start();
                    }
                }
            }
        }

        // ПРОВЕРКА, МОЖНО ЛИ ПОСТАВИТЬ ЦИФРУ
        private bool IsMoveValid(int row, int col, int value)
        {
            // Проверка строки
            for (int j = 0; j < _boardSize; j++)
                if (j != col && Cells[row][j].Value == value) return false;

            // Проверка столбца
            for (int i = 0; i < _boardSize; i++)
                if (i != row && Cells[i][col].Value == value) return false;

            // Проверка квадрата 3x3
            if (_boardSize == 9)
            {
                int boxRow = (row / 3) * 3;
                int boxCol = (col / 3) * 3;
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        if ((boxRow + i != row || boxCol + j != col) &&
                            Cells[boxRow + i][boxCol + j].Value == value) return false;
            }

            return true;
        }

        private void GameOver()
        {
            MessageBox.Show("GameOver вызван");
            if (_isGameOver) return;

            _isGameOver = true;
            _gameTimer?.Stop();

            MessageBox.Show("💔 Игра провалена! У вас закончились жизни.",
                "Поражение", MessageBoxButton.OK, MessageBoxImage.Warning);

            GameOverRequested?.Invoke();
        }

        private bool CheckCompletion()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (Cells[row][col].Value != _solution[row, col])
                        return false;
                }
            }

            return true;
        }

        private async void CheckSolution()
        {
            if (_isPaused)
            {
                MessageBox.Show("Сначала снимите игру с паузы!", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CheckCompletion())
            {
                _isGameCompleted = true;
                _gameTimer.Stop();

                int score = CalculateScore();
                int timeSeconds = (int)(DateTime.Now - _startTime).TotalSeconds;

                await SaveProgressToApi(score, timeSeconds);

                MessageBox.Show("Поздравляем! Судоку решено правильно!\n" +
                    $"Сложность: {_currentDifficulty}\n" +
                    $"Ваш счёт: {score}\n" +
                    $"Время: {TimeDisplay}",
                    "Победа!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Решение пока не завершено. Продолжайте!",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowHint()
        {
            if (_isPaused)
            {
                MessageBox.Show("Сначала снимите игру с паузы!", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            for (int i = 0; i < _boardSize; i++)
            {
                for (int j = 0; j < _boardSize; j++)
                {
                    if (Cells[i][j].Value == 0 && !Cells[i][j].IsFixed)
                    {
                        int hintValue = FindPossibleValue(i, j);
                        if (hintValue > 0)
                        {
                            Cells[i][j].Value = hintValue;
                            MessageBox.Show($"Подсказка: в ячейку [{i + 1},{j + 1}] можно поставить {hintValue}",
                                "Подсказка", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                }
            }
            MessageBox.Show("Нет доступных подсказок!", "Подсказка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private int FindPossibleValue(int row, int col)
        {
            for (int val = 1; val <= _boardSize; val++)
            {
                bool valid = true;
                for (int j = 0; j < _boardSize; j++)
                    if (Cells[row][j].Value == val) { valid = false; break; }
                for (int i = 0; i < _boardSize; i++)
                    if (Cells[i][col].Value == val) { valid = false; break; }
                if (_boardSize == 9)
                {
                    int boxRow = (row / 3) * 3;
                    int boxCol = (col / 3) * 3;
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < 3; j++)
                            if (Cells[boxRow + i][boxCol + j].Value == val)
                            { valid = false; break; }
                }
                if (valid) return val;
            }
            return 0;
        }

        private async Task SaveProgressToApi(int finalScore, int timeSeconds)
        {
            try
            {
                // Если сервис говорит, что мы не авторизованы, попробуем всё равно отправить
                // (или выведем предупреждение, чтобы вы знали, в чём дело)
                if (!_apiService.IsAuthenticated)
                {
                    // Если у вас в ApiService токен затирается при создании нового объекта `new ApiService()`,
                    // сервер вернет 401 Unauthorized. Проверим это:
                    MessageBox.Show("Предупреждение: Сервис сообщает, что вы не авторизованы. Пробуем отправить результат...", "Синхронизация", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Отправляем правильные данные в базу данных
                await _apiService.SaveProgressAsync(_currentDifficulty, finalScore, timeSeconds);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки результата на сервер лидерборда:\n{ex.Message}",
                    "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // CellViewModel с поддержкой подсветки ошибок
    public class CellViewModel : INotifyPropertyChanged
    {
        private Cell _cell;
        private GameViewModel _gameViewModel;
        private int _value;
        private bool _isFixed;
        private bool _isWrong;

        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    _cell.Value = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayValue));
                    OnPropertyChanged(nameof(BackgroundColor));

                    // ТОЛЬКО если значение НЕ ноль, ячейка НЕ фиксированная и игра НЕ окончена
                   
                }
            }
        }

        public string DisplayValue
        {
            get => _value == 0 ? string.Empty : _value.ToString();
            set
            {
                if (int.TryParse(value, out int newValue))
                {
                    Value = newValue;
                }
                else if (string.IsNullOrEmpty(value))
                {
                    Value = 0;
                }
            }
        }

        public bool IsFixed
        {
            get => _isFixed;
            set
            {
                _isFixed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayValue));
                OnPropertyChanged(nameof(TextColor));
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        public bool IsWrong
        {
            get => _isWrong;
            set
            {
                _isWrong = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        public string BackgroundColor
        {
            get
            {
                if (_isWrong) return "#FF6666";
                if (_value == 0) return "#FFFFD0D0";
                return "White";
            }
        }

        public string TextColor
        {
            get
            {
                if (IsFixed) return "#FFFF69B4";
                return "#FFB22222";
            }
        }

        public int Row => _cell.Row;
        public int Col => _cell.Col;

        public CellViewModel(Cell cell, GameViewModel gameViewModel)
        {
            _cell = cell;
            _gameViewModel = gameViewModel;
            _value = cell.Value;
            _isFixed = cell.IsFixed;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}