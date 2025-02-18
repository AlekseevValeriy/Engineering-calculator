﻿using System.Linq.Expressions;

namespace EngineeringCalculator
{
    internal class InputController
    {
        public delegate void ChangeHandler();
        public event ChangeHandler Update;

        public void Add(ref List<Composite> expression, Term number)
        {
            List<Composite> actualExpression = GetActualExpression(ref expression);

            if (actualExpression.Count == 0) actualExpression.Add(number);
            else 
            { 
                switch (actualExpression.Last())
                {
                    case Term lnumber:
                        {
                            if (lnumber.Record == "0") actualExpression[actualExpression.Count - 1] = number;
                            else actualExpression[actualExpression.Count - 1].Set(lnumber.Record + number.Record);
                            break;
                        }
                    case Operator: { actualExpression.Add(number); break; }
                }
            }
            Update.Invoke();
        }

        public void Add(ref List<Composite> expression, Comma comma)
        {
            List<Composite> actualExpression = GetActualExpression(ref expression);

            if (actualExpression.Count != 0 && actualExpression.Last() is Term term && !term.HasDouble)
            {
                term.ToDouble();
                Update.Invoke();
            }
        }

        public void Add(ref List<Composite> expression, Constanta constanta)
        {
            List<Composite> actualExpression = GetActualExpression(ref expression);

            if (actualExpression.Count == 0) actualExpression.Add(constanta);
            else
            {
                switch (actualExpression.Last())
                {
                    case Term: { actualExpression[actualExpression.Count - 1] = constanta; break; }
                    case Operator: { actualExpression.Add(constanta); break; }
                }
            }
            Update.Invoke();
        }

        public void Add(ref List<Composite> expression, Operator operation)
        {
            List<Composite> actualExpression = GetActualExpression(ref expression);

            if (actualExpression.Count != 0)
            {
                switch (actualExpression.Last())
                {
                    case Operator: { actualExpression[actualExpression.Count - 1] = operation; break; }
                    case Term or Function or Staples: { actualExpression.Add(operation); break; } 
                }
                Update.Invoke();
            }
        }

        public void Add(ref List<Composite> expression, IExpressionStoreable storable)
        {
            List<Composite> actualExpression = GetActualExpression(ref expression);

            if (actualExpression.Count == 0 || actualExpression.Last() is Operator) { actualExpression.Add((Composite)storable); Update.Invoke(); }
        }

        public void ChangeSign(ref List<Composite> expression)
        {
            List<Composite> actualExpression = GetActualExpression(ref expression);

            if (actualExpression.Count != 0 && actualExpression.Last() is Term term) { term.ChangeSign(); Update.Invoke(); }
        }

        private List<Composite> GetActualExpression(ref List<Composite> expression)
        {
            return expression.Count != 0 && expression.Last() is IExpressionStoreable expressionStoreable ?
                expressionStoreable.GetActualExpression(ref expression) :
                expression;
        }

        private List<Composite> GetNotEmptyExpression(ref List<Composite> expression)
        {
            if( expression.Last() is IExpressionStoreable expressionStoreable )
            {
                return expressionStoreable.GetActualNotEmptyExpression(ref expression);
            }
            return expression;
        }

        public void CloseExpressionWrite(ref List<Composite> expression)
        {
            if (expression.Count == 0) return;

            if (expression.Last() is IExpressionStoreable compositeStoreable)
            {
                IExpressionStoreable actualComposite = compositeStoreable.GetActualComposite();
                List<Composite>? currentExpression = actualComposite.GetCurrentExpression();
                if (currentExpression is not null && currentExpression.Count != 0 && currentExpression.Last() is not Operator) actualComposite.CloseWrite();
            }
        }

        public void Equally(ref List<Composite> expression)
        {
            if (expression.Count > 1 && expression[expression.Count - 2] is Operator)
            {
                expression = new List<Composite>()
                {
                    Calculate.SolutionEquation(ref expression)
                };
                Update.Invoke();
            }
        }

        public void DeleteLast(ref List<Composite> expression) // TODO надо переделать под рекурсию для storable
        {
            if (expression.Count == 0) return;

            List<Composite> actualExpression = GetNotEmptyExpression(ref expression);

            switch (actualExpression.Last())
            {
                case Term number:
                    {
                        if (actualExpression[actualExpression.Count - 1].Record.Length == 1) actualExpression.RemoveAt(actualExpression.Count - 1);
                        else actualExpression[actualExpression.Count - 1].Set(number.Record.Remove(number.Record.Length - 1));
                        break;
                    }
                default:
                    {
                        actualExpression.RemoveAt(actualExpression.Count - 1);
                        break;
                    }
            }
            Update.Invoke();
        }

        public void ClearAll(ref List<Composite> expression)
        {
            if (expression.Count != 0) { expression.Clear(); Update.Invoke(); }
        }
        public void ClearOne(ref List<Composite> expression) // TODO надо переделать под рекурсию для storable
        {
            if (expression.Count == 0) return;

            List<Composite> actualExpression = GetNotEmptyExpression(ref expression);

            actualExpression.RemoveAt(actualExpression.Count - 1);

            Update.Invoke();
        }
    }
}