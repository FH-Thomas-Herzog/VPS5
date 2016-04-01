using System;
using System.Threading;

namespace VSS.ToiletSimulation {
  public class PeopleGenerator {
    private int idSeed;
    private Random random;
    private ExponentialRandom exponentialRandom;

    public string Name { get; private set; }
    public IQueue Queue { get; private set; }


    public PeopleGenerator(string name, IQueue queue, int randomSeed) {
      random = new Random(randomSeed);
      exponentialRandom = new ExponentialRandom(random, 1.0 / Parameters.MeanArrivalTime);
      Name = name;
      Queue = queue;
    }


    public void Produce() {
      var thread = new Thread(new ThreadStart(Run));
      thread.Name = Name;
      thread.Start();
    }

    private void Run() {
      idSeed = 0;
      for (int i = 0; i < Parameters.JobsPerProducer; i++) {
        Thread.Sleep((int)Math.Round(exponentialRandom.NextDouble()));
        idSeed++;
        Queue.Enqueue(new Person(random, Name + " - Person " + idSeed.ToString("00")));
      }
    }
  }
}
