using System;
using System.Linq;


namespace WeatherLab
{
    class Program
    {
        static string dbfile = @".\data\climate.db";

        static void Main(string[] args)
        {
            var measurements = new WeatherSqliteContext(dbfile).Weather;
            var precipitationValues2020 = measurements.Where(measurement => measurement.year == 2020)
                                            .Select(measurement => measurement.precipitation);
            var total_2020_precipitation = precipitationValues2020.Sum();
            Console.WriteLine($"Total precipitation in 2020: {total_2020_precipitation} mm\n");

            //
            // Heating Degree days have a mean temp of < 18C
            //
            var hddList = measurements
                .Where(measurement => measurement.meantemp < 18)
                .GroupBy(
                    measurement => measurement.year,
                    measurement => measurement.meantemp,
                    (year, measurements) => new{
                        year = year,
                        count = measurements.Count()
                    }
                );

            //
            // Cooling degree days have a mean temp of >=18C
            //
            var cddList = measurements
                .Where(measurement => measurement.meantemp >= 18)
                .GroupBy(
                    measurement => measurement.year,
                    measurement => measurement,
                    (year, measurements) => new{
                        year = year,
                        count = measurements.Count()
                    }
                );

            //
            // Most Variable days are the days with the biggest temperature
            // range. That is, the largest difference between the maximum and
            // minimum temperature
            //
            // Oh: and number formatting to zero pad.
            // 
            // For example, if you want:
            //      var x = 2;
            // To display as "0002" then:
            //      $"{x:d4}"
            //

            Console.WriteLine("Year\tHDD\tCDD");
            var yearlyMeasurements = cddList
                .Join(
                    hddList,
                    cdd => cdd.year,
                    hdd => hdd.year,
                    (cdd, hdd) => new
                    {
                        year = cdd.year,
                        hdd = hdd.count,
                        cdd = cdd.count
                    }
                ).OrderBy(yearlyMeasurement=> yearlyMeasurement.year);

            foreach (var yearlyMeasurement in yearlyMeasurements)
            {
                Console.WriteLine($"{yearlyMeasurement.year:d4}\t{yearlyMeasurement.hdd:d4}\t{yearlyMeasurement.cdd:d4}");
            }

            Console.WriteLine("\nTop 5 Most Variable Days");
            Console.WriteLine("YYYY-MM-DD\tDelta");
            var dayTemperatureVariationMeasuremetns = measurements.Select(measurement => new
                                                        {
                                                            year = measurement.year,
                                                            month = measurement.month,
                                                            day = measurement.day,
                                                            delta = measurement.maxtemp - measurement.mintemp,
                                                        });
                var top5TemperatureVariationDays = dayTemperatureVariationMeasuremetns
                                                    .OrderByDescending(measurement=>measurement.delta)
                                                    .Take(5);
             foreach(var dayMeasurement in top5TemperatureVariationDays)
            {
                Console.WriteLine($"{dayMeasurement.year:d4}-{dayMeasurement.month:d2}-{dayMeasurement.day:d2}\t{dayMeasurement.delta:0.00}");
            }                      
        }
    }
}
