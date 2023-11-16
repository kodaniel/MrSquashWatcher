namespace MrSquashWatcher
{
    public class UserSettings
    {
        private record struct Cell(int Row, int Column);

        private static Lazy<UserSettings> _instance = new Lazy<UserSettings>(() => new UserSettings());
        public static UserSettings Instance => _instance.Value;

        private UserSettings()
        {
        }

        private HashSet<Cell> _selectedGrids = new HashSet<Cell>();

        public string Name
        {
            get => user.Default.name;
            private set => user.Default.name = value;
        }

        public string Email
        {
            get => user.Default.email;
            private set => user.Default.email = value;
        }

        public string Phone
        {
            get => user.Default.phone;
            private set => user.Default.phone = value;
        }

        public void SetUser(string name, string email, string phone)
        {
            Name = name;
            Email = email; 
            Phone = phone;
        }

        public void Load()
        {
            _selectedGrids = JsonConvert.DeserializeObject<HashSet<Cell>>(user.Default.watchinggames) ?? new HashSet<Cell>();
        }

        public void Save()
        {
            string output = JsonConvert.SerializeObject(_selectedGrids);
            user.Default.watchinggames = output;
            user.Default.Save();
        }

        public bool IsWatching(int row, int column)
        {
            return _selectedGrids.Contains(new Cell(row, column));
        }

        public void SetSelected(int row, int column, bool value)
        {
            var c = new Cell(row, column);

            if (value)
                _selectedGrids.Add(c);
            else
                _selectedGrids.Remove(c);
        }
    }
}
