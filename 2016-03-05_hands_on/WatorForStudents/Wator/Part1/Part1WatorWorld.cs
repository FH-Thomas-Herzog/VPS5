using System;
using System.Collections.Generic;
using System.Drawing;

namespace VSS.Wator.Part1
{
    public enum Direction
    {
        UP, DOWN, LEFT, RIGHT
    }

    // object-oriented implementation of the wator world simulation
    public class Part1WatorWorld : IWatorWorld
    {
        // random number generator
        private Random random;

        // A matrix of ints that determines the order of execution of each cell of the world.
        // this matrix is shuffled in each time step.
        // Cells of the world must be executed in a random order,
        // otherwise the animal in the first cell is always allowed to move first.
        private int[,] randomMatrix;

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
        public Part1WatorWorld(Settings settings)
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
            randomMatrix = GenerateRandomMatrix(Width, Height);
        }

        // execute one time step of the simulation. Each cell of the world must be executed once
        // Animal move around on the grid. To make sure each animal is executed only once we
        // use the moved flag.
        public void ExecuteStep()
        {
            // shuffle the values in randomMatrix to make sure
            // that in each time step the order of execution of cells is different (and random)
            RandomizeMatrix(randomMatrix);

            // go over all cells of the random matrix
            // the variables row and col contain the actual position of the
            // grid cell that should be executed.
            int row, col;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    // determine row and col of the grid cell by manipulating the value
                    // of the current cell in the random matrix
                    col = randomMatrix[i, j] % Width;
                    row = randomMatrix[i, j] / Width;

                    // if there is an animal on this cell that has not been moved in this simulation step
                    // then we execute it
                    if (Grid[col, row] != null && !Grid[col, row].Moved)
                        Grid[col, row].ExecuteStep();
                }
            }

            // !!!!! 
            // Handle only moved animals and ignore not moved animals. (saved references in collection) 
            //!!!!!!
            // commit all animals in the grid to prepare for the next simulation step
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Grid[i, j] != null)
                        Grid[i, j].Commit();
                }
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
        public Point GetNeighbors(Type type, Point position)
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

        // select a random neighbouring cell that contains an animal (or null) of the given type
        public Point SelectNeighbor(Type type, Point position)
        {
            // first determine _all_ neighbours of the given type
            Point[] neighbors = new Point[] { GetNeighbors(type, position) };
            if (neighbors.Length > 1)
            {
                // if more than one cell has been found => return a randomly selected cell
                return neighbors[random.Next(neighbors.Length)];
            }
            else if (neighbors.Length == 1)
            {
                // if only a single cell contains an animal of the given type we can save the call to random
                return neighbors[0];
            }
            else {
                // return a point with negative coordinates to indicate
                // that no neighbouring cell has found
                // return value must be checked by the caller
                return new Point(-1, -1);
            }
        }

        // create a 2D array containing all numbers in the range 0 .. width * height
        // the numbers are shuffled to create a random ordering
        private int[,] GenerateRandomMatrix(int width, int height)
        {
            int[,] matrix = new int[width, height];

            // initialize
            int row = 0;
            int col = 0;
            for (int i = 0; i < matrix.Length; i++)
            {
                matrix[col, row] = i;
                col++;
                if (col >= width) { col = 0; row++; }
            }
            // shuffle matrix
            RandomizeMatrix(matrix);
            return matrix;
        }

        // shuffle the values of the 2D array in a random fashion
        private void RandomizeMatrix(int[,] matrix)
        {
            // perform a Knuth shuffle (http://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle)
            // here we need to shuffle a 2D array instead of a simple array
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            int temp, selectedRow, selectedCol;

            // row and col are updated in the following loop over all cells
            // begin in the top left (0/0) position of the matrix
            int row = 0;
            int col = 0;
            // for all cells of the matrix
            for (int i = 0; i < height * width; i++)
            {
                // store the original value
                temp = matrix[col, row];
                // select a random row for the swap operation
                // beginning from the current row (as per Knuth shuffle)
                selectedRow = random.Next(row, height);
                // 2 cases: 
                // 1) the randomly selected row is the current row => select a random column larger than the current column
                // 2) the randomly selected row is larger than the current row => select a random column in the range 0..width (exclusive)
                if (selectedRow == row) selectedCol = random.Next(col, width);
                else selectedCol = random.Next(width);

                // swap the values at the current cell and the randomly selected cell
                matrix[col, row] = matrix[selectedCol, selectedRow];
                matrix[selectedCol, selectedRow] = temp;

                // always increment current column
                col++;
                // when the current column was the last column in the row 
                // then increment the current row and reset the column to zero
                if (col >= width) { col = 0; row++; }
            }
        }
    }
}
