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
        private static int score;

        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Ngee Ann City!");
            Console.Write("Enter your name: ");
            playerName = Console.ReadLine().Trim();
            Console.WriteLine("");
            Console.WriteLine($"Welcome, {playerName}!");
            coins = InitialCoins;
            DisplayMainMenu();
        }

        private static void DisplayMainMenu()
        {
            string highScoreFileName = "high_scores.txt";
            while (true)
            {
                Console.WriteLine("");
                Console.WriteLine("=== Main Menu ===");
                Console.WriteLine("1. Start New Arcade Game");
                Console.WriteLine("2. Start New Free Play Game");
                Console.WriteLine("3. Load Saved Game");
                Console.WriteLine("4. Display High Scores");
                Console.WriteLine("5. Exit Game");

                Console.Write("Choose an option (1-5): ");
                string input = Console.ReadLine();
                Console.WriteLine("");

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
                        DisplayAllHighScores(highScoreFileName);
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

        private static void InitializeGame(int size)
        {
            gridSize = size;
            grid = new Building[gridSize, gridSize];
            buildings = new List<Building>();
            coins = InitialCoins;
            turnNumber = 1;
        }

        private static void StartArcadeGame()
        {
            InitializeGame(ArcadeGridSize);
            Console.WriteLine($"Starting Arcade Game, {playerName}!");
            StartArcade();
        }

        private static void StartFreePlayGame()
        {
            InitializeGame(FreePlayInitialGridSize);
            Console.WriteLine($"Starting Free Play Game, {playerName}!");
            StartFreePlay();
        }

        private static void StartArcade()
        {
            bool isFirstTurn = true;
            bool isSecondTurn = false;

            while (coins > 0)
            {
                if (IsGridFilled())
                {
                    EndArcadeMode("The grid is completely filled!");
                    return;
                }

                BuildingType[] options = GetUniqueRandomBuildings();
                bool validInput = false;

                while (!validInput)
                {
                    DisplayArcadeGrid();
                    Console.WriteLine($"Turn: {turnNumber} | Coins: {coins} | Score: {CalculateScore()}");

                    if (isFirstTurn)
                    {
                        Console.WriteLine(" ");
                        Console.WriteLine("~Instructions (1/2)~");
                        Console.WriteLine("- You are the mayor of this city.");
                        Console.WriteLine("- The objective of this game is to build a city that scores as many points as possible.");
                        Console.WriteLine("- For the first building, you can build anywhere in the city.");
                        Console.WriteLine("There are five types of buildings:");
                        Console.WriteLine("- Residential (R): If it is next to an industry (I), then it scores 1 point only. Otherwise, it scores 1 point for each adjacent residential (R) or commercial (C), and 2 points for each adjacent park (O). Each residential building generates 1 coin per turn. Each cluster of residential buildings (must be immediately next to each other) requires 1 coin per turn to upkeep.");
                        Console.WriteLine("- Industry (I): Scores 1 point per industry in the city. Each industry generates 1 coin per residential building adjacent to it. Each industry generates 2 coins per turn and costs 1 coin per turn to upkeep.");
                        Console.WriteLine("- Commercial (C): Scores 1 point per commercial adjacent to it. Each commercial generates 1 coin per residential adjacent to it. Each commercial generates 3 coins per turn and costs 2 coins per turn to upkeep.");
                        Console.WriteLine("- Park (O): Scores 1 point per park adjacent to it. Each park costs 1 coin to upkeep.");
                        Console.WriteLine("- Road (*): Scores 1 point per connected road (*) in the same row. Each unconnected road segment costs 1 coin to upkeep.");
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
                        Console.WriteLine("~Instructions (2/2)~");
                        Console.WriteLine("- For subsequent constructions, you can only build on squares connected to existing buildings");
                        Console.WriteLine("- The game ends either when the city is filled, or when you run out of coins.");
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
                            Console.Write("Press any key to continue");
                            Console.Read();
                            continue;
                        }

                        switch (choice)
                        {
                            case 1:
                            case 2:
                                BuildingType selectedBuilding = options[choice - 1];
                                Console.WriteLine("");
                                Console.WriteLine($"You selected: {selectedBuilding}");

                                while (true)
                                {
                                    (int x, int y) = GetBuildLocation(isFirstTurn);
                                    if (IsValidLocation(x - 1, y - 1, isFirstTurn))
                                    {
                                        PlaceBuilding(selectedBuilding, x - 1, y - 1);
                                        coins--; //Each construction cost 1 coin.
                                        if (!isFirstTurn)
                                        {
                                            UpdateCoins();
                                        }
                                        validInput = true;
                                        if (isFirstTurn)
                                        {
                                            isFirstTurn = false;
                                        }
                                        turnNumber++;
                                        break;
                                    }

                                    else
                                    {
                                        Console.WriteLine("Invalid choice. Press 0 to go back to the main menu or press any key to continue playing.");
                                        string input = Console.ReadLine().Trim();

                                        if (input == "0")
                                        {
                                            validInput = false;
                                            break;
                                        }
                                    }
                                }
                                break;

                            case 3:
                                if (!isFirstTurn)
                                {
                                    Console.WriteLine("");
                                    Console.WriteLine("Select a cell with a building to demolish it (1 coin cost), or key in X: 0 and Y: 0 to cancel.");

                                    int x, y;
                                    bool validCoordinates = false;

                                    while (!validCoordinates)
                                    {
                                        Console.Write("Enter X (1-20): ");
                                        if (!int.TryParse(Console.ReadLine(), out x) || x < 0 || x > 20)
                                        {
                                            Console.WriteLine("Invalid input. Please enter an integer between 1 and 20.");
                                            continue;
                                        }

                                        Console.Write("Enter Y (1-20): ");
                                        if (!int.TryParse(Console.ReadLine(), out y) || y < 0 || y > 20)
                                        {
                                            Console.WriteLine("Invalid input. Please enter an integer between 1 and 20.");
                                            continue;
                                        }

                                        if (x == 0 && y == 0)
                                        {
                                            Console.WriteLine("Demolishing buildings cancelled.");
                                            validCoordinates = true; // Exit the loop and cancel demolition
                                        }
                                        else if (grid[y - 1, x - 1] != null)
                                        {
                                            RemoveBuilding(y - 1, x - 1);
                                            coins--; // Deduct 1 coin for demolition
                                            validInput = true;
                                            turnNumber++;
                                            validCoordinates = true; // Exit the loop after successful demolition
                                        }
                                        else
                                        {
                                            Console.WriteLine("No building found at the selected location. Please try again.");
                                        }
                                    }
                                }
                                break;
                            case 4:
                                ArcadeSaveGame();
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

            EndArcadeMode("You have run out of coins!");
        }

        private static bool IsGridFilled()
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void EndArcadeMode(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Game Over!");
            Console.WriteLine($"Final Score: {CalculateScore()}");
            Console.WriteLine($"Total Turns: {turnNumber}");
            Console.WriteLine("Press any key to return to the Main Menu...");
            Console.ReadLine();
            DisplayMainMenu();
        }

        private static void DisplayArcadeGrid()
        {
            Console.WriteLine("");
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

            while (turnNumber <= 20)
            {
                BuildingType[] options = GetFixedBuildingOptions();
                bool validInput = false;

                while (!validInput)
                {
                    DisplayFreePlayGrid();
                    Console.WriteLine($"Turn: {turnNumber} | Coins: {"UNLIMITED"} | Score: {CalculateScore()}");

                    if (isFirstTurn)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("~Instructions~ ");
                        Console.WriteLine("- The objective of this game is to build a city that scores as many points as possible.");
                        Console.WriteLine("- You can construct any of the buildings in the city, on any cell in the city.");
                        Console.WriteLine("There are five types of buidings:");
                        Console.WriteLine("- Residential (R): Each residential building generates 1 coin per turn. Each cluster of residential buildings (must be immediately next to each other) requires 1 coin per turn to upkeep.");
                        Console.WriteLine("- Industry (I): Each industry generates 2 coins per turn and cost 1 coin per turn to upkeep.");
                        Console.WriteLine("- Commercial (C): Each commercial generates 3 coins per turn and cost 2 coins per turn to upkeep.");
                        Console.WriteLine("- Park (O): Each park costs 1 coin to upkeep.");
                        Console.WriteLine("- Road (*): Each unconnected road segment costs 1 coin to upkeep.");
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
                            Console.WriteLine("");
                            Console.WriteLine($"You selected: {selectedBuilding}");

                            (int x, int y) = GetBuildLocation(isFirstTurn);
                            if (IsValidLocationFreePlay(x - 1, y - 1))
                            {
                                PlaceBuilding(selectedBuilding, x - 1, y - 1);
                                if (!isFirstTurn)
                                {
                                    UpdateCoins();
                                }
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
                                Console.WriteLine("");
                                Console.WriteLine("Select a cell with a building to demolish it, or key in X: 0 and Y: 0 to cancel.");
                                (int x2, int y2) = GetBuildLocation(isFirstTurn);

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
                                Console.Write("Press any key to continue");
                                Console.Read();
                                continue;
                            }
                            break;
                        case "7":
                            FreePlaySaveGame();
                            break;
                        case "8":
                            return; // Exit to Main Menu
                        default:
                            Console.WriteLine("Invalid option. Please choose a valid option (1, 2, 3, 4, 5, 6, 7, 8).");
                            break;
                    }
                }
            }
            Console.WriteLine("Free Play Game has ended after 20 turns!");
            InitializeGame(FreePlayInitialGridSize);
            DisplayMainMenu();
        }


        private static void DisplayFreePlayGrid()
        {
            Console.WriteLine("");
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
            //Console.WriteLine();
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
                Console.WriteLine("");
                Console.WriteLine($"The city has expanded to {newSize}x{newSize}.");
            }
            else
            {
                Console.WriteLine("The city has reached its maximum size.");
            }
        }

        private static void ArcadeSaveGame()
        {
            string fileName = $"{playerName}_arcade_save.txt";
            string highScoreFileName = "high_scores.txt";
            int gameNumber = 1;

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine($"player name:{playerName}");
                writer.WriteLine($"coins:{coins}");
                writer.WriteLine($"turn:{turnNumber}");
                writer.WriteLine($"score:{CalculateScore()}");

                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        if (grid[i, j] != null)
                        {
                            writer.WriteLine($"{grid[i, j].Type},{j + 1},{i + 1}");
                        }
                    }
                }
            }
            UpdateHighScores(playerName, CalculateScore(), highScoreFileName);
            Console.WriteLine("");
            Console.WriteLine("Game saved successfully.");
        }

        private static void FreePlaySaveGame()
        {
            string fileName = $"{playerName}_freeplay_save.txt";

            int gameNumber = 1;

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                if (gameNumber > 1)
                {
                    writer.WriteLine();
                }
                writer.WriteLine($"player name:{playerName}");
                writer.WriteLine($"gridSize:{gridSize}");
                writer.WriteLine($"coins:{coins}");
                writer.WriteLine($"turn:{turnNumber}");
                writer.WriteLine($"score:{CalculateScore()}");

                // Save the grid state
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        if (grid[i, j] != null)
                        {
                            writer.WriteLine($"{grid[i, j].Type},{j + 1},{i + 1}");
                        }
                    }
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Game saved successfully.");
        }


        private static void ArcadeLoadSavedGame()
        {
            string fileName = $"{playerName}_arcade_save.txt";

            if (File.Exists(fileName))
            {
                InitializeGame(ArcadeGridSize);
                using (StreamReader reader = new StreamReader(fileName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("player name:"))
                        {
                            playerName = line.Split(':')[1]; // Set playerName from saved data
                        }
                        else if (line.StartsWith("turn:"))
                        {
                            turnNumber = int.Parse(line.Split(':')[1]); // Update turn number from saved data
                        }
                        else if (line.StartsWith("coins:"))
                        {
                            coins = int.Parse(line.Split(':')[1]); // Update coins from saved data
                        }
                        else if (line.StartsWith("score:"))
                        {
                            score = int.Parse(line.Split(':')[1]); // Update score from saved data
                        }
                        else if (line.Contains(','))
                        {
                            string[] parts = line.Split(',');
                            if (parts.Length >= 3)
                            {
                                BuildingType type = Enum.Parse<BuildingType>(parts[0], true);
                                int x = int.Parse(parts[1]) - 1;
                                int y = int.Parse(parts[2]) - 1;
                                PlaceBuilding(type, y, x); // Place buildings from saved data
                            }
                        }
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

        private static void FreePlayLoadSavedGame()
        {
            string fileName = $"{playerName}_freeplay_save.txt";

            if (File.Exists(fileName))
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    string line;
                    int loadedGridSize = FreePlayInitialGridSize; // default initial grid size

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("gridSize:"))
                        {
                            int.TryParse(line.Split(':')[1].Trim(), out loadedGridSize);
                            break;
                        }
                    }

                    InitializeGame(loadedGridSize);

                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    reader.DiscardBufferedData();

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("player name:"))
                        {
                            playerName = line.Split(':')[1].Trim();
                        }
                        else if (line.StartsWith("turn:"))
                        {
                            int parsedTurnNumber;
                            if (int.TryParse(line.Split(':')[1].Trim(), out parsedTurnNumber))
                            {
                                turnNumber = parsedTurnNumber; // Update turn number from saved data
                            }
                            // No warning needed for failed parse
                        }
                        else if (line.StartsWith("coins:"))
                        {
                            int parsedCoins;
                            if (int.TryParse(line.Split(':')[1].Trim(), out parsedCoins))
                            {
                                coins = parsedCoins; // Update coins from saved data
                            }
                            // No warning needed for failed parse
                        }
                        else if (line.StartsWith("score:"))
                        {
                            int parsedScore;
                            if (int.TryParse(line.Split(':')[1].Trim(), out parsedScore))
                            {
                                score = parsedScore; // Update score from saved data
                            }
                            // No warning needed for failed parse
                        }
                        else if (line.Contains(','))
                        {
                            string[] parts = line.Split(',');
                            if (parts.Length >= 3)
                            {
                                BuildingType type;
                                if (Enum.TryParse(parts[0], true, out type))
                                {
                                    int x, y;
                                    if (int.TryParse(parts[1].Trim(), out y) && int.TryParse(parts[2].Trim(), out x))
                                    {
                                        // Check if coordinates are within bounds
                                        if (x >= 1 && x <= gridSize && y >= 1 && y <= gridSize)
                                        {
                                            PlaceBuilding(type, x - 1, y - 1); // Adjust coordinates (zero-indexed)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Game loaded successfully.");
                //DisplayFreePlayGrid(); // Display the current grid after loading
                StartFreePlay(); // Start the game or continue further actions
            }
            else
            {
                Console.WriteLine("Saved game file not found.");
            }
        }


        private static void LoadSavedGame()
        {
            Console.WriteLine("Please choose the game mode to load:");
            Console.WriteLine("1. Arcade mode");
            Console.WriteLine("2. Free Play mode");
            Console.Write("Enter your choice (1 or 2): ");
            string choice = Console.ReadLine();
            Console.WriteLine("");

            switch (choice)
            {
                case "1":
                    Console.WriteLine("Loading game in Arcade mode...");
                    ArcadeLoadSavedGame();
                    break;
                case "2":
                    Console.WriteLine("Loading game in Free Play mode...");
                    FreePlayLoadSavedGame();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please enter '1' or '2'.");
                    break;
            }
        }

        private static void UpdateHighScores(string playerName, int score, string highScoreFileName)
        {
            List<string> highScores = new List<string>();
            if (File.Exists(highScoreFileName))
            {
                highScores = File.ReadAllLines(highScoreFileName).ToList();
            }

            bool updated = false;
            for (int i = 0; i < highScores.Count; i++)
            {
                if (highScores[i].StartsWith($"{playerName}:"))
                {
                    int existingScore = int.Parse(highScores[i].Split(':')[1]);
                    if (score > existingScore)
                    {
                        highScores[i] = $"{playerName}:{score}";
                        updated = true;
                    }
                    break;
                }
            }

            if (!updated)
            {
                highScores.Add($"{playerName}:{score}");
            }

            highScores.Sort((a, b) =>
            {
                int scoreA = int.Parse(a.Split(':')[1]);
                int scoreB = int.Parse(b.Split(':')[1]);
                return scoreB.CompareTo(scoreA);
            });

            // Trim the list to the top 10 scores
            if (highScores.Count > 10)
            {
                highScores = highScores.Take(10).ToList();
            }

            File.WriteAllLines(highScoreFileName, highScores);
        }

        private static void DisplayAllHighScores(string highScoreFileName)
        {
            if (File.Exists(highScoreFileName))
            {
                string[] highScores = File.ReadAllLines(highScoreFileName).Take(5).ToArray(); // Read only the top 5 scores
                Console.WriteLine("High Scores:");
                foreach (string score in highScores)
                {
                    string[] parts = score.Split(':');
                    Console.WriteLine($"{parts[0]} - {parts[1]}");
                }
            }
            else
            {
                Console.WriteLine("No high scores found.");
            }
        }

        private static (int x, int y) GetBuildLocation(bool isFirstTurn)
        {
            int x, y;

            while (true)
            {
                Console.Write($"Enter X (1-{gridSize}): ");
                if (int.TryParse(Console.ReadLine(), out x) && x >= 1 && x <= gridSize)
                {
                    break;
                }
                else
                {
                    Console.WriteLine($"Invalid input. Please enter an integer between 1 and {gridSize}.");
                }
            }

            while (true)
            {
                Console.Write($"Enter Y (1-{gridSize}): ");
                if (int.TryParse(Console.ReadLine(), out y) && y >= 1 && y <= gridSize)
                {
                    break;
                }
                else
                {
                    Console.WriteLine($"Invalid input. Please enter an integer between 1 and {gridSize}.");
                }
            }

            Console.WriteLine("");

            return (y, x);
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

            Console.WriteLine($"Checking adjacency for ({y + 1}, {x + 1})");

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize)
                {
                    if (grid[nx, ny] != null)
                    {
                        Console.WriteLine($"Adjacent building found at ({ny + 1}, {nx + 1})");
                        Console.WriteLine("");
                        return true;
                    }
                }
            }
            Console.WriteLine("No adjacent building found.");
            Console.WriteLine("");
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
            Building building = new Building(type, x, y);
            grid[x, y] = building;
            buildings.Add(building);
        }


        private static void RemoveBuilding(int x, int y)
        {
            Building building = grid[x, y];
            buildings.Remove(building);
            grid[x, y] = null;
        }

        private static int CalculateScore()
        {
            int score = 0;

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] != null)
                    {
                        switch (grid[i, j].Type)
                        {
                            case BuildingType.Residential:
                                score += CalculateResidentialScore(i, j);
                                break;
                            case BuildingType.Industry:
                                score += CalculateIndustryScore();
                                break;
                            case BuildingType.Commercial:
                                score += CalculateCommercialScore(i, j);
                                break;
                            case BuildingType.Park:
                                score += CalculateParkScore(i, j);
                                break;
                            case BuildingType.Road:
                                score += CalculateRoadScore(i, j);
                                break;
                        }
                    }
                }
            }

            return score;
        }


        private static int CalculateResidentialScore(int x, int y)
        {
            int score = 0;
            bool adjacentToIndustry = false;

            // Check adjacent cells for residential (R), commercial (C), park (O), and industry (I)
            int[][] directions = new int[][]
            {
                new int[] { 0, 1 },
                new int[] { 1, 0 },
                new int[] { 0, -1 },
                new int[] { -1, 0 }
            };

            foreach (var dir in directions)
            {
                int nx = x + dir[0];
                int ny = y + dir[1];

                if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize && grid[nx, ny] != null)
                {
                    switch (grid[nx, ny].Type)
                    {
                        case BuildingType.Residential:
                        case BuildingType.Commercial:
                            score += 1;
                            break;
                        case BuildingType.Park:
                            score += 2;
                            break;
                        case BuildingType.Industry:
                            adjacentToIndustry = true;
                            break;
                    }
                }
            }

            return adjacentToIndustry ? 1 : score;
        }

        private static int CalculateIndustryScore()
        {
            int score = 0;

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] != null && grid[i, j].Type == BuildingType.Industry)
                    {
                        score += 1; // Each industry scores 1 point per industry in the city
                    }
                }
            }

            return score;
        }

        private static int CalculateCommercialScore(int x, int y)
        {
            int score = 0;

            // Check adjacent cells for commercial (C)
            int[][] directions = new int[][]
            {
                new int[] { 0, 1 },
                new int[] { 1, 0 },
                new int[] { 0, -1 },
                new int[] { -1, 0 }
            };

            foreach (var dir in directions)
            {
                int nx = x + dir[0];
                int ny = y + dir[1];

                if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize && grid[nx, ny] != null && grid[nx, ny].Type == BuildingType.Commercial)
                {
                    score += 1;
                }
            }

            return score;
        }

        private static int CalculateParkScore(int x, int y)
        {
            int score = 0;

            // Check adjacent cells for parks (O)
            int[][] directions = new int[][]
            {
                new int[] { 0, 1 },
                new int[] { 1, 0 },
                new int[] { 0, -1 },
                new int[] { -1, 0 }
            };

            foreach (var dir in directions)
            {
                int nx = x + dir[0];
                int ny = y + dir[1];

                if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize && grid[nx, ny] != null && grid[nx, ny].Type == BuildingType.Park)
                {
                    score += 1;
                }
            }

            return score;
        }

        private static int CalculateRoadScore(int x, int y)
        {
            int score = 0;

            // Check connected roads (*) in the same row
            for (int j = 0; j < gridSize; j++)
            {
                if (grid[x, j] != null && grid[x, j].Type == BuildingType.Road)
                {
                    score += 1;
                }
            }

            return score;
        }

        private static void UpdateCoins()
        {
            int totalCoinsEarned = 0;
            int upkeepCost = 0;

            // Generate coins based on building types placed in previous turns
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] != null && grid[i, j].TurnPlaced < turnNumber)
                    {
                        switch (grid[i, j].Type)
                        {
                            case BuildingType.Residential:
                                totalCoinsEarned += 1; // Each residential building generates 1 coin per turn
                                Console.WriteLine($"Residential at ({j + 1}, {i + 1}) +1 coin");
                                break;
                            case BuildingType.Industry:
                                totalCoinsEarned += 2; // Each industry generates 2 coins per turn
                                Console.WriteLine($"Industry at ({j + 1}, {i + 1}) +2 coins");
                                break;
                            case BuildingType.Commercial:
                                totalCoinsEarned += 3; // Each commercial generates 3 coins per turn
                                Console.WriteLine($"Commercial at ({j + 1}, {i + 1}) +3 coins");
                                break;
                        }
                    }
                }
            }

            coins += totalCoinsEarned;

            // Deduct upkeep costs
            upkeepCost += CalculateResidentialClustersUpkeep(); // Update this line to calculate residential clusters upkeep
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] != null && grid[i, j].TurnPlaced < turnNumber)
                    {
                        switch (grid[i, j].Type)
                        {
                            case BuildingType.Industry:
                                upkeepCost += 1; // Each industry costs 1 coin per turn to upkeep
                                Console.WriteLine($"Industry at ({j + 1}, {i + 1}) -1 coin for upkeep");
                                break;
                            case BuildingType.Commercial:
                                upkeepCost += 2; // Each commercial costs 2 coins per turn to upkeep
                                Console.WriteLine($"Commercial at ({j + 1}, {i + 1}) -2 coins for upkeep");
                                break;
                            case BuildingType.Park:
                                upkeepCost += 1; // Each park costs 1 coin to upkeep
                                Console.WriteLine($"Park at ({j + 1}, {i + 1}) -1 coin for upkeep");
                                break;
                            case BuildingType.Road:
                                if (!IsConnectedRoad(i, j))
                                {
                                    upkeepCost += 1; // Each unconnected road segment costs 1 coin to upkeep
                                    Console.WriteLine($"Unconnected road at ({j + 1}, {i + 1}) -1 coin for upkeep");
                                }
                                break;
                        }
                    }
                }
            }

            coins -= upkeepCost;
        }

        private static int CalculateResidentialClustersUpkeep()
        {
            int upkeep = 0;
            bool[,] visited = new bool[gridSize, gridSize];

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] != null && grid[i, j].Type == BuildingType.Residential && !visited[i, j])
                    {
                        if (IsClusterResidential(i, j, visited))
                        {
                            upkeep += 1; // Each distinct cluster of residential buildings requires 1 coin per turn to upkeep
                            Console.WriteLine($"Residential cluster at ({j + 1}, {i + 1}) -1 coin for upkeep");
                        }
                    }
                }
            }

            return upkeep;
        }

        private static bool IsClusterResidential(int x, int y, bool[,] visited)
        {
            Stack<(int, int)> stack = new Stack<(int, int)>();
            stack.Push((x, y));
            visited[x, y] = true;

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            while (stack.Count > 0)
            {
                (int cx, int cy) = stack.Pop();

                for (int i = 0; i < 4; i++)
                {
                    int nx = cx + dx[i];
                    int ny = cy + dy[i];

                    if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize && grid[nx, ny] != null && grid[nx, ny].Type == BuildingType.Residential && !visited[nx, ny])
                    {
                        stack.Push((nx, ny));
                        visited[nx, ny] = true;
                    }
                }
            }

            return true;
        }

        private static bool IsConnectedRoad(int x, int y)
        {
            // Check if the road segment is connected to another road in the same row or column
            int[][] directions = new int[][]
            {
                new int[] { 0, 1 },
                new int[] { 1, 0 },
                new int[] { 0, -1 },
                new int[] { -1, 0 }
            };

            foreach (var dir in directions)
            {
                int nx = x + dir[0];
                int ny = y + dir[1];

                if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize && grid[nx, ny] != null && grid[nx, ny].Type == BuildingType.Road)
                {
                    return true;
                }
            }

            return false;
        }

        public class Building
        {
            public BuildingType Type { get; }
            public int X { get; }
            public int Y { get; }
            public int TurnPlaced { get; }

            public Building(BuildingType type, int x, int y)
            {
                Type = type;
                X = x;
                Y = y;
                TurnPlaced = turnNumber;
            }
        }
    }
}


