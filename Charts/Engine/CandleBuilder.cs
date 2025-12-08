using System;
using System.Collections.Generic;

namespace thecalcify.Charts
{
    public class CandleBuilder
    {
        private TimeSpan _interval;
        private Candle _current;
        private readonly List<Candle> _history = new List<Candle>();

        public CandleBuilder(TimeSpan interval)
        {
            _interval = interval;
        }

        public void SetTimeFrame(TimeSpan interval)
        {
            _interval = interval;
            _current = null;
            _history.Clear();
        }

        private DateTime Align(DateTime t)
        {
            long alignedTicks = (t.Ticks / _interval.Ticks) * _interval.Ticks;
            return new DateTime(alignedTicks, t.Kind); // keep Kind (UTC/Local)
        }

        /// <summary>
        /// Add tick. Volume is optional (if not available, pass null).
        /// </summary>
        public void AddTick(DateTime time, double price, double? volume = null)
        {
            DateTime bucket = Align(time);

            // first tick
            if (_current == null)
            {
                _current = new Candle
                {
                    OpenTime = bucket,
                    Interval = _interval,
                    Open = price,
                    High = price,
                    Low = price,
                    Close = price,
                    Volume = volume ?? 0
                };
                return;
            }

            // new candle: bucket changed
            if (bucket != _current.OpenTime)
            {
                _history.Add(_current);

                // fill gaps with flat candles
                DateTime missing = _current.OpenTime + _interval;
                while (missing < bucket)
                {
                    _history.Add(new Candle
                    {
                        OpenTime = missing,
                        Interval = _interval,
                        Open = _current.Close,
                        High = _current.Close,
                        Low = _current.Close,
                        Close = _current.Close,
                        Volume = 0
                    });

                    missing += _interval;
                }

                _current = new Candle
                {
                    OpenTime = bucket,
                    Interval = _interval,
                    Open = price,
                    High = price,
                    Low = price,
                    Close = price,
                    Volume = volume ?? 0
                };

                return;
            }

            // same candle: update OHLC + volume
            if (price > _current.High) _current.High = price;
            if (price < _current.Low) _current.Low = price;
            _current.Close = price;

            if (volume.HasValue)
                _current.Volume += volume.Value;
        }

        public IReadOnlyList<Candle> GetAll(bool includeCurrent = true)
        {
            var list = new List<Candle>(_history);
            if (includeCurrent && _current != null)
                list.Add(_current);
            return list;
        }

        /// <summary>
        /// Load historical OHLC candles (e.g. from REST API) as base state.
        /// </summary>
        public void LoadHistorical(IEnumerable<Candle> candles)
        {
            _history.Clear();
            _current = null;

            if (candles == null)
                return;

            // store copies in history
            foreach (var c in candles)
            {
                var clone = new Candle
                {
                    OpenTime = c.OpenTime,
                    Interval = _interval,   // use builder's interval
                    Open = c.Open,
                    High = c.High,
                    Low = c.Low,
                    Close = c.Close,
                    Volume = c.Volume
                };

                _history.Add(clone);
            }

            if (_history.Count > 0)
            {
                // last historical candle becomes current (manual copy, no C# 8 index, no copy ctor)
                var last = _history[_history.Count - 1];

                _current = new Candle
                {
                    OpenTime = last.OpenTime,
                    Interval = last.Interval,
                    Open = last.Open,
                    High = last.High,
                    Low = last.Low,
                    Close = last.Close,
                    Volume = last.Volume
                };
            }
        }
    }
}
