
namespace VSS.ToiletSimulation {
  public interface IQueue {
    int Count { get; }                     // number of queued jobs
    void Enqueue(IJob job);                // enqueue a new job
    bool TryDequeue(out IJob job); // fetch next job
    void CompleteAdding();
    bool IsCompleted { get; }
  }
}
