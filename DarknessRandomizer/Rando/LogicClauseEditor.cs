﻿using DarknessRandomizer.Data;
using DarknessRandomizer.Lib;
using RandomizerCore.Logic;
using RandomizerCore.StringLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DarknessRandomizer.Rando
{
    using SceneTree = BooleanTree<SceneOrLantern, SceneTreeMetadata>;

    // Can't use alias in another alias?
    using LogicTree = BooleanTree<ParsedToken, BooleanTree<SceneOrLantern, SceneTreeMetadata>>;

    using DarknessLogicAdder = Action<List<LogicToken>>;

    // Infers if a particular logic token is explicitly related to a particular SceneName.
    // If so, the darkness level of that scene name is assumed relevant to logic.
    public delegate bool SceneNameInferrer(string term, out SceneName sceneName);

    internal enum Lantern { Instance }

    internal class SceneOrLantern : Variant<SceneName, Lantern>
    {
        public SceneOrLantern(SceneName sceneName) : base(sceneName) { }

        private SceneOrLantern(Lantern lantern) : base(lantern) { }

        public static readonly SceneOrLantern Lantern = new(Rando.Lantern.Instance);

        public bool IsScene => HasFirst;

        public SceneName SceneName => First;

        public bool IsLantern => HasSecond;
    }

    internal class SceneTreeMetadata
    {
        public bool GuaranteedLantern = false;
    }

    public static class LogicClauseEditor
    {
        private static bool GuaranteedLantern(this SceneTree tree) => tree.Metadata != null && tree.Metadata.GuaranteedLantern;

        public static void EditDarkness(LogicManagerBuilder lmb, string name, SimpleToken lanternToken, SceneNameInferrer sni, DarknessLogicAdder dla)
        {
            var lc = lmb.LogicLookup[name];

            // Parse to tree of +|
            Stack<LogicTree> stack = new();
            foreach (var lt in lc)
            {
                if (lt is OperatorToken ot)
                {
                    var arg2 = stack.Pop();
                    var arg1 = stack.Pop();
                    stack.Push(LogicTree.CreateTree(ot.OperatorType, new List<LogicTree> { arg1, arg2 }));
                }
                else
                {
                    stack.Push(LogicTree.CreateLeaf(ParsedToken.Parse(lt, name, sni)));
                }
            }

            // Get the top node.
            var ln = stack.Pop();
            if (stack.Count != 0)
            {
                throw new ArgumentException($"Bad logic expression: {lc.ToInfix()}");
            }

            CalculateSceneTrees(ln);

            // At this point we can create a new LogicClause.
            List<LogicToken> tokens = new();
            bool lanternSubstituted = false;
            ExpandModifiedLogicTree(name, ln, lanternToken, dla, null, ref lanternSubstituted, tokens);
            LogicClauseBuilder lcb = new(tokens);
            lmb.LogicLookup[name] = new(lcb);
        }

        private static void CalculateSceneTrees(LogicTree tree)
        {
            if (tree.IsTree)
            {
                foreach (var t in tree.Children)
                {
                    CalculateSceneTrees(t);
                }

                // Child SceneTrees are now calculated, so we can calculate ours.
                tree.Metadata = Combine(tree.Op, tree.Children.Select(t => t.Metadata));
            }
            else if (tree.Value.IsLantern)
            {
                tree.Metadata = LanternSceneTree();
            }
            else if (tree.Value.IsScene)
            {
                tree.Metadata = SceneTree.CreateLeaf(new(tree.Value.SceneName));
            }
        }

        private static SceneTree LanternSceneTree()
        {
            var tree = SceneTree.CreateLeaf(SceneOrLantern.Lantern);
            tree.Metadata = new() { GuaranteedLantern = true };
            return tree;
        }

        private static SceneTree Combine(OperatorType op, IEnumerable<SceneTree> trees)
        {
            var treeList = trees.Where(t => t != null).ToList();
            if (treeList.Count == 0)
            {
                return null;
            }

            bool guaranteedLantern;
            if (op == OperatorType.AND)
            {
                if (treeList.All(t => t.IsLeaf && t.Value.IsLantern))
                {
                    return LanternSceneTree();
                }
                guaranteedLantern = treeList.All(t => t.GuaranteedLantern());
            }
            else
            {
                if (treeList.Any(t => t.IsLeaf && t.Value.IsLantern))
                {
                    return LanternSceneTree();
                }
                guaranteedLantern = treeList.Any(t => t.GuaranteedLantern());
            }

            // Deduplicate identical SceneNames at the same level.
            List<SceneTree> subtrees = new();
            Dictionary<SceneName, SceneTree> uniqueAtoms = new();
            foreach (var t in treeList)
            {
                if (t.IsTree)
                {
                    subtrees.Add(t);
                }
                else if (t.Value.IsScene)
                {
                    uniqueAtoms[t.Value.SceneName] = t;
                }
            }

            List<SceneTree> list = new();
            subtrees.ForEach(t => list.Add(t));
            uniqueAtoms.Values.ToList().ForEach(t => list.Add(t));

            if (list.Count == 0)
            {
                return guaranteedLantern ? LanternSceneTree() : null;
            }
            else if (list.Count == 1)
            {
                return list[0];
            }
            else
            {
                var tree = SceneTree.CreateTree(op, list);
                tree.Metadata = new() { GuaranteedLantern = guaranteedLantern };
                return tree;
            }
        }

        private static OperatorToken GetOperatorToken(OperatorType type)
        {
            return type == OperatorType.OR ? OperatorToken.OR : OperatorToken.AND;
        }

        private static void ExpandModifiedLogicTree(
            string logicName, LogicTree tree, SimpleToken lanternToken, DarknessLogicAdder dla, SceneTree sceneTree, ref bool lanternSubstituted, List<LogicToken> sink)
        {
            // The authoritative SceneTree for this branch is the one from the top-most conjunction.
            // These comprise the scenes necessary for this Lantern to be unnecessary.
            if (tree.IsLeaf || tree.Op == OperatorType.AND)
            {
                sceneTree ??= tree.Metadata;
            }

            if (tree.IsLeaf)
            {
                if (tree.Value.IsLantern)
                {
                    if (!sceneTree.GuaranteedLantern() || !lanternSubstituted)
                    {
                        sink.Add(lanternToken);
                        if (ListLanternTreeTokens(sceneTree, sink))
                        {
                            sink.Add(OperatorToken.OR);
                        }
                        lanternSubstituted = true;
                    }
                    else
                    {
                        sink.Add(ConstToken.True);
                    }
                }
                else if (tree.Value.IsScene && (sceneTree == null || !sceneTree.GuaranteedLantern()))
                {
                    sink.Add(lanternToken);
                    AddDarknessCheck(tree.Value.SceneName, sink);
                    sink.Add(OperatorToken.OR);
                    dla.Invoke(sink);
                    sink.Add(OperatorToken.OR);
                    sink.Add(tree.Value.Token);
                    sink.Add(OperatorToken.AND);
                }
                else
                {
                    sink.Add(tree.Value.Token);

                    // We force damageboosts to require light, since they may rely on hazard respawn triggers.
                    if (tree.Value.Token is SimpleToken st && st.Write() == "DAMAGEBOOSTS")
                    {
                        // If we have NOLANTERN, HRTs are never re-enabled, so the skip may be impossible.
                        sink.Add(lanternToken.Write() == "NOLANTERN" ? ConstToken.False : lanternToken);
                        if (sceneTree != null && ListLanternTreeTokens(sceneTree, sink))
                        {
                            sink.Add(OperatorToken.OR);
                        }
                        sink.Add(OperatorToken.AND);
                    }
                }
            }
            else
            {
                bool internalLanternSubstited = lanternSubstituted;
                for (int i = 0; i < tree.Children.Count; i++)
                {
                    ExpandModifiedLogicTree(logicName, tree.Children[i], lanternToken, dla, sceneTree, ref internalLanternSubstited, sink);
                    if (i > 0)
                    {
                        sink.Add(GetOperatorToken(tree.Op));
                    }
                }
            }
        }

        private static bool ListLanternTreeTokens(SceneTree tree, List<LogicToken> sink)
        {
            if (tree == null || (tree.IsLeaf && tree.Value.IsLantern))
            {
                return false;
            }

            if (tree.IsLeaf)
            {
                AddDarknessCheck(tree.Value.SceneName, sink);
            }
            else
            {
                int stacked = 0;
                foreach (var t in tree.Children)
                {
                    if (ListLanternTreeTokens(t, sink) && ++stacked > 1)
                    {
                        sink.Add(GetOperatorToken(tree.Op));
                    }
                }
            }
            return true;
        }

        private static void AddDarknessCheck(SceneName sceneName, List<LogicToken> sink)
        {
            sink.Add(new ComparisonToken(ComparisonType.LT, $"$DarknessLevel[{sceneName}]", "2"));
        }
    }

    class ParsedToken
    {
        enum TokenType
        {
            Scene,
            Lantern,
            Irrelevant
        }

        public LogicToken Token { get; private set; }
        private readonly TokenType type;
        private readonly SceneName sceneName;

        private ParsedToken(LogicToken token, TokenType type, SceneName sceneName)
        {
            this.Token = token;
            this.type = type;
            this.sceneName = sceneName;
        }

        public static readonly ParsedToken Lantern = new(null, TokenType.Lantern, null);

        public static ParsedToken Parse(LogicToken token, string logicName, SceneNameInferrer sni)
        {
            if (token is SimpleToken st)
            {
                string name = st.Write();
                if (name == "LANTERN" || name == "NOLANTERN")
                {
                    return Lantern;
                }

                if (name == logicName)
                {
                    // Do not infer scenes for self-matching transition names.
                    return new(token, TokenType.Irrelevant, null);
                }
                if (sni.Invoke(name, out SceneName sceneName))
                {
                    return new(token, TokenType.Scene, sceneName);
                }
            }

            return new(token, TokenType.Irrelevant, null);
        }

        public bool IsScene => type == TokenType.Scene;

        public SceneName SceneName
        {
            get
            {
                if (!IsScene)
                {
                    throw new ArgumentException("Illegal access");
                }
                return sceneName;
            }
        }

        public bool IsLantern => type == TokenType.Lantern;

        public bool IsRelevant => type != TokenType.Irrelevant;
    }
}
