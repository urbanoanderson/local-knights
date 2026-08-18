using System.Runtime.InteropServices;
using System.Text.Json;
using LocalKnights.Common.Models;

namespace LocalKnights.Common.Helpers.ChessAlgorithms.Pairing
{
    /// <summary>
    /// Generates pairings for round robin competitions.
    /// </summary>
    public static class RoundRobinPairingAlgorithm
    {
        [DllImport("chesspairing.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GenerateRoundRobinPairings([MarshalAs(UnmanagedType.LPUTF8Str)] string input);

        [DllImport("chesspairing.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GenerateDoubleRoundRobinPairings([MarshalAs(UnmanagedType.LPUTF8Str)] string input);

        /// <summary>
        /// Generates round robin pairings for the specified round.
        /// </summary>
        /// <param name="competition">The competition.</param>
        /// <returns>The generated pairings.</returns>
        public static List<CompetitionPairing> GenerateCurrentRoundPairings(Competition competition)
        {
            List<CompetitionPairing> pairings = new();

            try
            {
                // Create input dict to be serialized and passed to chesspairing library
                Dictionary<string, object> inputDict = new();
                inputDict["currentRound"] = competition.CurrentRound;

                // Add participant players to input dict
                List<Dictionary<string, object>> inputPlayers = new();

                foreach (CompetitionParticipant p in competition.CompetitionParticipants)
                {
                    Dictionary<string, object> pDict = new Dictionary<string, object>
                    {
                        ["id"] = p.ParticipantIdentifierName,
                        ["name"] = p.ParticipantIdentifierName,
                        ["rating"] = (int)p.OldRating.Rating
                    };

                    inputPlayers.Add(pDict);
                }

                inputDict["players"] = inputPlayers.ToArray();

                // Add previous rounds information to input dict
                List<Dictionary<string, object>> inputRounds = new();

                for (int round = 1; round < competition.CurrentRound; round++)
                {
                    IEnumerable<CompetitionPairing> roundPairings = competition.CompetitionPairings.Where(p => p.RoundNumber == round);
                    List<Dictionary<string, object>> roundGames = new();
                    List<Dictionary<string, object>> roundByes = new();
                    Dictionary<string, object> roundDict = new();
                    roundDict["number"] = round;

                    foreach (CompetitionPairing p in roundPairings)
                    {
                        // If BYE pairing
                        if (p.TableNumber < 0 || string.IsNullOrWhiteSpace(p.BlackPlayerIdentifierName))
                        {
                            Dictionary<string, object> bDict = new Dictionary<string, object>
                            {
                                ["playerID"] = p.WhitePlayerIdentifierName,
                            };

                            roundByes.Add(bDict);
                        }
                        else
                        {
                            Dictionary<string, object> gDict = new Dictionary<string, object>
                            {
                                ["whiteID"] = p.WhitePlayerIdentifierName,
                                ["blackID"] = p.BlackPlayerIdentifierName,
                                ["result"] = p.Result == GameResult.WhiteWins ? "1-0" : p.Result == GameResult.BlackWins ? "0-1" : "0.5-0.5"
                            };

                            roundGames.Add(gDict);
                        }
                    }

                    roundDict["games"] = roundGames.ToArray();
                    roundDict["byes"] = roundByes.ToArray();
                    inputRounds.Add(roundDict);
                }

                inputDict["rounds"] = inputRounds.ToArray();

                // Serialize input and call chesspairing library
                string json = JsonSerializer.Serialize(inputDict);
                IntPtr ptr = (bool)competition.IsDoubleRoundRobin ? GenerateDoubleRoundRobinPairings(json) : GenerateRoundRobinPairings(json);
                string result = Marshal.PtrToStringUTF8(ptr) ?? string.Empty;

                if (!LooksLikeJson(result))
                {
                    throw new InvalidOperationException($"Round-robin pairing generation failed: {result}");
                }

                // Parse results and populate output list
                var doc = JsonDocument.Parse(result);
                var root = doc.RootElement;

                // Pairings array
                if (root.TryGetProperty("Pairings", out JsonElement jsonPairings) && jsonPairings.ValueKind != JsonValueKind.Null)
                {
                    foreach (var jsonPairing in jsonPairings.EnumerateArray())
                    {
                        CompetitionPairing p = new CompetitionPairing
                        {
                            RoundNumber = competition.CurrentRound,
                            TableNumber = jsonPairing.GetProperty("Board").GetInt32(),
                            WhitePlayerIdentifierName = jsonPairing.GetProperty("WhiteID").GetString(),
                            BlackPlayerIdentifierName = jsonPairing.GetProperty("BlackID").GetString(),
                        };

                        pairings.Add(p);
                    }
                }

                // BYEs array
                if (root.TryGetProperty("Byes", out JsonElement jsonByes) && jsonByes.ValueKind != JsonValueKind.Null)
                {
                    int byeTable = -1;
                    foreach (var jsonBye in jsonByes.EnumerateArray())
                    {
                        CompetitionPairing p = new CompetitionPairing
                        {
                            RoundNumber = competition.CurrentRound,
                            TableNumber = byeTable,
                            WhitePlayerIdentifierName = jsonBye.GetProperty("PlayerID").GetString(),
                            BlackPlayerIdentifierName = string.Empty,
                        };

                        pairings.Add(p);
                        byeTable--;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return pairings;
        }

        /// <summary>
        /// Calculates the number of rounds needed for a round robin tournament based on the number of participants and whether it's a double round robin.
        /// </summary>
        /// <param name="participantCount"></param>
        /// <param name="isDoubleRoundRobin"></param>
        /// <returns></returns>
        public static int CalculateNumberOfRounds(int participantCount, bool isDoubleRoundRobin)
        {
            if (participantCount < 2)
            {
                return 0;
            }

            bool oddParticipants = participantCount % 2 == 1;
            int baseRounds = oddParticipants ? participantCount : participantCount - 1;
            return isDoubleRoundRobin ? baseRounds * 2 : baseRounds;
        }

        private static bool LooksLikeJson(string value)
        {
            foreach (char character in value)
            {
                if (char.IsWhiteSpace(character))
                {
                    continue;
                }

                return character == '{' || character == '[';
            }

            return false;
        }
    }
}
