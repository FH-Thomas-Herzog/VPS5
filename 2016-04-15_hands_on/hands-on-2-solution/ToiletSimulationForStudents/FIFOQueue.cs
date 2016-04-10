using System;

namespace VSS.ToiletSimulation {

  public class FIFOQueue : Queue {
    public FIFOQueue() { }

    public override void Enqueue(IJob job) {
      throw new NotImplementedException();
    }

    public override bool TryDequeue(out IJob job) {
      throw new NotImplementedException();
    }

    public override void CompleteAdding() {
      throw new NotImplementedException();
    }
  }
}
