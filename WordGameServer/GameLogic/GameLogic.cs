using System;
using System.Collections.Generic;
using System.Linq;
using WordGameServer.Common.NetworkCommandCodes;

namespace WordGameServer.GameLogic
{
    /// <summary>
    /// This class houses the game logic operations of the server.
    /// It can choose a word to guess, check if a certain word exist in a dictionary and evaluate a guess
    /// </summary>
    public class GameLogic
    {
        private List<string>               _acceptableWordPool;
        private Dictionary<string, string> _wordsToGuess;
        private Random                     _random;

        private const string WORD_POOL_FILE_PATH =
            @"C:\Users\wolverine1984\RiderProjects\WordGameServer\WordGameServer\Resources\all_words\words_of_length_5.csv";

        private const string COMMON_WORDS_FILE_PATH =
            @"C:\Users\wolverine1984\RiderProjects\WordGameServer\WordGameServer\Resources\common_words\common_words_of_length_5.csv";

        private const string ERROR_CODE = "99999";

        public GameLogic()
        {
            _acceptableWordPool = LoadCSVFileToStringListFromPath(WORD_POOL_FILE_PATH);
            _wordsToGuess       = new Dictionary<string, string>();
            _random             = new Random();
            Console.WriteLine(_acceptableWordPool.ToString());
        }


        /// <summary>
        /// Checks if a specific word exist in the acceptable word pool.
        /// </summary>
        /// <param name="wordToCheck">The word to check against the acceptable words pool</param>
        /// <returns>True if this word exist in the word pool, False otherwise.</returns>
        public bool CheckIsViableWord(string wordToCheck)
        {
            return _acceptableWordPool.Contains(wordToCheck.ToLower());
        }

        /// <summary>
        /// Picks a new word a specific player needs to guess. If the player already exists in the internal dictionary
        /// we pick an new word for him to guess. If he doesn't we add him to the dictionary and pick a new word.
        /// </summary>
        /// <param name="playerIdentifier">string identifier of the player</param>
        public string PickWordToGuess(string playerIdentifier)
        {
            var possibleWords = LoadCSVFileToStringListFromPath(COMMON_WORDS_FILE_PATH);
            var wordToGuess   = possibleWords[_random.Next(0, possibleWords.Count)];

            //player already exist in the dictionary
            if (_wordsToGuess.Keys.Contains(playerIdentifier))
            {
                _wordsToGuess[playerIdentifier] = wordToGuess;
            }
            else
            {
                _wordsToGuess.Add(playerIdentifier, wordToGuess);
            }

            return wordToGuess;
        }

        /// <summary>
        /// Evaluates players guess. Checks if for each character of the player guess if it exist in the
        /// word they need to guess and if it does if it's in the correct place or not.
        /// returns a string of digits that correspond to the index of the letters in the evaluated guess.
        /// For example: If the word to guess is 'brain' and the user guesses 'crane' the output will be`02200`
        /// because `r` and `a` exist in `brain` and they are in the correct place. 
        /// </summary>
        /// <param name="playerIdentifier">string identifier of the player</param>
        /// <param name="playerGuess">The guess we're evaluating</param>
        /// <returns>
        /// String of digits. Each digit correspond to a letter in the guess where
        /// `0` means the character doesn't exist in the word at all.
        /// `1` means the character exist in the word but in the incorrect place.
        /// `2` means that the character exist and is in the correct place.
        /// If an error occurs returns '99999'
        /// </returns>
        public string EvaluateGuess(string playerIdentifier, string playerGuess)
        {
            Console.WriteLine($"Evaluating guess for Player: {playerIdentifier} Guess: {playerGuess} ");
            if (!_wordsToGuess.ContainsKey(playerIdentifier))
            {
                Console.WriteLine($"ERROR! Player: {playerIdentifier} Doesn't exist in the player dictionary! ");
                return ERROR_CODE;
            }

            var wordToCheckAgainst = _wordsToGuess[playerIdentifier];

            if (wordToCheckAgainst.Length != playerGuess.Length)
            {
                Console.WriteLine(
                    $"ERROR! Player guess word {playerGuess} is of length: {playerGuess.Length} but it should be of length {wordToCheckAgainst.Length}");
                return ERROR_CODE;
            }

            var guessEvaluation = "";

            for (var i = 0; i < playerGuess.Length; i++)
            {
                // current char exist in the word.
                if (wordToCheckAgainst.Contains(playerGuess[i]))
                {
                    // current char exist in the word and is in the correct place.
                    if (wordToCheckAgainst[i] == playerGuess[i])
                    {
                        Console.WriteLine($"Letter {playerGuess[i]} Exist in the word and it's in the correct place!");
                        guessEvaluation += LetterEvaluationCodes.LETTER_EXISTS_IN_WORD_AND_IN_CORRECT_PLACE;
                    }
                    else
                    {
                        Console.WriteLine($"Letter {playerGuess[i]} Exist in the BUT it's NOT in the correct place!");
                        guessEvaluation += LetterEvaluationCodes.LETTER_EXISTS_IN_WORD_BUT_NOT_IN_ORDER;
                    }
                }
                else //character doesn't exist in the word at all
                {
                    Console.WriteLine($"Letter {playerGuess[i]} NOT exist in the word at all!");
                    guessEvaluation += LetterEvaluationCodes.LETTER_DOESNT_EXIST_IN_WORD;
                }
            }

            return guessEvaluation;
        }


        /// <summary>
        /// Loads words from a CSV file in the provided path. Also converts all words to lower case.
        /// </summary>
        /// <param name="pathToLoad">Path to the CSV file containing the words</param>
        /// <returns>List of strings with the loaded words</returns>
        private List<string> LoadCSVFileToStringListFromPath(string pathToLoad)
        {
            return System.IO.File.ReadAllText(pathToLoad).Split(',')
                         .Select(x => x.ToLower()).ToList();
        }
    }
}