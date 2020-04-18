using System;
using System.Collections.Generic;
using System.Linq;

namespace lab5
{
    class Logger
    {
        public static LogLevelEnum LogLevel { get; set; }

        /// <summary>
        /// Log message with specify log levelEnum
        /// </summary>
        /// <param name="text"></param>
        /// <param name="levelEnum"></param>
        public static void Log(object text, LogLevelEnum levelEnum = LogLevelEnum.Info)
        {
            if (levelEnum >= LogLevel)
            {
                Console.WriteLine(text.ToString());
            }
        }

        public enum LogLevelEnum
        {
            Debug,
            Info
        }
    }

    struct PatternOccurrence
    {
        public int CharPosition { get; set; }

        public int PatternNumber { get; set; }
    }

    class Node
    {
        private readonly Node parent;
        private readonly Node rootNode;
        private Node suffixLink;
        private Node supressedSuffixLink;
        private readonly Dictionary<char, Node> children = new Dictionary<char, Node>();
        private readonly Dictionary<char, Node> transitions = new Dictionary<char, Node>();
        private readonly List<PatternInfo> patternInfos = new List<PatternInfo>();

        public Node()
        {
            rootNode = this;
        }

        private Node(Node rootNode, Node parent)
        {
            this.parent = parent;
            this.rootNode = rootNode;
        }

        public static char? Joker { get; set; }

        public static char? JokerException { get; set; }

        public void AddString(string str, int numberOfPattern, int index = -1)
        {
            Logger.Log($"Add string {str} with number of pattern {numberOfPattern+1}", Logger.LogLevelEnum.Debug);
            // If not root node
            if (index != -1)
            {
                Logger.Log($"\tProceed => {str[index]}", Logger.LogLevelEnum.Debug);
                Name = str[index];
            }

            if (index == str.Length - 1)
            {
                Logger.Log($"Finish add string {str} with number of pattern {numberOfPattern+1}", Logger.LogLevelEnum.Debug);
                
                // Save info about pattern
                patternInfos.Add(new PatternInfo
                {
                    PatternNumber = numberOfPattern + 1,
                    PatternLength = str.Length
                });
                return;
            }


            var nextChar = str[index + 1];
            Node existedNode;
            if (!children.TryGetValue(nextChar, out existedNode))
            {
                Logger.Log($"\tAdd new node", Logger.LogLevelEnum.Debug);
                existedNode = new Node(rootNode, this);
                children.Add(nextChar, existedNode);
            }
            else
            {
                Logger.Log($"\tFound existing node", Logger.LogLevelEnum.Debug);
            }

            existedNode.AddString(str, numberOfPattern, ++index);
        }

        public IEnumerable<PatternOccurrence> FindPatternsInText(string text)
        {
            Logger.Log($"Search patterns in text:", Logger.LogLevelEnum.Debug);
            var retList = new List<PatternOccurrence>();
            var current = rootNode;
            for (var i = 0; i < text.Length; ++i)
            {
                current = GetTransition(current, text[i]);
                Logger.Log($"Proceed text[{i}/{text.Length-1}] => {text[i]}. Current state: {current.Name}. Terminal: {current.IsTerminal}",
                    Logger.LogLevelEnum.Debug);    
                if (current.IsTerminal)
                {
                    Logger.Log($"Found occurrence at index {i}", Logger.LogLevelEnum.Debug);    
                    retList.AddRange(current.ComputePatternOccurrences(i));
                }
                var currentSuppressedSuffixLink = GetSuppressedSuffixLink(current);
                Logger.Log($"Suppressed suffix link: {currentSuppressedSuffixLink.Name}. Root: {currentSuppressedSuffixLink.IsRoot}",
                    Logger.LogLevelEnum.Debug);
                
                // Find all others terminals
                while (!currentSuppressedSuffixLink.IsRoot)
                {
                    retList.AddRange(currentSuppressedSuffixLink.ComputePatternOccurrences(i));
                    currentSuppressedSuffixLink = GetSuppressedSuffixLink(currentSuppressedSuffixLink);
                    Logger.Log($"Suppressed suffix link: {currentSuppressedSuffixLink.Name}. Root: {currentSuppressedSuffixLink.IsRoot}",
                        Logger.LogLevelEnum.Debug);
                }
            }

            return retList;
        }

        private bool IsRoot
        {
            get { return parent == null; }
        }

        private bool IsTerminal
        {
            get { return patternInfos.Count != 0; }
        }

        private char Name { get; set; } = 'Z'; // Stub name for root

        private IEnumerable<PatternOccurrence> ComputePatternOccurrences(int index)
        {
            return patternInfos.Select(x => new PatternOccurrence
                {
                    CharPosition = index - x.PatternLength + 2, // Find start index of occurrence 
                    PatternNumber = x.PatternNumber
            });
        }

        private Node GetSuffixLink(Node node)
        {
            Logger.Log($"Getting suffix for {node.Name}...",
                Logger.LogLevelEnum.Debug);
            if (node.suffixLink != null)
            {
                Logger.Log($"\tSuffix link for node {node.Name} already computed to {node.suffixLink.Name}",
                    Logger.LogLevelEnum.Debug);
                return node.suffixLink;
            }

            if (node.IsRoot || node.parent.IsRoot)
            {
                Logger.Log($"\tNode or parent of node is root.",
                    Logger.LogLevelEnum.Debug);
                node.suffixLink = rootNode;
            }
            else
            {
                // Try to find at transition by parent's suffix link
                node.suffixLink = GetTransition(GetSuffixLink(node.parent), node.Name);
            }

            Logger.Log($"Suffix link for node {node.Name} = {node.suffixLink.Name}",
                Logger.LogLevelEnum.Debug);
            return node.suffixLink;
        }

