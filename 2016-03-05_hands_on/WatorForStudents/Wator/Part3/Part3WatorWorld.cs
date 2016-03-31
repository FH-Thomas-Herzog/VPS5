using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace VSS.Wator.Part3
{
    public enum Direction
    {
        UP, DOWN, LEFT, RIGHT
    }

    // object-oriented implementation of the wator world simulation
    public class Part3WatorWorld : IWatorWorld
    {
        // random number generator
        private Random random;

        // A matrix of ints that determines the order of execution of each cell of the world.
        // this matrix is shuffled in each time step.
        // Cells of the world must be executed in a random order,
        // otherwise the animal in the first cell is always allowed to move first.
        private int[,] randomMatrix;
        Point[] randomPoints;

        // for visualization
        private byte[] rgbValues;

        #region Properties
        // width (number of cells) of the world
        public int Width { get; private set; }
        // height (number of cells) of the world
        public int Height { get; private set; }
        // the cells of the world (2D-array of animal (fish or shark), empty cells have the value null)
        public Animal[,] Grid { get; private set; }

        // simulation parameters
        public int InitialFishPopulation { get; private set; }
        public int InitialFishEnergy { get; private set; }
        public int FishBreedTime { get; private set; }

        public int InitialSharkPopulation { get; private set; }
        public int InitialSharkEnergy { get; private set; }
        public int SharkBreedEnergy { get; private set; }
        #endregion

        private IList<Direction> directionList = new List<Direction>() { Direction.UP, Direction.DOWN, Direction.LEFT, Direction.RIGHT };
        private readonly Point INVALID_POINT = new Point(-1, -1);

        // create and init a new wator world with the given settings
        public Part3WatorWorld(Settings settings)
        {
            // copy settings 
            Width = settings.Width;
            Height = settings.Height;
            InitialFishPopulation = settings.InitialFishPopulation;
            InitialFishEnergy = settings.InitialFishEnergy;
            FishBreedTime = settings.FishBreedTime;
            InitialSharkPopulation = settings.InitialSharkPopulation;
            InitialSharkEnergy = settings.InitialSharkEnergy;
            SharkBreedEnergy = settings.SharkBreedEnergy;

            rgbValues = new byte[Width * Height * 4];

            random = new Random();
            Grid = new Animal[Width, Height];

            // initialize the population by placing the required number of shark and fish
            // randomly on the grid
            // randomMatrix contains all values from 0 .. Width*Height in a random ordering
            // so we can simply place a fish onto a cell if the value in the same cell of the
            // randomMatrix is smaller then the number of fish 
            // subsequently we can place a shark if the number in randomMatrix is smaller than
            // the number of fish and shark
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    int value = random.Next(Width * Height);
                    if (value < InitialFishPopulation)
                    {
                        Grid[i, j] = new Fish(this, new Point(i, j), random.Next(0, FishBreedTime));
                    }
                    else if (value < InitialFishPopulation + InitialSharkPopulation)
                    {
                        Grid[i, j] = new Shark(this, new Point(i, j), random.Next(0, SharkBreedEnergy));
                    }
                    else {
                        Grid[i, j] = null;
                    }
                }
            }

            // populate the random matrix that determines the order of execution for the cells
            //randomMatrix = GenerateRandomMatrix(Width, Height);

            randomPoints = new Point[Height * Width];
            foreach (int x in Enumerable.Range(0, Height))
            {
                foreach (int y in Enumerable.Range(0, Width))
                {
                    randomPoints[(Height * x) + y] = new Point(x, y);
                }
            }
        }

        // execute one time step of the simulation. Each cell of the world must be executed once
        // Animal move around on the grid. To make sure each animal is executed only once we
        // use the moved flag.
        public void ExecuteStep()
        {
            // Shuffel point positions, so that each time different position ordering is present.
            shuffelsPoints(randomPoints);

            // List which holds the moved animals (assume 30% items moved on the grid)
            IList<Animal> movedAnimals = new List<Animal>((int)(Grid.Length * 0.3));

            // visit each shuffeled position
            foreach (var point in randomPoints)
            {
                Animal animal = Grid[point.X, point.Y];
                // If animal present and not already moved
                if ((animal != null) && (!animal.Moved))
                {
                    animal.ExecuteStep();
                    // remember moved animal
                    if(animal.Moved)
                    {
                        movedAnimals.Add(animal);
                    }
                }
            }

            // visit all animals to commit them
            foreach (var animal in movedAnimals)
            {
                animal.Commit();
            }
        }

        // generates a bitmap for the current state of the wator world
        public Bitmap GenerateImage()
        {
            int counter = 0;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    Color col;
                    if (Grid[x, y] == null) col = Color.DarkBlue;
                    else col = Grid[x, y].Color;

                    rgbValues[counter++] = col.B; //  // b
                    rgbValues[counter++] = col.G; // // g
                    rgbValues[counter++] = col.R; //  // R
                    rgbValues[counter++] = col.A; //  // a
                }
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            var bitmap = new Bitmap(Width, Height);
            System.Drawing.Imaging.BitmapData bmpData = null;
            try
            {
                bmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Copy the RGB values back to the bitmap
                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);
            }
            finally
            {
                // Unlock the bits.
                if (bmpData != null) bitmap.UnlockBits(bmpData);
            }
            return bitmap;
        }

        // find all neighbouring cells of the given position that contain an animal of the given type
        public Point GetNeighbor(Type type, Point position)
        {
            // Could be randomly accessed, but this cost too much.
            for (int i = 0; i < 4; i++)
            {
                int newX, newY;
                newX = position.X;
                newY = position.Y;
                switch (directionList[i])
                {
                    case Direction.UP:
                        newY++;
                        break;
                    case Direction.DOWN:
                        newY++;
                        break;
                    case Direction.RIGHT:
                        newX++;
                        break;
                    case Direction.LEFT:
                        newX++;
                        break;
                }

                newX = (newX > (Width - 1)) ? 0 : newX;
                newY = (newY > (Height - 1)) ? 0 : newY;

                Animal item = Grid[newX, newY];
                // Empty cell searched
                if (type == null)
                {
                    if (item == null)
                    {
                        return new Point(newX, newY);
                    }
                }
                // Search for item of type
                else if ((type.IsInstanceOfType(item)))
                {
                    return new Point(newX, newY);
                }
            }

            return INVALID_POINT;
        }

        /// <summary>
        /// Shuffels teh point array to get random ordering of the to visit points.
        /// Oportunity is no duplicate positions will ever occur.
        /// </summary>
        /// <param name="pointArray">the array to be shuffeled</param>
        private void shuffelsPoints(Point[] pointArray)
        {
            // 'Fisher–Yates shuffle' algorithm
            for (int i = ((Height * Width) -1); i > 0; i--)
            {
                int j = random.Next(0, (i +1));
                Point tmp = pointArray[j];
                pointArray[j] = pointArray[i];
                pointArray[i] = tmp;
            }
        }
    }
}
