using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LocalKnights.Common.Helpers
{
    public static class PgnAnalysisHelper
    {
        private const string LichessAnalysisBaseUrl = "https://lichess.org/analysis/pgn/";

        private static readonly Regex TagPairLineRegex = new(@"^\s*\[[^\]]*\]\s*$", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex BraceCommentRegex = new(@"\{[^}]*\}", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex SemicolonCommentRegex = new(@";[^\r\n]*", RegexOptions.Compiled);
        private static readonly Regex MovePrefixRegex = new(@"^\d+\.(\.\.)?", RegexOptions.Compiled);
        private static readonly Regex AnnotationSuffixRegex = new(@"[!?]+$", RegexOptions.Compiled);

        // Accepts the SAN notations used in this project and validates each extracted move token.
        private static readonly Regex SanMoveRegex = new(
            @"^(O-O(-O)?|" +
            @"[a-h][1-8](=[QRBN])?[+#]?|" +
            @"[a-h]x[a-h][1-8](=[QRBN])?[+#]?|" +
            @"[KQRBN][a-h]?[1-8]?x?[a-h][1-8][+#]?|" +
            @"[KQRBN][a-h]?[1-8]?[+#]?)$",
            RegexOptions.Compiled);

        public static bool TryBuildLichessAnalysisUrl(string? pgnText, out string analysisUrl)
        {
            analysisUrl = string.Empty;

            if (!TryExtractMoves(pgnText, out List<string> moves))
            {
                return false;
            }

            string encodedMoves = Uri.EscapeDataString(string.Join(" ", moves));
            analysisUrl = LichessAnalysisBaseUrl + encodedMoves;
            return true;
        }

        private static bool TryExtractMoves(string? pgnText, out List<string> moves)
        {
            moves = new List<string>();

            if (string.IsNullOrWhiteSpace(pgnText))
            {
                return false;
            }

            string text = pgnText;
            text = TagPairLineRegex.Replace(text, " ");
            text = BraceCommentRegex.Replace(text, " ");
            text = SemicolonCommentRegex.Replace(text, " ");
            text = RemoveParenthesizedVariations(text);

            List<string> rawTokens = text
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
                .ToList();

            foreach (string raw in rawTokens)
            {
                string token = MovePrefixRegex.Replace(raw, string.Empty);
                token = AnnotationSuffixRegex.Replace(token, string.Empty);

                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (IsResultToken(token) || token == "--")
                {
                    continue;
                }

                if (!SanMoveRegex.IsMatch(token))
                {
                    return false;
                }

                moves.Add(token);
            }

            return moves.Count > 0;
        }

        private static bool IsResultToken(string token)
        {
            return token is "1-0" or "0-1" or "1/2-1/2" or "\u00bd-\u00bd" or "*";
        }

        private static string RemoveParenthesizedVariations(string text)
        {
            string current = text;
            string next;

            do
            {
                next = Regex.Replace(current, @"\([^()]*\)", " ");
                if (ReferenceEquals(next, current))
                {
                    break;
                }

                current = next;
            }
            while (current.Contains('(') && current.Contains(')'));

            return current;
        }
    }
}
