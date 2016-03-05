using System;
using System.Drawing;

namespace VSS.Wator.Original {
  public class Shark : Animal {
    // sharks are displayed as red dots
    public override Color Color {
      get { return Color.Red; }
    }

    // create and initialize a shark on the specified position in the given world
    public Shark(OriginalWatorWorld world, Point position, int energy)
      : base(world, position) {
      Energy = energy;
    }

    // execute one simulation step for this shark
    // sharks try to eat neighbouring fish if possible
    // otherwise they move to a random free neighbouring point
    // if a shark has eaten enough fish it spawns
    public override void ExecuteStep() {
      // assert that a the shark is never moved twice within one simulation step
      if (Moved) throw new InvalidProgramException("Tried to move a shark twice within one time step.");
      // increase the age and reduce the energy in each time step
      Age++;
      Energy--;
      // try to find neighbouring fish
      Point fish = World.SelectNeighbor(typeof(Fish), Position);
      if (fish.X != -1) {
        // if a neighbouring fish has been found:
        // eat the fish & move onto the cell of the fish
        Energy += World.Grid[fish.X, fish.Y].Energy;
        Move(fish);
      } else {
        // no fish found on a neighbouring cell so move to a random empty neighbouring cell
        Point free = World.SelectNeighbor(null, Position);
        // only move if there is an empty cell
        if (free.X != -1) Move(free);
      }

      // if the shark is not hungry then it spawns
      if (Energy >= World.SharkBreedEnergy) Spawn();
      // if the energy of the shark is zero it dies => clear the cell
      if (Energy <= 0) World.Grid[Position.X, Position.Y] = null;
    }

    // spawning behaviour of sharks
    protected override void Spawn() {
      // find a neighbouring empty cell
      Point free = World.SelectNeighbor(null, Position);
      if (free.X != -1) {
        // an empty cell is available
        // create a new shark on the empty cell
        // share the energy between the new and the old shark
        Shark shark = new Shark(World, free, Energy / 2);
        Energy = Energy / 2;
      }
    }
  }
}
