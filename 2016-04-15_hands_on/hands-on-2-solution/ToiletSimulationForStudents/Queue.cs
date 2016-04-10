using System;
using System.Collections.Generic;

namespace VSS.ToiletSimulation {
  public abstract class Queue : IQueue {
    protected IList<IJob> queue;

    public int Count {
      get { throw new NotImplementedException(); }
    }

    protected Queue() {
      queue = new List<IJob>();
    }

    public abstract void Enqueue(IJob job);


    public abstract bool TryDequeue(out IJob job);


    public virtual void CompleteAdding() {
      throw new NotImplementedException();
    }

    public bool IsCompleted {
      get {
        throw new NotImplementedException();
      }
    }
  }
}
