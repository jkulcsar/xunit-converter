﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.DeadRegionAnalysis
{
    internal class PreprocessorExpressionEvaluator : CSharpSyntaxVisitor<Tristate>
    {
        private IReadOnlyDictionary<string, Tristate> m_symbolStates;
        private Tristate m_undefinedSymbolValue;

        public PreprocessorExpressionEvaluator(IReadOnlyDictionary<string, Tristate> symbolStates, Tristate undefinedSymbolValue)
        {
            Debug.Assert(symbolStates != null);
            m_symbolStates = symbolStates;
            m_undefinedSymbolValue = undefinedSymbolValue;
        }

        public override Tristate VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            Tristate state;
            if (m_symbolStates.TryGetValue(node.ToString(), out state))
            {
                return state;
            }

            switch (node.CSharpKind())
            {
                case SyntaxKind.TrueLiteralExpression:
                    return Tristate.True;
                case SyntaxKind.FalseLiteralExpression:
                    return Tristate.False;
                default:
                    throw new InvalidPreprocessorExpressionException("Expected true or false literal expression");
            }
        }

        public override Tristate VisitIdentifierName(IdentifierNameSyntax node)
        {
            Tristate state;
            if (m_symbolStates.TryGetValue(node.ToString(), out state))
            {
                return state;
            }
            else
            {
                return m_undefinedSymbolValue;
            }
        }

        public override Tristate VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            if (node.CSharpKind() != SyntaxKind.LogicalNotExpression)
            {
                throw new InvalidPreprocessorExpressionException("Expected logical not expression");
            }

            return !node.Operand.Accept(this);
        }

        public override Tristate VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            Tristate left = node.Left.Accept(this);
            Tristate right = node.Right.Accept(this);

            switch (node.CSharpKind())
            {
                case SyntaxKind.LogicalAndExpression:
                    return left & right;
                case SyntaxKind.LogicalOrExpression:
                    return left | right;
                default:
                    throw new InvalidPreprocessorExpressionException("Expected logical and/or expression");
            }
        }

        public override Tristate VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            return node.Expression.Accept(this);
        }
    }

    internal class InvalidPreprocessorExpressionException : Exception
    {
        public InvalidPreprocessorExpressionException(string message) : base(message)
        {
        }
    }

    internal class CompositePreprocessorExpressionEvaluator
    {
        private IEnumerable<PreprocessorExpressionEvaluator> m_expressionEvaluators;

        public CompositePreprocessorExpressionEvaluator(IEnumerable<PreprocessorExpressionEvaluator> expressionEvaluators)
        {
            if (expressionEvaluators == null)
            {
                throw new ArgumentNullException("expressionEvaluators");
            }

            m_expressionEvaluators = expressionEvaluators;
        }


        public Tristate EvaluateExpression(CSharpSyntaxNode expression)
        {
            var it = m_expressionEvaluators.GetEnumerator();
            if (!it.MoveNext())
            {
                Debug.Assert(false, "We should have at least one expression evaluator");
            }

            Tristate result = expression.Accept(it.Current);
            if (result == Tristate.Varying)
            {
                return Tristate.Varying;
            }

            while (it.MoveNext())
            {
                if (expression.Accept(it.Current) != result)
                {
                    return Tristate.Varying;
                }
            }

            return result;
        }
    }
}
