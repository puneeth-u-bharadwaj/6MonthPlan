﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BegCShCollections.CollectionWithinCollections.ReadAllCountries
{
    public class LINQOrderBy
    {
        /// <summary>
        /// PadLeft(15) adds 15 characters to the left as margin
        /// </summary>
        public void Data()
        {
            string filePath = "../../LINQ/ReadAllCountries/Pop by Largest Final.csv";

            CsvReader reader = new CsvReader(filePath);

            List<Country> countries = reader.ReadAllCountries();

            foreach (Country country in countries.OrderBy(x => x.Name))
            {
                Console.WriteLine($"{PopulationFormatter.FormatPopulation(country.Population).PadLeft(15)}: {country.Name} ");
            }
        }
    }
}
