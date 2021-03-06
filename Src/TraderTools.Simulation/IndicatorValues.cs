﻿using System;
using System.Collections.Generic;

namespace TraderTools.Simulation
{
    public class IndicatorValues
    {
        private float _value;

        // Value and time in ticks
        public List<(long TimeTicks, float? Value)> Values { get; } = new List<(long TimeTicks, float? Value)>(50000);

        public float Value
        {
            get
            {
                if (!HasValue) throw new ApplicationException("Indicator has no value. Check HasValue before calling Value");
                return _value;
            }
            set => _value = value;
        }

        public bool HasValue { get; set; }

        public float? this[int i] => Values[i].Value;

        public int Count => Values.Count;
    }
}