using System.Text.RegularExpressions;

namespace WordGameServer.Common
{
    public class CommunicationStruct
    {
        public        int    RequestId;
        private const string REQUEST_ID_STRING = "RequestId";

        public        int    RequestCommand;
        private const string REQUEST_COMMAND_STRING = "RequestCommand";

        public        string PlayerIdentifier;
        private const string PLAYER_IDENTIFIER_STRING = "PlayerIdentifier";

        public        string Payload;
        private const string PAYLOAD_STRING = "Payload";

        private readonly Regex _regex;

        public CommunicationStruct()
        {
            _regex = new Regex($"{REQUEST_ID_STRING}:(?<{REQUEST_ID_STRING}>\\d+) "               +
                               $"{REQUEST_COMMAND_STRING}:(?<{REQUEST_COMMAND_STRING}>\\d+) "     +
                               $"{PLAYER_IDENTIFIER_STRING}:(?<{PLAYER_IDENTIFIER_STRING}>\\w+) " +
                               $"{PAYLOAD_STRING}:(?<{PAYLOAD_STRING}>\\w+)");
        }

        public override string ToString()
        {
            return $"{REQUEST_ID_STRING}:{RequestId} "               +
                   $"{REQUEST_COMMAND_STRING}:{RequestCommand} "     +
                   $"{PLAYER_IDENTIFIER_STRING}:{PlayerIdentifier} " +
                   $"{PAYLOAD_STRING}:{Payload}";
        }

        public void FromString(string inputString)
        {
            var match = _regex.Match(inputString);
            if (!match.Success)
            {
                //throw exception!  
            }

            RequestId        = int.Parse(match.Groups[$"{REQUEST_ID_STRING}"].Value);
            RequestCommand   = int.Parse(match.Groups[$"{REQUEST_COMMAND_STRING}"].Value);
            PlayerIdentifier = match.Groups[$"{PLAYER_IDENTIFIER_STRING}"].Value;
            Payload          = match.Groups[$"{PAYLOAD_STRING}"].Value;
        }
    }
}