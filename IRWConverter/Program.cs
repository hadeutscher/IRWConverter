/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IRWConverter
{
    class Program
    {
        static string inDir = "";
        static string outDir = "";

        static IEnumerable<string> ReadFile(string name)
        {
            return File.ReadLines(inDir + name);
        }

        static List<string[]> ReadSplitFile(string name)
        {
            IEnumerable<string> data = ReadFile(name);
            return data.Select(x => x.Split(",".ToCharArray())).ToList();
        }

        static void WriteFile(string name, IEnumerable<string> lines)
        {
            File.WriteAllLines(outDir + name, lines);
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: IRWConverter.exe <input dir> <output dir>");
                return;
            }
            inDir = args[1];
            outDir = args[2];

            IEnumerable<string> data;
            List<string[]> splitData;

            Console.WriteLine("Reading agency.txt");
            data = ReadFile("agency.txt");
            string irw_agency_id = data.Where(x => x.Contains("rail")).ElementAt(0).Split(",".ToCharArray())[0];

            Console.WriteLine("Reading routes.txt");
            splitData = ReadSplitFile("routes.txt");
            HashSet<string> irw_routes = new HashSet<string>();
            foreach (string[] route in splitData)
            {
                if (route[1] == irw_agency_id)
                {
                    irw_routes.Add(route[0]);
                }
            }

            Console.WriteLine("Reading trips.txt");
            data = ReadFile("trips.txt");
            HashSet<string> irw_services = new HashSet<string>(), irw_trips = new HashSet<string>();
            List<string> tripsOutLines = new List<string>();
            tripsOutLines.Add(data.ElementAt(0));
            foreach (string line in data.Skip(1))
            {
                string[] trip = line.Split(",".ToCharArray());
                if (irw_routes.Contains(trip[0]))
                {
                    irw_services.Add(trip[1]);
                    irw_trips.Add(trip[2]);
                    tripsOutLines.Add(line);
                }
            }
            WriteFile("trips.txt", tripsOutLines);

            Console.WriteLine("Reading calendar.txt");
            data = ReadFile("calendar.txt");
            List<string> calOutLines = new List<string>();
            calOutLines.Add(data.ElementAt(0));
            foreach (string dataLine in data.Skip(1))
            {
                if (irw_services.Contains(dataLine.Split(",".ToCharArray())[0]))
                {
                    calOutLines.Add(dataLine);
                }
            }
            WriteFile("calendar.txt", calOutLines);

            Console.WriteLine("Reading stop_times.txt");
            HashSet<string> stations = new HashSet<string>();
            List<string> stopTimesOutLines = new List<string>();
            data = ReadFile("stop_times.txt");
            stopTimesOutLines.Add(data.ElementAt(0));
            int i = 0;
            foreach (string dataLine in data.Skip(1))
            {
                if (irw_trips.Contains(dataLine.Substring(0, dataLine.IndexOf(","))))
                {
                    stopTimesOutLines.Add(dataLine);
                    stations.Add(dataLine.Split(",".ToCharArray())[3]);
                }
                i++;
                if (i % 100000 == 0)
                {
                    Console.WriteLine(i);
                }
            }
            WriteFile("stop_times.txt", stopTimesOutLines);

            Console.WriteLine("Reading stops.txt");
            data = ReadFile("stops.txt");
            List<string> stopsOutLines = new List<string>();
            stopsOutLines.Add(data.ElementAt(0));
            foreach (string dataLine in data.Skip(1))
            {
                if (stations.Contains(dataLine.Split(",".ToCharArray())[0]))
                {
                    stopsOutLines.Add(dataLine);
                }
            }
            WriteFile("stops.txt", stopsOutLines);
        }
    }
}
