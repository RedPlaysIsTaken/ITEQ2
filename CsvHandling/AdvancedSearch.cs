using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ITEQ2.CsvHandling
{
    public static class AdvancedSearch
    {
        public static List<EquipmentObject> Search(IEnumerable<EquipmentObject> dataset, string query)
        {
            var ast = SearchParser.Parse(query);
            if (ast == null)
            {
                return dataset.ToList();
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
        private static readonly Regex TokenRegex = new(@"(?<field>\w+):(?<term>\w+)|(?<termOnly>\w+)|(?<op>\|\||&&)|(?<paren>[()])", RegexOptions.IgnoreCase);

        public static INode Parse(string input)
        {
            var tokens = Tokenize(input);
            var ast = ParseExpression(tokens);
            return ast;
        }

        private static Queue<string> Tokenize(string input)
        {
            var tokens = new Queue<string>();
            foreach (Match match in TokenRegex.Matches(input))
            {
                if (match.Groups["field"].Success)
                    tokens.Enqueue($"{match.Groups["field"].Value}:{match.Groups["term"].Value}");
                else if (match.Groups["termOnly"].Success)
                    tokens.Enqueue(match.Groups["termOnly"].Value);
                else if (match.Groups["op"].Success)
                    tokens.Enqueue(match.Groups["op"].Value);
                else if (match.Groups["paren"].Success)
                    tokens.Enqueue(match.Groups["paren"].Value);
            }
            return tokens;
        }

        private static INode ParseExpression(Queue<string> tokens)
        {
            var stack = new Stack<INode>();
            var ops = new Stack<string>();

            while (tokens.Count > 0)
            {
                string token = tokens.Dequeue();

                if (token == "(")
                {
                    stack.Push(ParseExpression(tokens));
                }
                else if (token == ")")
                {
                    break;
                }
                else if (token == "&&" || token == "||")
                {
                    ops.Push(token);
                }
                else
                {
                    INode term;
                    if (token.Contains(':'))
                    {
                        var parts = token.Split(':');
                        term = new TermNode(parts[0], parts[1]);
                    }
                    else
                    {
                        term = new TermNode(null, token);
                    }
                    stack.Push(term);

                    if (ops.Count > 0 && stack.Count >= 2)
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        var op = ops.Pop();
                        stack.Push(op == "&&" ? new AndNode(left, right) : new OrNode(left, right));
                    }
                }
            }

            return stack.Count == 1 ? stack.Pop() : null;
        }
    }
}
