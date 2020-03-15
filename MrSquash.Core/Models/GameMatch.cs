using Prism.Mvvm;
using System;

namespace MrSquash.Core
{
    public class GameMatch : BindableBase
    {
        private int _row;
        public int Row
        {
            get => _row;
            set => SetProperty(ref _row, value);
        }

        private int _column;
        public int Column
        {
            get => _column;
            set => SetProperty(ref _column, value);
        }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        private bool _busy;
        public bool Busy
        {
            get => _busy;
            set => SetProperty(ref _busy, value);
        }

        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set => SetProperty(ref _enabled, value);
        }

        private bool _watching;
        public bool Watching
        {
            get => _watching;
            set => SetProperty(ref _watching, value);
        }
    }
}
