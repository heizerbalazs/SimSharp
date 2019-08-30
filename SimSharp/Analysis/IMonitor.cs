﻿#region License Information
/* SimSharp - A .NET port of SimPy, discrete event simulation framework
Copyright (C) 2019  Heuristic and Evolutionary Algorithms Laboratory (HEAL)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.*/
#endregion

using System;

namespace SimSharp {
  public interface IMonitor {
    bool Active { get; set; }
    string Name { get; }

    string Summarize();


    event EventHandler Updated;
  }

  public interface INumericMonitor : IMonitor {
    bool Collect { get; }

    double Min { get; }
    double Max { get; }
    double Sum { get; }
    double Mean { get; }
    double StdDev { get; }
    double Last { get; }

    double GetMedian();
    double GetPercentile(double p);
  }

  public interface ISampleMonitor : INumericMonitor {
    void Add(double value);
  }

  public interface ITimeSeriesMonitor : INumericMonitor {
    void Increase(double value);
    void Decrease(double value);
    void UpdateTo(double value);
  }
}
