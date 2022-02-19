using System;
using System.Collections.Generic;
using System.Linq;

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

        private const string WORD_POOL_FILE_PATH =
            @"C:\Users\wolverine1984\RiderProjects\WordGameServer\WordGameServer\Resources\all_words\words_of_length_5.csv";
        
        private const string COMMON_WORDS_FILE_PATH =
            @"C:\Users\wolverine1984\RiderProjects\WordGameServer\WordGameServer\Resources\common_words\common_words_of_length_5.csv";

        public GameLogic()
        {
            _acceptableWordPool = new List<string>();
            _wordsToGuess       = new Dictionary<string, string>();
            LoadAcceptableWordPoolFromCsv();
            Console.WriteLine(_acceptableWordPool);
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
        /// Load the word pool that we consider to be acceptable from the WORD_POOL_FILE_PATH path.
        /// </summary>
        private void LoadAcceptableWordPoolFromCsv()
        {
            _acceptableWordPool = System.IO.File.ReadAllText(WORD_POOL_FILE_PATH).Split(',')
                                        .Select(x => x.ToLower()).ToList();
        }
    }
}