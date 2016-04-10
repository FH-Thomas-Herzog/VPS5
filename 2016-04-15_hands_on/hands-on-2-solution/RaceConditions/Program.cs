using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceConditions
{
    public class Program
    {
        private static readonly MyRaceConditionExample myRaceCondition = new MyRaceConditionExample();
        private static readonly RaceConditionExampleFixed raceConditionFixed = new RaceConditionExampleFixed();

        private static void Main(string[] args)
        {
            //myRaceCondition.Run();
            raceConditionFixed.Run();
        }
    }
}