        private Node GetTransition(Node node, char c)
        {
            Logger.Log($"Getting transition for {node.Name}... by symbol {c}",
                Logger.LogLevelEnum.Debug);
            Node retNode;
            if (node.transitions.TryGetValue(c, out retNode))
            {
                Logger.Log($"\tTransition for node {node.Name} by symbol {c} already computed to {retNode.Name}",
                    Logger.LogLevelEnum.Debug);
                return retNode;
            }

            var isAcceptableForJoker = JokerException == null || c != JokerException;

            if (JokerException != null)
            {
                Logger.Log($"\tSymbol {c} is acceptable for joker : {isAcceptableForJoker}",
                    Logger.LogLevelEnum.Debug);    
            }

            if (node.children.TryGetValue(c, out retNode) 
                || isAcceptableForJoker 
                && Joker != null 
                && node.children.TryGetValue((char)Joker, out retNode))
            {
                Logger.Log($"\tFound symbol {c} among children of",
                    Logger.LogLevelEnum.Debug);    
                node.transitions.Add(c, retNode);
            }
            else if (node == rootNode)
            {
                Logger.Log($"\tNode is root node",
                    Logger.LogLevelEnum.Debug);  
                node.transitions.Add(c, rootNode);
            }
            else
            {
                // Fallback to suffix link
                node.transitions.Add(c, GetTransition(GetSuffixLink(node), c));
            }

            node.transitions.TryGetValue(c, out retNode);
            
            Logger.Log($"Transition for node {node.Name} by symbol {c} = {retNode?.Name}",
                Logger.LogLevelEnum.Debug);  
            return retNode;
        }

        private Node GetSuppressedSuffixLink(Node node)
        {
            Logger.Log($"Getting suppressed suffix link for {node.Name}...",
                Logger.LogLevelEnum.Debug);
            if (node.supressedSuffixLink != null)
            {
                Logger.Log($"\tSuppressed suffix link for {node.Name} is already computed to {node.supressedSuffixLink.Name}",
                    Logger.LogLevelEnum.Debug);
                return node.supressedSuffixLink;
            }

            var computedSuffixLink = GetSuffixLink(node);
            if (computedSuffixLink.IsTerminal || computedSuffixLink == rootNode)
            {
                Logger.Log($"\tSuffix link for node {node.Name} = {computedSuffixLink.Name}. Terminal: {computedSuffixLink.IsTerminal}. Root: {computedSuffixLink.IsRoot}",
                    Logger.LogLevelEnum.Debug);
                node.supressedSuffixLink = computedSuffixLink;
            }
            else
            {
                // Fallback to suffix link
                node.supressedSuffixLink = GetSuppressedSuffixLink(GetSuffixLink(node));
            }
            
            Logger.Log($"Suppressed suffix link for node {node.Name} = {node.supressedSuffixLink.Name}",
                Logger.LogLevelEnum.Debug);  
            return node.supressedSuffixLink;
        }
        
        private struct PatternInfo
        {
            public int PatternNumber { get; set; }

            public int PatternLength { get; set; }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var logLevelFromEnv = Environment.GetEnvironmentVariable("LAB5_LOG_LEVEL");
            Logger.LogLevel = string.IsNullOrWhiteSpace(logLevelFromEnv)
                ? Logger.LogLevelEnum.Info
                : (Logger.LogLevelEnum)Enum.Parse(typeof(Logger.LogLevelEnum), logLevelFromEnv);
            const string firstStepNumber = "1";
            
            var text = Console.ReadLine();
            var root = new Node();
            
            // Use dotnet run {stepNumber} for choice step number
            // stepNumber == 1 by default
            var isFirstStep = args.Length == 0 || args[0] == firstStepNumber;
            
            if (isFirstStep)
            {
                Logger.Log("Number of step: 1", Logger.LogLevelEnum.Debug);                
                var countsOfStrings = int.Parse(Console.ReadLine());
                for (var i = 0; i < countsOfStrings; ++i)
                {
                    root.AddString(Console.ReadLine(), i);
                }
            }
            else
            {
                Logger.Log("Number of step: 2", Logger.LogLevelEnum.Debug);
                root.AddString(Console.ReadLine(), 0);
                Node.Joker = Console.ReadLine()[0];
                Node.JokerException = Console.ReadLine()[0];
            }
            var result = root.FindPatternsInText(text).ToArray();
            if (!result.Any())
            {
                Logger.Log("Not found any occurrence of pattern{s} in text!");
            }
            
            foreach (var occurrence in result
                .OrderBy(x => x.CharPosition)
                .ThenBy(x => x.PatternNumber))
            {
                var resultString = !isFirstStep
                    ? $"{occurrence.CharPosition}"
                    : $"{occurrence.CharPosition} {occurrence.PatternNumber}";
                
                Logger.Log(resultString);
            }
        }
    }
}