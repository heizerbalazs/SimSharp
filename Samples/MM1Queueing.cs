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
using System.Collections.Generic;

namespace SimSharp.Samples {
  public class MM1Queueing {
    private static readonly TimeSpan OrderArrivalTime = TimeSpan.FromMinutes(3.33);
    private static readonly TimeSpan ProcessingTime = TimeSpan.FromMinutes(2.5);
    
    private IEnumerable<Event> Source(Simulation env, Resource server) {
      while (true) {
        yield return env.TimeoutExponential(OrderArrivalTime);
        env.Process(Order(env, server));
      }
    }

    private IEnumerable<Event> Order(Simulation env, Resource server) {
      using (var req = server.Request()) {
        yield return req;
        yield return env.TimeoutExponential(ProcessingTime);
      }
    }

    public void Simulate() {
      var lambda = 1 / OrderArrivalTime.TotalMinutes;
      var mu = 1 / ProcessingTime.TotalMinutes;
      var rho = lambda / mu;
      var analyticWIP = rho / (1 - rho);
      var analyticLeadtime = 1 / (mu - lambda);
      var analyticWaitingtime = rho / (mu - lambda);

      var env = new Simulation(randomSeed: 1, defaultStep: TimeSpan.FromMinutes(1));
      var utilization = new TimeSeriesMonitor(env, name: "Utilization");
      var wip = new TimeSeriesMonitor(env, name: "WIP", collect: true);
      var leadtime = new SampleMonitor(name: "Lead time", collect: true);
      var waitingtime = new SampleMonitor(name: "Waiting time", collect: true);

      env.Log("Analytical results of this system:");
      env.Log("\tUtilization.Mean\tWIP.Mean\tLeadtime.Mean\tWaitingTime.Mean");
      env.Log("\t{0}\t{1}\t{2}\t{3}", rho, analyticWIP, analyticLeadtime, analyticWaitingtime);

      // example to create a running report of these measures every simulated week
      //var report = Report.CreateBuilder(env)
      //  .Add("Utilization", utilization, Report.Measures.Mean | Report.Measures.StdDev)
      //  .Add("WIP", wip, Report.Measures.Min | Report.Measures.Mean | Report.Measures.Max)
      //  .Add("Leadtime", leadtime, Report.Measures.Min | Report.Measures.Mean | Report.Measures.Max)
      //  .Add("WaitingTime", waitingtime, Report.Measures.Min | Report.Measures.Mean | Report.Measures.Max)
      //  .SetOutput(env.Logger) // use a "new StreamWriter("report.csv")" to direct to a file
      //  .SetSeparator("\t")
      //  .SetPeriodicUpdate(TimeSpan.FromDays(7), withHeaders: true)
      //  .Build();

      var summary = Report.CreateBuilder(env)
        .Add("Utilization", utilization, Report.Measures.Mean)
        .Add("WIP", wip, Report.Measures.Mean)
        .Add("Leadtime", leadtime, Report.Measures.Mean)
        .Add("WaitingTime", waitingtime, Report.Measures.Mean)
        .SetOutput(env.Logger)
        .SetSeparator("\t")
        .SetFinalUpdate(withHeaders: false) // creates a summary of the means at the end
        .SetTimeAPI(useDApi: true)
        .Build();

      env.Log("Simulated results of this system:");
      env.Log("");
      summary.WriteHeader(); // write the header just once

      for (var i = 0; i < 5; i++) {
        env.Reset(i + 1); // reset environment
        utilization.Reset(); // reset monitors
        wip.Reset();
        leadtime.Reset();
        waitingtime.Reset();
        var server = new Resource(env, capacity: 1) {
          Utilization = utilization,
          WIP = wip,
          LeadTime = leadtime,
          WaitingTime = waitingtime,
        };

        env.Process(Source(env, server));
        env.Run(TimeSpan.FromDays(365));
      }

      env.Log("");
      env.Log("Detailed results from the last run:");
      env.Log("");
      env.Log(utilization.Summarize());
      env.Log(wip.Summarize(maxBins: 10, histInterval: 2));
      env.Log(leadtime.Summarize(maxBins: 10, histInterval: 5));
      env.Log(waitingtime.Summarize(maxBins: 10, histInterval: 4));  ;
    }
  }
}
