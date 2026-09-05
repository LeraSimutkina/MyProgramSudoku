namespace SudokuEcosystem.Models
{
    public class Cell
    {
        public int Value { get; set; }
        public bool IsFixed { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
    }
}