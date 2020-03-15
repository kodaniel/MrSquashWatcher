using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace MrSquashWatcher
{
    public class UserSettings
    {
        private List<Cell> gridWatchings = new List<Cell>();

        private static UserSettings _instance;
        public static UserSettings Instance => _instance ?? (_instance = new UserSettings());

        private UserSettings()
        {

        }

        public void Load()
        {
            try
            {
                gridWatchings = JsonConvert.DeserializeObject<List<Cell>>(user.Default.watchinggames) ?? new List<Cell>();
            }
            catch
            {

            }
        }

        public void Save()
        {
            try
            {
                string output = JsonConvert.SerializeObject(gridWatchings);
                user.Default.watchinggames = output;
                user.Default.Save();
            }
            catch
            {

            }
        }

        public bool IsWatching(int row, int column)
        {
            return gridWatchings.Contains(new Cell(row, column));
        }

        public void SetWatching(int row, int column, bool value)
        {
            var c = new Cell(row, column);

            if (value)
            {
                if (!gridWatchings.Contains(c))
                    gridWatchings.Add(c);
            }
            else
            {
                gridWatchings.Remove(c);
            }
        }

        private class Cell : IEquatable<Cell>
        {
            public int Row { get; set; }
            public int Col { get; set; }

            public Cell(int r, int c)
            {
                Row = r;
                Col = c;
            }

            public bool Equals([AllowNull] Cell other)
            {
                return Row == other.Row && Col == other.Col;
            }
        }
    }
}
