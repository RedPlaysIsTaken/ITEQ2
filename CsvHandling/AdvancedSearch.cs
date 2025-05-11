using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ITEQ2.CsvHandling
{
    public class AdvancedSearch
    {
        public string SearchErrorFeedback { get; private set; } = "";

        public List<EquipmentObject> Search(IEnumerable<EquipmentObject> dataset, string query)
        {
            SearchErrorFeedback = "";

            if (string.IsNullOrWhiteSpace(query))
            {
                return dataset.ToList();
            }

            var ast = SearchParser.Parse(query);

            if (ast == null)
            {
                SearchErrorFeedback = "Invalid search query. Check syntax";

                return new List<EquipmentObject>();
            }

            return dataset.Where(obj => ast.Evaluate(obj)).ToList();
        }
    }

    public interface INode
    {
        bool Evaluate(EquipmentObject obj);
    }

    public class TermNode : INode
    {
        public string Field { get; }
        public string Value { get; }

        public TermNode(string field, string value)
        {
            Field = field;
            Value = value;
        }

        public bool Evaluate(EquipmentObject obj)
        {
            if (string.IsNullOrWhiteSpace(Field))
            {
                return obj.GetType().GetProperties()
                    .Any(prop => prop.GetValue(obj)?.ToString()
                        .IndexOf(Value, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var propInfo = obj.GetType().GetProperty(Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null) return false;

            var val = propInfo.GetValue(obj)?.ToString();
            return val != null && val.IndexOf(Value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public class AndNode : INode
    {
        public INode Left, Right;
        public AndNode(INode left, INode right) { Left = left; Right = right; }
        public bool Evaluate(EquipmentObject obj) => Left.Evaluate(obj) && Right.Evaluate(obj);
    }

    public class OrNode : INode
    {
        public INode Left, Right;
        public OrNode(INode left, INode right) { Left = left; Right = right; }
        public bool Evaluate(EquipmentObject obj) => Left.Evaluate(obj) || Right.Evaluate(obj);
    }

    public static class SearchParser
    {
        private static readonly Regex TokenRegex = new(
    @"(?<field>\w+):(?<term>\w+)|(?<termOnly>\w+)|(?<op>\|\||&&)|(?<not>!)|(?<paren>[()])",
    RegexOptions.IgnoreCase);

        public static INode Parse(string input)
        {
            try
            {
                var tokens = Tokenize(input);
                var ast = ParseExpression(tokens);

                if (tokens.Count > 0)
                    throw new InvalidOperationException("Unexpected tokens remaining after parsing.");

                return ast;
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Search query parse error: {ex.Message}");
                return null;
            }
        }

        private static Queue<string> Tokenize(string input)
        {
            var tokens = new Queue<string>();

            if (string.IsNullOrWhiteSpace(input))
                return tokens;

            foreach (Match match in TokenRegex.Matches(input))
            {
                if (match.Groups["field"].Success)
                    tokens.Enqueue($"{match.Groups["field"].Value}:{match.Groups["term"].Value}");
                else if (match.Groups["termOnly"].Success)
                    tokens.Enqueue(match.Groups["termOnly"].Value);
                else if (match.Groups["op"].Success)
                    tokens.Enqueue(match.Groups["op"].Value);
                else if (match.Groups["not"].Success)
                    tokens.Enqueue("!");
                else if (match.Groups["paren"].Success)
                    tokens.Enqueue(match.Groups["paren"].Value);
            }

            return tokens;
        }

        private static INode ParseExpression(Queue<string> tokens)
        {
            if (tokens == null || tokens.Count == 0)
            {
                Debug.WriteLine("Searchfield is empty");
                return null;
            }
            else
            {
                return ParseOr(tokens);
            }
                        
        }
        public class NotNode : INode
        {
            public INode Operand { get; }

            public NotNode(INode operand)
            {
                Operand = operand;
            }

            public bool Evaluate(EquipmentObject obj)
            {
                return !Operand.Evaluate(obj);
            }
        }
        private static INode ParseOr(Queue<string> tokens)
        {
            var left = ParseAnd(tokens);

            while (tokens.Count > 0 && tokens.Peek() == "||")
            {
                tokens.Dequeue();

                if (tokens.Count == 0 || tokens.Peek() == ")")
                    throw new InvalidOperationException("Expected expression after '||'");

                var right = ParseAnd(tokens);
                left = new OrNode(left, right);
            }

            return left;
        }

        private static INode ParseAnd(Queue<string> tokens)
        {
            var left = ParsePrimary(tokens);

            while (tokens.Count > 0 && tokens.Peek() == "&&")
            {
                tokens.Dequeue();

                if (tokens.Count == 0 || tokens.Peek() == ")")
                    throw new InvalidOperationException("Expected expression after '&&'");

                var right = ParsePrimary(tokens);
                left = new AndNode(left, right);
            }

            return left;
        }

        private static INode ParsePrimary(Queue<string> tokens)
        {
            if (tokens.Count == 0)
                throw new InvalidOperationException("Unexpected end of input");

            string token = tokens.Dequeue();

            if (token == "!")
            {
                if (tokens.Count == 0)
                    throw new InvalidOperationException("Expected expression after '!'");

                var operand = ParsePrimary(tokens);
                return new NotNode(operand);
            }

            if (token == "(")
            {
                var expr = ParseExpression(tokens);
                if (tokens.Count == 0 || tokens.Dequeue() != ")")
                    throw new InvalidOperationException("Missing closing parenthesis");
                return expr;
            }

            if (token.Contains(":"))
            {
                var parts = token.Split(':');
                return new TermNode(parts[0], parts[1]);
            }
            else
            {
                return new TermNode(null, token);
            }
        }
    }
}
