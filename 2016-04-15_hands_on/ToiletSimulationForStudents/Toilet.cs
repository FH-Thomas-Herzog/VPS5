using System;
using System.Threading;

namespace VSS.ToiletSimulation {
  public class Toilet {
    public string Name { get; private set; }
    public IQueue Queue { get; private set; }

    private Thread thread;
    private Toilet() { }
    public Toilet(string name, IQueue queue) {
      Name = name;
      Queue = queue;
    }

    public void Consume() {
      throw new NotImplementedException();
    }

    public void Run() {
      throw new NotImplementedException();
    }

  }
}
