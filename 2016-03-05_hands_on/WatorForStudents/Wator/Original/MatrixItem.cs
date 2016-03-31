using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VSS.Wator.Original
{
    public enum MatrixItemType
    {
        NONE,
        FISH,
        SHARK
    };

    public class MatrixItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public MatrixItemType Type { get; set; }
        public Color Color { get; set; } = Color.Blue;
        public int Energy { get; set; } = 0;
        public int Age { get; set; } = 0;
        public Boolean Moved { get; set; } = true;

        public IWatorWorld World { get; private set; }

        public MatrixItem(IWatorWorld world, MatrixItemType type, int x, int y, int value)
        {
            World = world;
            Type = type;
            X = x;
            Y = y;
            World.Grid[X, Y] = this;
            switch (type)
            {
                case MatrixItemType.FISH:
                    Age = value;
                    Energy = World.InitialFishEnergy;
                    Color = Color.White;
                    break;
                case MatrixItemType.SHARK:
                    Age = 0;
                    Energy = value;
                    Color = Color.Red;
                    break;
                default:
                    break;
            }
        }

        protected void Move(int x, int y)
        {
            World.Grid[X, Y] = null;
            World.Grid[x, y] = this;
            X = x;
            Y = y;
            Moved = true;
        }

        // execute one simulation step for this animal 
        // animal behaviour is implemented in the specific classes (fish, shark)
        public void ExecuteStep()
        {
            CheckIfAlreadyMoved();
            switch (Type)
            {
                case MatrixItemType.FISH:
                    ExecuteFishStep();
                    break;
                case MatrixItemType.SHARK:
                    ExecuteSharkStep();
                    break;
                default: break;
            }
        }

        // commit the current simulation step for this animal
        // resets the moved flag to prepare for the next simulation step
        public void Commit()
        {
            Moved = false;
        }

        // animals can spawn to create new children
        // specific spawning behaviour of animal is implemented in the specific classes
        public void Spawn()
        {
            switch (Type)
            {
                case MatrixItemType.FISH:
                    ExecuteFishSpawn();
                    break;
                case MatrixItemType.SHARK:
                    ExecuteSharkSpawn();
                    break;
                default: break;
            }
        }

        #region Private Helpers

        private void ExecuteFishSpawn()
        {
            int x, y;
            // Move to random cell if possible
            World.SelectNeighbor(MatrixItemType.NONE, X, Y, out x, out y);
            if (x != -1)
            {
                // when an empty cell is available
                // create a new fish on the cell
                new MatrixItem(World, MatrixItemType.FISH, x, y, 0);
                // reduce the age of the parent fish to make sure it is allowed to 
                // reproduce only every FishBreedTime steps
                Age -= World.FishBreedTime;
            }
        }

        private void ExecuteFishStep()
        {
            Age++;
            int x, y;
            // Move to random cell if possible
            World.SelectNeighbor(MatrixItemType.NONE, X, Y, out x, out y);
            // Perform move on free cell 
            if (x != -1) { Move(x, y); }
            // if the fish has reached a given age => spawn
            if (Age >= World.FishBreedTime) { Spawn(); }
        }

        private void ExecuteSharkStep()
        {
            // increase the age and reduce the energy in each time step
            Age++;
            Energy--;

            // try to find neighbouring fish
            int x, y;
            World.SelectNeighbor(MatrixItemType.FISH, X, Y, out x, out y);
            if (x != -1)
            {
                // if a neighbouring fish has been found:
                // eat the fish & move onto the cell of the fish
                Energy += World.Grid[x, y].Energy;
                Move(x, y);
            }
            else if (Energy > 0)
            {
                // no fish found on a neighbouring cell so move to a random empty neighbouring cell
                World.SelectNeighbor(MatrixItemType.NONE, X, Y, out x, out y);
                // only move if there is an empty cell
                if (x != -1) { Move(x, y); }
                if (Energy >= World.SharkBreedEnergy) { Spawn(); }
            }
            // Shark dies here
            else { World.Grid[X, Y] = null; }
        }

        private void ExecuteSharkSpawn()
        {
            // find a neighbouring empty cell
            int x, y;
            World.SelectNeighbor(MatrixItemType.NONE, X, Y, out x, out y);
            if (x != -1)
            {
                Energy = Energy / 2;
                // an empty cell is available
                // create a new shark on the empty cell
                // share the energy between the new and the old shark

                new MatrixItem(World, MatrixItemType.SHARK, x, y, Energy);
            }
        }

        private void CheckIfAlreadyMoved()
        {
            if (Moved)
            {
                throw new InvalidProgramException("Tried to move a fish twice in one time step.");
            }
        }
        #endregion
    }
}
