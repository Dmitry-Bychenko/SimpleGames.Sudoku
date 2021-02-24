using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SimpleGames.Sudoku {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Sudoku 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class Sudoku : IEquatable<Sudoku>, ISerializable, ICloneable {
    #region Private Data

    int[][] m_Items = Enumerable
      .Range(0, 9)
      .Select(_ => new int[9])
      .ToArray();

    #endregion Private Data

    #region Algorithm

    private (int row, int column)[] Empty() {
      List<(int row, int column)> result = new List<(int row, int column)>();

      for (int r = 0; r < m_Items.Length; ++r)
        for (int c = 0; c < m_Items.Length; ++c) {
          if (0 == m_Items[r][c])
            result.Add((r, c));
        }

      return result.ToArray();
    }

    private bool TrySet(int row, int column, int value) {
      if (value == 0)
        return true;

      bool IsUnique(IEnumerable<int> value) {
        HashSet<int> hs = new HashSet<int>();

        foreach (int v in value)
          if (v != 0 && !hs.Add(v))
            return false;

        return true;
      }

      int[] array = Row(row);
      array[column] = value;

      if (!IsUnique(array))
        return false;

      array = Column(column);
      array[row] = value;

      if (!IsUnique(array))
        return false;

      array = Square(SquareIndex(row, column));
      array[(row % 3) * 3 + column % 3] = value;

      if (!IsUnique(array))
        return false;

      return true;
    }

    private static int[][] ParseArray(string value) {
      if (string.IsNullOrWhiteSpace(value))
        return null;

      string empty = ".\t,0_?-=~xX*#";

      var data = value
        .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .Select(line => line
           .Where(c => !char.IsWhiteSpace(c))
           .Take(10)
           .Select(c => c >= '1' && c <= '9' ? c - '0' : empty.Contains(c) ? 0 : -1)
           .ToArray())
        .Take(10)
        .ToArray();

      if (data.Length != 9 || data.Any(line => line.Length != 9))
        return null;

      if (data.Any(line => line.Any(d => d < 0 || d > 9)))
        return null;

      return data;
    }

    #endregion Algorithm

    #region Create

    private Sudoku(SerializationInfo info, StreamingContext context) {
      if (info is null)
        throw new ArgumentNullException(nameof(info));

      int[][] data = ParseArray(info.GetString("data"));

      if (data is null)
        throw new ArgumentException("Incorrect data format.", nameof(info));

      m_Items = data;
    }

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public Sudoku() { }

    /// <summary>
    /// Clone
    /// </summary>
    public Sudoku Clone() {
      Sudoku result = new Sudoku();

      for (int r = 0; r < m_Items.Length; ++r)
        for (int c = 0; c < m_Items.Length; ++c)
          result.m_Items[r][c] = m_Items[r][c];

      return result;
    }

    /// <summary>
    /// Try Parse
    /// </summary>
    public static bool TryParse(string value, out Sudoku result) {
      int[][] data = ParseArray(value);

      if (data is null) {
        result = null;

        return false;
      }

      result = new Sudoku();

      for (int r = 0; r < 9; ++r)
        for (int c = 0; c < 9; ++c)
          result.m_Items[r][c] = data[r][c];

      return true;
    }

    /// <summary>
    /// Parse 
    /// </summary>
    public static Sudoku Parse(string value) {
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      if (TryParse(value, out Sudoku result))
        return result;

      throw new FormatException("Sudoku failed to be parsed from given string.");
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Row Count
    /// </summary>
    public int RowCount => m_Items.Length;

    /// <summary>
    /// Column Count
    /// </summary>
    public int ColumnCount => m_Items.Length <= 0 ? 0 : m_Items[0].Length;

    /// <summary>
    /// Solutions
    /// </summary>
    public IEnumerable<Sudoku> Solutions() {
      if (IsSolved) {
        yield return this;

        yield break;
      }

      if (!IsValid)
        yield break;

      Sudoku current = Clone();

      (int row, int column)[] cells = Empty();
      int[] values = new int[cells.Length];

      for (int i = 0; i < values.Length && i >= 0;) {
        var at = cells[i];

        int v = values[i] + 1;

        if (v > 9) {
          values[i] = 0;
          current[at.row, at.column] = 0;

          i -= 1;

          continue;
        }

        values[i] = v;

        if (!current.TrySet(at.row, at.column, v))
          continue;

        current[at.row, at.column] = v;

        i += 1;

        if (i >= values.Length) {
          yield return current.Clone();

          i -= 1;
        }
      }
    }

    /// <summary>
    /// Items (0 to clear cell)
    /// </summary>
    public int this[int row, int column] {
      get {
        if (row < 0 || row >= RowCount)
          throw new ArgumentOutOfRangeException(nameof(row));
        if (column < 0 || column >= ColumnCount)
          throw new ArgumentOutOfRangeException(nameof(column));

        return m_Items[row][column];
      }
      set {
        if (row < 0 || row > RowCount)
          throw new ArgumentOutOfRangeException(nameof(row));
        if (column < 0 || column > ColumnCount)
          throw new ArgumentOutOfRangeException(nameof(column));
        if (value < 0 || value > 9)
          throw new ArgumentOutOfRangeException(nameof(value));

        m_Items[row][column] = value;
      }
    }

    /// <summary>
    /// Items (0 to clear cell)
    /// </summary>
    public int this[(int row, int column) at] {
      get {
        if (at.row < 0 || at.row >= RowCount)
          throw new ArgumentOutOfRangeException(nameof(at.row));
        if (at.column < 0 || at.column >= ColumnCount)
          throw new ArgumentOutOfRangeException(nameof(at.column));

        return m_Items[at.row][at.column];
      }
      set {
        if (at.row < 0 || at.row >= RowCount)
          throw new ArgumentOutOfRangeException(nameof(at.row));
        if (at.column < 0 || at.column >= ColumnCount)
          throw new ArgumentOutOfRangeException(nameof(at.column));
        if (value < 0 || value > 9)
          throw new ArgumentOutOfRangeException(nameof(value));

        m_Items[at.row][at.column] = value;
      }
    }

    /// <summary>
    /// Row
    /// </summary>
    public int[] Row(int row) {
      if (row < 0 || row >= RowCount)
        throw new ArgumentOutOfRangeException(nameof(row));

      int[] result = new int[9];

      for (int c = 0; c < RowCount; ++c)
        result[c] = m_Items[row][c];

      return result;
    }

    /// <summary>
    /// Column
    /// </summary>
    public int[] Column(int column) {
      if (column < 0 || column >= ColumnCount)
        throw new ArgumentOutOfRangeException(nameof(column));

      int[] result = new int[RowCount];

      for (int r = 0; r < RowCount; ++r)
        result[r] = m_Items[r][column];

      return result;
    }

    /// <summary>
    /// Square
    /// </summary>
    /// <param name="square">square in [0..8] range</param>
    public int[] Square(int square) {
      if (square < 0 || square > 8)
        throw new ArgumentOutOfRangeException(nameof(square));

      int[] result = new int[9];

      for (int q = 0; q < 9; ++q) {
        int r = (square / 3) * 3 + q / 3;
        int c = (square % 3) * 3 + q % 3;

        result[q] = m_Items[r][c];
      }

      return result;
    }

    /// <summary>
    /// Square Index
    /// </summary>
    public int SquareIndex(int row, int column) {
      if (row < 0 || row >= RowCount)
        throw new ArgumentOutOfRangeException(nameof(row));
      if (column < 0 || column >= ColumnCount)
        throw new ArgumentOutOfRangeException(nameof(column));

      return (row / 3) * 3 + column / 3;
    }

    /// <summary>
    /// Square Index
    /// </summary>
    public int SquareIndex((int row, int column) at) => SquareIndex(at.row, at.column);

    /// <summary>
    /// Is current Sudoku a parent for a given child
    /// </summary>
    public bool IsParentFor(Sudoku child) {
      if (child is null)
        return false;

      for (int r = 0; r < m_Items.Length; ++r)
        for (int c = 0; c < m_Items.Length; ++c)
          if (m_Items[r][c] != 0 && m_Items[r][c] != child.m_Items[r][c])
            return false;

      return true;
    }

    /// <summary>
    /// Is current Sudoku a child of a given parent
    /// </summary>
    public bool IsChildOf(Sudoku parent) => parent is not null && parent.IsParentFor(this);

    /// <summary>
    /// Is Solved
    /// </summary>
    public bool IsSolved => IsValid && m_Items.All(line => line.All(d => d != 0));

    /// <summary>
    /// Hints (possible values)
    /// </summary>
    public int[] Hints(int row, int column) {
      if (row < 0 || row >= RowCount)
        throw new ArgumentOutOfRangeException(nameof(row));
      if (column < 0 || column >= ColumnCount)
        throw new ArgumentOutOfRangeException(nameof(column));

      if (m_Items[row][column] != 0)
        return new int[] { m_Items[row][column] };

      HashSet<int> excluded = new HashSet<int>();

      foreach (int v in Row(row))
        excluded.Add(v);

      foreach (int v in Column(column))
        excluded.Add(v);

      foreach (int v in Square(SquareIndex(row, column)))
        excluded.Add(v);

      HashSet<int> included = new HashSet<int>(Enumerable.Range(1, 9));

      included.ExceptWith(excluded);

      return included
        .Except(excluded)
        .Where(item => item != 0)
        .OrderBy(item => item)
        .ToArray();
    }

    /// <summary>
    /// Hints (possible values)
    /// </summary>
    public int[] Hints((int row, int column) at) => Hints(at.row, at.column);

    /// <summary>
    /// Kernel [Solution]: current sudoku and all cells with the only possible filling 
    /// </summary>
    public Sudoku Kernel() {
      Sudoku result = Clone();

      if (!result.IsValid)
        return result;

      while (true) {
        if (result.IsSolved)
          return result;

        bool found = false;

        for (int r = 0; r < result.m_Items.Length; ++r)
          for (int c = 0; c < result.m_Items[r].Length; ++c) {
            if (result.m_Items[r][c] != 0)
              continue;

            int[] hints = result.Hints(r, c);

            if (hints.Length == 1) {
              found = true;

              result.m_Items[r][c] = hints[0];
            }
          }

        if (!found)
          break;
      }

      return result;
    }

    /// <summary>
    /// Is Valid
    /// </summary>
    public bool IsValid {
      get {
        bool IsUnique(IEnumerable<int> value) {
          HashSet<int> hs = new HashSet<int>();

          foreach (int v in value)
            if (v != 0 && !hs.Add(v))
              return false;

          return true;
        }

        for (int q = 0; q < 9; ++q) {
          if (!IsUnique(Row(q)))
            return false;

          if (!IsUnique(Column(q)))
            return false;

          if (!IsUnique(Square(q)))
            return false;
        }

        return true;
      }
    }

    /// <summary>
    /// To String
    /// </summary>
    public override string ToString() =>
      string.Join(Environment.NewLine, m_Items
        .Select(line => string.Concat(line.Select(d => d == 0 ? '.' : (char)(d + '0')))));

    /// <summary>
    /// To Report 
    /// </summary>
    public string ToReport() {
      string Cell(int r, int c) => m_Items[r][c] > 0
        ? m_Items[r][c].ToString()
        : ".";

      string Record(int r) =>
        $"{Cell(r, 0)}{Cell(r, 1)}{Cell(r, 2)}|{Cell(r, 3)}{Cell(r, 4)}{Cell(r, 5)}|{Cell(r, 6)}{Cell(r, 7)}{Cell(r, 8)}";

      return string.Join(Environment.NewLine,
        Record(0),
        Record(1),
        Record(2),
        "--- --- ---",
        Record(3),
        Record(4),
        Record(5),
        "--- --- ---",
        Record(6),
        Record(7),
        Record(8)
      );
    }

    #endregion Public

    #region Operators

    /// <summary>
    /// Equals
    /// </summary>
    public static bool operator ==(Sudoku left, Sudoku right) {
      if (ReferenceEquals(left, right))
        return true;
      if (left is null || right is null)
        return false;

      return left.Equals(right);
    }

    /// <summary>
    /// Not Equals
    /// </summary>
    public static bool operator !=(Sudoku left, Sudoku right) {
      if (ReferenceEquals(left, right))
        return false;
      if (left is null || right is null)
        return true;

      return !left.Equals(right);
    }

    #endregion Operators

    #region IEquatable<Sudoku>

    /// <summary>
    /// Equals
    /// </summary>
    public bool Equals(Sudoku other) {
      if (ReferenceEquals(this, other))
        return true;
      if (other is null)
        return false;

      for (int r = 0; r < m_Items.Length; ++r)
        if (!m_Items[r].SequenceEqual(other.m_Items[r]))
          return false;

      return true;
    }

    /// <summary>
    /// Equals
    /// </summary>
    public override bool Equals(object o) => o is Sudoku other && Equals(other);

    /// <summary>
    /// Hash Code
    /// </summary>
    public override int GetHashCode() {
      int result = 0;

      for (int d = 0; d < 9; ++d)
        result ^= m_Items[d][d];

      return result;
    }

    #endregion IEquatable<Sudoku>

    #region ISerializable

    /// <summary>
    /// Object Data for serialization
    /// </summary>
    public void GetObjectData(SerializationInfo info, StreamingContext context) {
      if (info is null)
        throw new ArgumentNullException(nameof(info));

      info.AddValue("data", ToString());
    }

    #endregion ISerializable

    #region IClonable

    /// <summary>
    /// Clone
    /// </summary>
    object ICloneable.Clone() => Clone();

    #endregion IClonable
  }

}
