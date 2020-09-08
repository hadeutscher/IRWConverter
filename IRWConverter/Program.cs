/* Copyright (C) 2015-2020 Yuval Deutscher

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace IRWConverter
{
    class Program
    {
        static string inFile = "";
        static string outDir = "";
        static ZipArchive zip;


        static StreamReader ReadFile(string name)
        {
            Stream s = zip.GetEntry(name).Open();
            return new StreamReader(s);
        }

        static List<string> ReadLines(StreamReader sr)
        {
            List<string> result = new List<string>();
            string line = sr.ReadLine();
            while (line != null)
            {
                result.Add(line);
                line = sr.ReadLine();
            }
            return result;
        }

        static List<string[]> ReadSplitFile(string name)
        {
            using (StreamReader sr = ReadFile(name))
            {
                return ReadLines(sr).Select(x => x.Split(",".ToCharArray())).ToList();
            }
        }

        static void WriteFile(string name, IEnumerable<string> lines)
        {
            File.WriteAllLines(Path.Combine(outDir, name), lines);
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: IRWConverter.exe <input zip> <output dir>");
                return;
            }
            inFile = args[0];
            outDir = args[1];
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }
            using (zip = new ZipArchive(File.OpenRead(inFile), ZipArchiveMode.Read))
            {
                IEnumerable<string> data;
                List<string[]> splitData;

                Console.WriteLine("Reading agency.txt");
                data = ReadLines(ReadFile("agency.txt"));
                string irw_agency_id = data.Where(x => x.Contains("rail")).ElementAt(0).Split(",".ToCharArray())[0];
                WriteFile("agency.txt", data);

                Console.WriteLine("Reading routes.txt");
                data = ReadLines(ReadFile("routes.txt"));
                HashSet<string> irw_routes = new HashSet<string>();
                List<string> routesOutLines = new List<string>();
                routesOutLines.Add(data.ElementAt(0));
                foreach (string line in data.Skip(1))
                {
                    string[] route = line.Split(",".ToCharArray());
                    if (route[1] == irw_agency_id)
                    {
                        irw_routes.Add(route[0]);
                        routesOutLines.Add(line);
                    }
                }
                WriteFile("routes.txt", routesOutLines);

                Console.WriteLine("Reading trips.txt");
                data = ReadLines(ReadFile("trips.txt"));
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
                data = ReadLines(ReadFile("calendar.txt"));
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

                using (StreamReader sr = ReadFile("stop_times.txt"))
                {
                    stopTimesOutLines.Add(sr.ReadLine());
                    int i = 0;
                    string dataLine = sr.ReadLine();
                    while (dataLine != null)
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
                        dataLine = sr.ReadLine();
                    }
                }
                WriteFile("stop_times.txt", stopTimesOutLines);

                Console.WriteLine("Reading stops.txt");
                data = ReadLines(ReadFile("stops.txt"));
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
}
