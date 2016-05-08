using System;
using System.Drawing;

namespace VSS.Wator.Original {
  public class Fish : Animal {
    // fish are white
    public override Color Color {
      get { return Color.White; }
    }

    // create and initialize a new fish on the specified position of the given world
    public Fish(OriginalWatorWorld world, Point position, int age)
      : base(world, position) {
      Energy = world.InitialFishEnergy;
      Age = age;
    }

    // execute one simulation step for the fish
    // fish move around randomly and spawn when they reach a certain age
    public override void ExecuteStep() {
      // assert that the fish is moved only once in a simulation step
      if (Moved) throw new InvalidProgramException("Tried to move a fish twice in one time step.");
      // increase the age of the fish
      Age++;
      // find a random empty neighbouring cell
      Point free = World.SelectNeighbor(null, Position);

      // if an empty cell has been found => move there
      if (free.X != -1) Move(free);
      // if the fish has reached a given age => spawn
      if (Age >= World.FishBreedTime) Spawn();
    }

    // implements spawning behaviour of fish
    protected override void Spawn() {
      // find an empty neighbouring cell
      Point free = World.SelectNeighbor(null, Position);
      if (free.X != -1) {
        // when an empty cell is available
        // create a new fish on the cell
        Fish fish = new Fish(World, free, 0);
        // reduce the age of the parent fish to make sure it is allowed to 
        // reproduce only every FishBreedTime steps
        Age -= World.FishBreedTime;
      }
    }
  }
}
