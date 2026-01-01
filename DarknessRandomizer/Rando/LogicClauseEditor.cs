using DarknessRandomizer.Data;
using RandomizerCore.Logic;
using RandomizerCore.StringLogic;
using RandomizerCore.StringParsing;
using System.Collections.Generic;
using System.Linq;

namespace DarknessRandomizer.Rando;

// Infers if a particular logic token is explicitly related to a particular SceneName.
// If so, the darkness level of that scene name is assumed relevant to logic.
public delegate bool SceneNameInferrer(string term, out SceneName sceneName);

public static class LogicClauseEditor
{
    private static IReadOnlyCollection<SceneName> GetRequiredScenes(
        Dictionary<Expression<LogicExpressionType>, IReadOnlyCollection<SceneName>> cache,
        string self,
        SceneNameInferrer sni,
        Expression<LogicExpressionType> expr)
    {
        if (cache.TryGetValue(expr, out var cached)) return cached;

        IReadOnlyCollection<SceneName> ret = [];
        switch (expr)
        {
            case LogicAtomExpression atom:
                var atomStr = atom.Token.Print();
                if (atomStr != self && sni(atomStr, out SceneName sceneName)) ret = [sceneName];
                break;
            case PostfixExpression<LogicExpressionType> postfixExpr:
                ret = GetRequiredScenes(cache, self, sni, postfixExpr.Operand);
                break;
            case PrefixExpression<LogicExpressionType> prefixExpr:
                ret = GetRequiredScenes(cache, self, sni, prefixExpr.Operand);
                break;
            case AndExpression andExpr:
                ret = new HashSet<SceneName>([.. GetRequiredScenes(cache, self, sni, andExpr.Left).Concat(GetRequiredScenes(cache, self, sni, andExpr.Right))]);
                break;
            case OrExpression orExpr:
                ret = [.. GetRequiredScenes(cache, self, sni, orExpr.Left).Intersect(GetRequiredScenes(cache, self, sni, orExpr.Right))];
                break;
            case CoalesceExpression coalesceExpr:
                ret = [.. GetRequiredScenes(cache, self, sni, coalesceExpr.Left).Intersect(GetRequiredScenes(cache, self, sni, coalesceExpr.Right))];
                break;
        }

        cache[expr] = ret;
        return ret;
    }

    private static Expression<LogicExpressionType> IsRoomLight(SceneName sceneName, ExpressionBuilder<LogicExpressionType> builder) => builder.ApplyInfixOperator(builder.NameAtom($"$DarknessLevel[{sceneName}]"), builder.Op("<"), builder.NumberAtom(2));

    private static bool IsNamedToken(string name, Expression<LogicExpressionType> expr) => expr is AtomExpression<LogicExpressionType> atom && atom.Token is NameToken named && named.Content == name;

    private static bool IsLanternExpression(Token lanternToken, Expression<LogicExpressionType> expr)
    {
        if (IsNamedToken(lanternToken.Content, expr)) return true;
        if (expr is OrExpression or
            && ((IsNamedToken(lanternToken.Content, or.Left) && IsNamedToken("DARKROOMS", or.Right))
                || (IsNamedToken(lanternToken.Content, or.Right) && IsNamedToken("DARKROOMS", or.Left))))
            return true;

        return false;
    }

    private static Expression<LogicExpressionType>? ApplyDarknessConstraints(
        IReadOnlyCollection<SceneName> scoped,
        Dictionary<Expression<LogicExpressionType>, IReadOnlyCollection<SceneName>> cache,
        string self,
        SceneNameInferrer sni,
        Token lanternToken,
        Expression<LogicExpressionType> darkroomsLogic,
        Expression<LogicExpressionType> expr,
        ExpressionBuilder<LogicExpressionType> builder)
    {
        // We force damageboosts to require light, since they may rely on hazard respawn triggers.
        if (scoped.Count > 0 && IsNamedToken("DAMAGEBOOSTS", expr))
        {
            // If we have NOLANTERN, HRTs are never re-enabled, so the skip may be impossible.
            if (lanternToken.Content == "NOLANTERN") return builder.NameAtom("FALSE");

            return builder.ApplyInfixOperator(
                expr,
                builder.Op("+"),
                builder.ApplyInfixOperatorLeftAssoc(
                    scoped.Select(s => IsRoomLight(s, builder)).Concat([builder.NameAtom(lanternToken.Print())]),
                    builder.Op("|")));
        }

        HashSet<SceneName> requiredScenes = [.. GetRequiredScenes(cache, self, sni, expr)];
        requiredScenes.RemoveWhere(scoped.Contains);

        if (requiredScenes.Count == 0)
        {
            // Ignore pre-existing LANTERN and DARKROOMS clauses.
            if (expr is AndExpression and)
            {
                if (IsLanternExpression(lanternToken, and.Left)) return and.Right;
                if (IsLanternExpression(lanternToken, and.Right)) return and.Left;
                return null;
            }
            if (expr is OrExpression or && (IsLanternExpression(lanternToken, or.Left) || IsLanternExpression(lanternToken, or.Right)))
                return builder.NameAtom("TRUE");
            if (IsLanternExpression(lanternToken, expr))
                return builder.NameAtom("TRUE");

            return null;
        }

        var constraint = builder.ApplyInfixOperatorLeftAssoc(requiredScenes.Select(s => IsRoomLight(s, builder)).Concat([builder.NameAtom(lanternToken.Print()), darkroomsLogic]), builder.Op("|"));
        return builder.ApplyInfixOperator(constraint, builder.Op("+"), expr.Transform(
            (e, b) => ApplyDarknessConstraints(requiredScenes, cache, self, sni, lanternToken, darkroomsLogic, e, b),
            builder));
    }

    public static void EditDarkness(LogicManagerBuilder lmb, string name, SceneNameInferrer sni, Token lanternToken, Expression<LogicExpressionType> darkroomsLogic)
    {
        Dictionary<Expression<LogicExpressionType>, IReadOnlyCollection<SceneName>> cache = [];
        LogicExpressionBuilder builder = new();

        var expr = lmb.LogicLookup[name].Expr;
        lmb.LogicLookup[name] = new(expr.Transform(
            (e, b) => ApplyDarknessConstraints([], cache, name, sni, lanternToken, darkroomsLogic, e, b), new LogicExpressionBuilder()));
    }
}
