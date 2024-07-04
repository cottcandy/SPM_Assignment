using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NgeeAnnCity
{
    public enum BuildingType
    {
        Residential,
        Industry,
        Commercial,
        Park,
        Road
    }

    public class Program
    {
        private const int ArcadeGridSize = 20;
        private const int FreePlayInitialGridSize = 5;
        private const int GridSizeIncrement = 10;
        private const int MaxGridSize = 25;
        private const int InitialCoins = 16;
        private static int coins;
        private static Building[,] grid;
        private static List<Building> buildings;
        private static Random random = new Random();
        private static int turnNumber = 1;
        private static string playerName;
        private static int gridSize;


        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to City Builder Game!");
            Console.Write("Enter your name: ");
            playerName = Console.ReadLine().Trim();
            coins = InitialCoins;
            DisplayMainMenu();
        }

        private static void DisplayMainMenu()
        {
            while (true)
            {
                Console.WriteLine($"Welcome, {playerName}!");
                Console.WriteLine("");
                Console.WriteLine("=== Main Menu ===");
                Console.WriteLine("1. Start New Arcade Game");
                Console.WriteLine("2. Start New Free Play Game");
                Console.WriteLine("3. Load Saved Game");
                Console.WriteLine("4. Display High Scores");
                Console.WriteLine("5. Exit Game");

                Console.Write("Choose an option (1-5): ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        StartArcadeGame();
                        break;
                    case "2":
                        StartFreePlayGame();
                        break;
                    case "3":
                        LoadSavedGame();
                        break;
                    case "4":
                        // DisplayHighScores();
                        break;
                    case "5":
                        Console.WriteLine("Exiting the game. Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please choose again.");
                        break;
                }
            }
        }

        private static void StartArcadeGame()
        {
            coins = InitialCoins;
            turnNumber = 1;
            gridSize = ArcadeGridSize;
            grid = new Building[gridSize, gridSize];
            buildings = new List<Building>();

            Console.WriteLine($"Starting Arcade Game, {playerName}!");

            StartArcade();
        }

        private static void StartFreePlayGame()
        {
            coins = InitialCoins;
            turnNumber = 1;
            gridSize = FreePlayInitialGridSize;
            grid = new Building[gridSize, gridSize];
            buildings = new List<Building>();

            Console.WriteLine($"Starting Free Play Game, {playerName}!");

            StartFreePlay();
            StartFreePlay();
        }

        private static void StartArcade()
        {
            bool isFirstTurn = true;
            bool isSecondTurn = false;

            while (coins > 0)
            {
                BuildingType[] options = GetUniqueRandomBuildings();
                bool validInput = false;

                while (!validInput)
                {
                    DisplayArcadeGrid();
                    Console.WriteLine($"Turn: {turnNumber} | Coins: {coins} | Score: {CalculateScore()}");

                    if (isFirstTurn)
                    {
                        Console.WriteLine(" ");
                        Console.WriteLine("- The objective of this game is to build a city that scores as many points as possible.");
                        Console.WriteLine("- For the first building, you can build anywhere in the city.");
                        Console.WriteLine("There are five types of buidings:");
                        Console.WriteLine("- Residential (R): Each residential building generates 1 coin per turn. Each cluster of residential buildings (must be immediately next to each other) requires 1 coin per turn to upkeep.");
                        Console.WriteLine("- Industry (I): Each industry generates 2 coins per turn and costs 1 coin per turn to upkeep.");
                        Console.WriteLine("- Commercial (C): Each commercial generates 3 coins per turn and costs 2 coins per turn to upkeep.");
                        Console.WriteLine("- Park (O): Each park costs 1 coin to upkeep.");
                        Console.WriteLine("- Road (*): Each unconnected road segment costs 1 coin to upkeep.");
                        Console.WriteLine(" ");
                        Console.WriteLine($"Options: 1. {options[0]} 2. {options[1]}");
                        Console.WriteLine("3. Select a cell with a building to demolish it (1 coin cost).");
                        Console.WriteLine("4. Save Game");
                        Console.WriteLine("5. Exit to Main Menu");
                        Console.Write("Choose an option (1, 2, 3, 4, 5): ");
                    }
                    else if (isSecondTurn)
                    {
                        Console.WriteLine(" ");
                        Console.WriteLine("- For subsequent constructions, you can only build on squares connected to existing buildings");
                        Console.WriteLine(" ");
                        Console.WriteLine($"Options: 1. {options[0]} 2. {options[1]}");
                        Console.WriteLine("3. Select a cell with a building to demolish it (1 coin cost).");
                        Console.WriteLine("4. Save Game");
                        Console.WriteLine("5. Exit to Main Menu");
                        Console.Write("Choose an option (1, 2, 3, 4, 5): ");
                    }
                    else
                    {
                        Console.WriteLine(" ");
                        Console.WriteLine($"Options: 1. {options[0]} 2. {options[1]}");
                        Console.WriteLine("3. Select a cell with a building to demolish it (1 coin cost).");
                        Console.WriteLine("4. Save Game");
                        Console.WriteLine("5. Exit to Main Menu");
                        Console.Write("Choose an option (1, 2, 3, 4, 5): ");
                    }

                    if (int.TryParse(Console.ReadLine(), out int choice) && (choice >= 1 && choice <= 5))
                    {
                        if (choice == 3 && isFirstTurn)
                        {
                            Console.WriteLine("You cannot demolish a building on the first turn.");
                            continue;
                        }

                        switch (choice)
                        {
                            case 1:
                            case 2:
                                BuildingType selectedBuilding = options[choice - 1];
                                Console.WriteLine("");
                                Console.WriteLine("");
                                Console.WriteLine($"You selected: {selectedBuilding}");

                                while (true)
                                {
                                    (int x, int y) = GetBuildLocation(isFirstTurn);
                                    if (IsValidLocation(x - 1, y - 1, isFirstTurn))
                                    {
                                        PlaceBuilding(selectedBuilding, x - 1, y - 1);
                                        coins--;
                                        UpdateCoins(selectedBuilding, x - 1, y - 1);
                                        validInput = true;
                                        if (isFirstTurn)
                                        {
                                            isFirstTurn = false;
                                            isSecondTurn = true;
                                        }
                                        else
                                        {
                                            isSecondTurn = false;
                                        }
                                        turnNumber++;
                                        break;
                                    }

                                    else
                                    {
                                        Console.WriteLine("The location must be adjacent to an existing building.");
                                        Console.WriteLine("Press 0 to go back to the main menu or press 00 to continue playing.");
                                        string input = Console.ReadLine().Trim();

                                        if (input == "0")
                                            break;
                                        else if (input == "00")
                                            continue;
                                    }
                                }
                                break;

                            case 3:
                                if (!isFirstTurn)
                                {
                                    Console.WriteLine("Select a cell with a building to demolish it (1 coin cost), or key in X: 0 and Y: 0 to cancel.");
                                    (int x, int y) = GetBuildLocation(isFirstTurn);

                                    if (x == 0 && y == 0)
                                    {
                                        Console.WriteLine("Demolishing buildings cancelled.");
                                        continue;
                                    }

                                    if (grid[x - 1, y - 1] != null)
                                    {
                                        RemoveBuilding(x - 1, y - 1);
                                        coins++;
                                        validInput = true;
                                        turnNumber++;
                                    }
                                    else
                                    {
                                        Console.WriteLine("No building found at the selected location. Please try again.");
                                    }
                                }
                                break;
                            case 4:
                                SaveGame();
                                break;
                            case 5:
                                return; // Exit to Main Menu
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid option. Please choose 1, 2, 3, 4, or 5.");
                    }
                }
            }

            Console.WriteLine("Game Over! Final Score: " + CalculateScore());
        }

        private static void DisplayArcadeGrid()
        {
            // Print the column labels
            Console.Write("     ");
            for (int i = 1; i <= gridSize; i++)
            {
                if (i < 10)
                {
                    Console.Write($"X{i}  "); // Adjust spacing for single-digit numbers
                }
                else
                {
                    Console.Write($"X{i} "); // Maintain spacing for two-digit numbers
                }
            }
            Console.WriteLine();

            // Print the grid with row labels and borders
            for (int i = 0; i < gridSize; i++)
            {
                // Print the top border of each cell row
                if (i == 0)
                {
                    Console.Write("    ");
                    for (int j = 0; j < gridSize; j++)
                    {
                        Console.Write("+---");
                    }
                    Console.WriteLine("+");
                }

                // Print the row label and cell contents with vertical borders
                Console.Write($"Y{i + 1} ");
                if (i < 9) Console.Write(" "); // align single-digit labels
                for (int j = 0; j < gridSize; j++)
                {
                    Console.Write("|");
                    if (grid[i, j] != null)
                    {
                        switch (grid[i, j].Type)
                        {
                            case BuildingType.Residential:
                                Console.Write(" R ");
                                break;
                            case BuildingType.Industry:
                                Console.Write(" I ");
                                break;
                            case BuildingType.Commercial:
                                Console.Write(" C ");
                                break;
                            case BuildingType.Park:
                                Console.Write(" O ");
                                break;
                            case BuildingType.Road:
                                Console.Write(" * ");
                                break;
                        }
                    }
                    else
                    {
                        Console.Write("   ");
                    }
                }
                Console.WriteLine("|");

                // Print the bottom border of each cell row
                Console.Write("    ");
                for (int j = 0; j < gridSize; j++)
                {
                    Console.Write("+---");
                }
                Console.WriteLine("+");
            }
        }

        private static void StartFreePlay()
        {
            bool isFirstTurn = true;

            while (coins > 0)
            {
                BuildingType[] options = GetFixedBuildingOptions();
                bool validInput = false;

                while (!validInput)
                {
                    DisplayFreePlayGrid();
                    Console.WriteLine($"Turn: {turnNumber} | Coins: {coins} | Score: {CalculateScore()}");

                    if (isFirstTurn)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("- In Free Play mode, you have unlimited coins.");
                        Console.WriteLine("- Start with a 5x5 grid and expand the grid by 5 rows/columns when a building is constructed on the border.");
                        Console.WriteLine("- There are five types of buildings:");
                        Console.WriteLine("- Residential (R): Each residential building generates 1 coin per turn. Each cluster of residential buildings (must be immediately next to each other) requires 1 coin per turn to upkeep.");
                        Console.WriteLine("- Industry (I): Each industry generates 2 coins per turn and costs 1 coin per turn to upkeep.");
                        Console.WriteLine("- Commercial (C): Each commercial generates 3 coins per turn and costs 2 coins per turn to upkeep.");
                        Console.WriteLine("- Park (O): Each park costs 1 coin to upkeep.");
                        Console.WriteLine("- Road (*): Each unconnected road segment costs 1 coin to upkeep.");
                        Console.WriteLine();
                    }

                    Console.WriteLine("");
                    Console.WriteLine($"Options: 1. {options[0]} 2. {options[1]} 3. {options[2]} 4. {options[3]} 5. {options[4]}");
                    Console.WriteLine("6. Select a cell with a building to demolish it.");
                    Console.WriteLine("7. Save Game");
                    Console.WriteLine("8. Exit to Main Menu");
                    Console.Write("Choose an option (1, 2, 3, 4, 5, 6, 7, 8): ");

                    string userInput = Console.ReadLine();

                    switch (userInput)
                    {
                        case "1":
                        case "2":
                        case "3":
                        case "4":
                        case "5":
                            int choice = int.Parse(userInput);
                            BuildingType selectedBuilding = options[choice - 1];
                            Console.WriteLine($"You selected: {selectedBuilding}");

                            (int x, int y) = GetBuildLocation(isFirstTurn);
                            if (IsValidLocationFreePlay(x - 1, y - 1))
                            {
                                PlaceBuilding(selectedBuilding, x - 1, y - 1);
                                UpdateCoins(selectedBuilding, x - 1, y - 1);
                                validInput = true;
                                isFirstTurn = false;
                                turnNumber++;
                                if (x == 1 || y == 1 || x == gridSize || y == gridSize)
                                {
                                    ExpandGrid();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid location. Please try again.");
                            }
                            break;
                        case "6":
                            if (!isFirstTurn)
                            {
                                Console.WriteLine("Select a cell with a building to demolish it, or key in X: 0 and Y: 0 to cancel.");
                                (int x2, int y2) = GetBuildLocation(isFirstTurn);

                                if (x2 == 0 && y2 == 0)
                                {
                                    Console.WriteLine("Demolishing buildings cancelled.");
                                    continue;
                                }
                                if (x2 == 0 && y2 == 0)
                                {
                                    Console.WriteLine("Demolishing buildings cancelled.");
                                    continue;
                                }

                                if (grid[x2 - 1, y2 - 1] != null)
                                {
                                    RemoveBuilding(x2 - 1, y2 - 1);
                                    validInput = true;
                                    turnNumber++;
                                }
                                else
                                {
                                    Console.WriteLine("No building found at the selected location. Please try again.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("You cannot demolish a building on the first turn.");
                            }
                            break;
                        case "7":
                            SaveGame();
                            break;
                        case "8":
                            return; // Exit to Main Menu
                        default:
                            Console.WriteLine("Invalid option. Please choose a valid option (1, 2, 3, 4, 5, 6, 7, 8).");
                            break;
                    }
                }
            }

            Console.WriteLine("Game Over! Final Score: " + CalculateScore());
        }


        private static void DisplayFreePlayGrid()
        {
            // Print the column labels
            Console.Write("     ");
            for (int i = 1; i <= gridSize; i++)
            {
                if (i < 10)
                {
                    Console.Write($"X{i}  "); // Adjust spacing for single-digit numbers
                }
                else
                {
                    Console.Write($"X{i} "); // Maintain spacing for two-digit numbers
                }
            }
            Console.WriteLine();

            // Print the grid with row labels and borders
            for (int i = 0; i < gridSize; i++)
            {
                // Print the top border of each cell row
                if (i == 0)
                {
                    Console.Write("    ");
                    for (int j = 0; j < gridSize; j++)
                    {
                        Console.Write("+---");
                    }
                    Console.WriteLine("+");
                }

                // Print the row label and cell contents with vertical borders
                Console.Write($"Y{i + 1} ");
                if (i < 9) Console.Write(" "); // align single-digit labels
                for (int j = 0; j < gridSize; j++)
                {
                    Console.Write("|");
                    if (grid[i, j] != null)
                    {
                        switch (grid[i, j].Type)
                        {
                            case BuildingType.Residential:
                                Console.Write(" R ");
                                break;
                            case BuildingType.Industry:
                                Console.Write(" I ");
                                break;
                            case BuildingType.Commercial:
                                Console.Write(" C ");
                                break;
                            case BuildingType.Park:
                                Console.Write(" O ");
                                break;
                            case BuildingType.Road:
                                Console.Write(" * ");
                                break;
                            default:
                                Console.Write("   "); // For any unhandled types, print empty space
                                break;
                        }
                    }
                    else
                    {
                        Console.Write("   ");
                    }
                }
                Console.WriteLine("|");

                // Print the bottom border of each cell row
                Console.Write("    ");
                for (int j = 0; j < gridSize; j++)
                {
                    Console.Write("+---");
                }
                Console.WriteLine("+");
            }
            Console.WriteLine();
        }

        private static void ExpandGrid()
        {
            int newSize = Math.Min(gridSize + GridSizeIncrement, MaxGridSize);
            Building[,] newGrid = null;

            if (newSize > gridSize)
            {
                newGrid = new Building[newSize, newSize];
                for (int i = 0; i < grid.GetLength(0); i++)
                {
                    for (int j = 0; j < grid.GetLength(1); j++)
                    {
                        newGrid[i, j] = grid[i, j];
                    }
                }
                gridSize = newSize;
                grid = newGrid;
                Console.WriteLine($"The city has expanded to {newSize}x{newSize}.");
            }
            else
            {
                Console.WriteLine("The city has reached its maximum size.");
            }
        }

        private static void SaveGame()
        {
            string fileName = $"{playerName}_save.txt";
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine(playerName);
                writer.WriteLine(coins);
                writer.WriteLine(turnNumber);
                writer.WriteLine(CalculateScore());

                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        if (grid[i, j] != null)
                        {
                            writer.WriteLine($"{i},{j},{grid[i, j].Type}");
                        }
                    }
                }
            }

            Console.WriteLine("Game saved successfully.");
        }

        private static void LoadSavedGame()
        {
            Console.Write("Enter the name of the saved game file: ");
            string fileName = Console.ReadLine().Trim();

            if (File.Exists(fileName))
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    playerName = reader.ReadLine();
                    coins = int.Parse(reader.ReadLine());
                    turnNumber = int.Parse(reader.ReadLine());

                    grid = new Building[gridSize, gridSize];
                    buildings = new List<Building>();

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(',');
                        int x = int.Parse(parts[0]);
                        int y = int.Parse(parts[1]);
                        BuildingType type = (BuildingType)Enum.Parse(typeof(BuildingType), parts[2]);

                        Building building = new Building(type, x, y);
                        grid[x, y] = building;
                        buildings.Add(building);
                    }
                }

                Console.WriteLine("Game loaded successfully.");
                StartArcade();
            }
            else
            {
                Console.WriteLine("Saved game file not found.");
            }
        }

        private static (int x, int y) GetBuildLocation(bool isFirstTurn)
        {
           
                Console.Write("Enter X (1-20): ");
                int x = int.Parse(Console.ReadLine());

                Console.Write("Enter Y (1-20): ");
                int y = int.Parse(Console.ReadLine());
                Console.WriteLine("");

                return (x, y);
        }
            

        private static bool IsValidLocation(int x, int y, bool isFirstTurn)
        {
            if (isFirstTurn)
            {
                return grid[x, y] == null;
            }
            else
            {
                return grid[x, y] == null && HasAdjacentBuilding(x, y);
            }
        }

        private static bool HasAdjacentBuilding(int x, int y)
        {
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            Console.WriteLine($"Checking adjacency for ({x}, {y})");

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize)
                {
                    if (grid[nx, ny] != null)
                    {
                        Console.WriteLine($"Adjacent building found at ({nx}, {ny})");
                        return true;
                    }
                }
            }
            Console.WriteLine("No adjacent building found.");
            return false;
        }

        private static bool IsValidLocationFreePlay(int x, int y)
        {
            // Check if coordinates are within grid bounds
            if (x < 0 || x >= gridSize || y < 0 || y >= gridSize)
            {
                Console.WriteLine("Selected location is outside the grid bounds.");
                return false;
            }

            // Check if the cell is already occupied
            if (grid[x, y] != null)
            {
                Console.WriteLine("There is already a building at the selected location.");
                return false;
            }

            // All conditions met, location is valid for Free Play mode
            return true;
        }


        private static BuildingType[] GetUniqueRandomBuildings()
        {
            List<BuildingType> buildingTypes = Enum.GetValues(typeof(BuildingType)).Cast<BuildingType>().ToList();
            BuildingType[] options = new BuildingType[2];

            for (int i = 0; i < 2; i++)
            {
                int index = random.Next(buildingTypes.Count);
                options[i] = buildingTypes[index];
                buildingTypes.RemoveAt(index);
            }

            return options;
        }

        private static BuildingType[] GetFixedBuildingOptions()
        {
            BuildingType[] fixedBuildingOptions = new BuildingType[]
            {
                BuildingType.Residential,
                BuildingType.Commercial,
                BuildingType.Industry,
                BuildingType.Park,
                BuildingType.Road
            };

            return fixedBuildingOptions;
        }

        private static void PlaceBuilding(BuildingType type, int x, int y)
        {
            Building building = new Building(type, y, x);
            grid[y, x] = building;
            buildings.Add(building);
        }

        private static void RemoveBuilding(int x, int y)
        {
            Building building = grid[x, y];
            buildings.Remove(building);
            grid[x, y] = null;
        }

        private static void UpdateCoins(BuildingType type, int x, int y)
        {
            switch (type)
            {
                case BuildingType.Residential:
                    coins += 1;
                    break;
                case BuildingType.Industry:
                    coins += 2;
                    break;
                case BuildingType.Commercial:
                    coins += 3;
                    break;
                case BuildingType.Park:
                    coins -= 1;
                    break;
                case BuildingType.Road:
                    coins -= 1;
                    break;
            }
        }

        private static int CalculateScore()
        {
            // Simple scoring mechanism
            int score = buildings.Count * 10;
            return score;
        }
    }

    public class Building
    {
        public BuildingType Type { get; }
        public int X { get; }
        public int Y { get; }

        public Building(BuildingType type, int x, int y)
        {
            Type = type;
            X = x;
            Y = y;
        }
    }
}
