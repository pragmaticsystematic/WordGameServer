namespace WordGameServer.Common
{
    namespace NetworkCommandCodes
    {
        public static class NetworkClientRequestCommandCodes
        {
            public const int GUESS_NEW_WORD     = 0;
            public const int EVALUATE_GUESS     = 1;
            public const int CHECK_WORD_EXISTS  = 2;
            public const int REQUEST_DISCONNECT = 9;
        }

        public static class NetworkServerReplyCommandCodes
        {
            public const int REQUEST_SUCCESS = 0;
            public const int REQUEST_FAILURE = 1;
        }

        public static class LetterEvaluationCodes
        {
            public const string LETTER_DOESNT_EXIST_IN_WORD                = "0";
            public const string LETTER_EXISTS_IN_WORD_BUT_NOT_IN_ORDER     = "1";
            public const string LETTER_EXISTS_IN_WORD_AND_IN_CORRECT_PLACE = "2";
            public const string SOMETHING_WENT_WRONG_YOU_SHOULD_PANIK      = "9";
        }
    }
}