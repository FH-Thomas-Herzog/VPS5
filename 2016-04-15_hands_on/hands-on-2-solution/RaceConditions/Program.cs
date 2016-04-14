using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceConditions
{
    /// <summary>
    /// Main program which tests the three requested code samples.
    /// </summary>
    public class Program
    {

        private static void Main(string[] args)
        {
            // implemented simple race condition example
            SimpleRaceconditionExample simpleRaceCondition = new SimpleRaceconditionExample();
            // Implemented fix for the race condition example
            RaceConditionExampleFixed raceConditionFixed = new RaceConditionExampleFixed();

            // simple race conditions (synchrnoized)
            //simpleRaceCondition.Run(true);

            // simple race conditions (critical)
            //simpleRaceCondition.Run(false);
            
            // the fixed version of the reader / writter example
            raceConditionFixed.Run();

            // block cosnole window
            Console.Read();
        }
    }
}
