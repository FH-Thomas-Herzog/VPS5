using System.Drawing;

namespace VSS.Wator.Part3
{
    // base class for animals (fish & sharks)
    public abstract class Animal
    {

        // the world that this animal lives in
        // the animal can check neighboring cells
        public Part3WatorWorld World { get; private set; }

        // position of the animal in the world (x/y position)
        public Point Position { get; private set; }

        // the age of the animal (only relevant for fish)
        public int Age { get; protected set; }

        // the energy of the animal
        // sharks need to eat fish to increase energy
        // the energy of a fish is constant
        public int Energy { get; protected set; }

        // boolean flag that indicates wether an animal has moved in the current iteration
        public bool Moved { get; set; }

        // the color of the enimal (e.g. fish=white, shark=red)
        public abstract Color Color { get; }

        private bool changed = false;

        // ctor: create a new animal on the specified position of the given world
        public Animal(Part3WatorWorld world, Point position)
        {
            World = world;
            Position = position;
            Age = 0;
            //Moved = true;
            Energy = 0;
            // place the new animal in the world
            World.Grid[position.X, position.Y] = this;
        }

        // move the animal to a given position
        // does not check if the position can be reached by the animal
        protected void Move(Point destination)
        {
            // Remember if really moved
            changed = Position == destination;

            World.Grid[Position.X, Position.Y] = null;
            World.Grid[destination.X, destination.Y] = this;
            Position = destination;
            Moved = true;
        }

        // execute one simulation step for this animal 
        // animal behaviour is implemented in the specific classes (fish, shark)
        public abstract void ExecuteStep();

        // commit the current simulation step for this animal
        // resets the moved flag to prepare for the next simulation step
        public virtual void Commit()
        {
            Moved = false;
            changed = false;
        }

        // animals can spawn to create new children
        // specific spawning behaviour of animal is implemented in the specific classes
        protected abstract void Spawn();

        public bool HasPositionChanged()
        {
            return Moved && changed;
        }
    }
}
