﻿namespace FFEC
{
    internal static class InputController
    {
        public static event ChangeHandler Update;
        public delegate void ChangeHandler();

        public static void Append(List<Composite> expression, Term number)
        {
            List<Composite> actualExpression = GetActualExpression(expression);

            if (actualExpression.Count == 0)
            {
                actualExpression.Add(number);
            }
            else
            {
                switch (actualExpression.Last())
                {
                    case Term lnumber:
                        {
                            if (lnumber.Record == "0")
                            {
                                actualExpression[actualExpression.Count - 1] = number;
                            }
                            else
                            {
                                actualExpression[actualExpression.Count - 1].Set(lnumber.Record + number.Record);
                            }

                            break;
                        }
                    case Operator: { actualExpression.Add(number); break; }
                }
            }
            Update.Invoke();
        }

        public static void Append(List<Composite> expression, Comma comma)
        {
            List<Composite> actualExpression = GetActualExpression(expression);

            if (actualExpression.Count != 0 && actualExpression.Last() is Term term && !term.HasDouble)
            {
                term.ToDouble();
                Update.Invoke();
            }
        }

        public static void Append(List<Composite> expression, Constanta constanta)
        {
            List<Composite> actualExpression = GetActualExpression(expression);

            if (actualExpression.Count == 0)
            {
                actualExpression.Add(constanta);
            }
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

        private static void AppendConstantaLike(List<Composite> expression, Composite composite)
        {
            List<Composite> actualExpression = GetActualExpression(expression);

            if (actualExpression.Count == 0)
            {
                actualExpression.Add(composite);
            }
            else
            {
                switch (actualExpression.Last())
                {
                    case Term: { actualExpression[actualExpression.Count - 1] = composite; break; }
                    case Operator: { actualExpression.Add(composite); break; }
                }
            }
            Update.Invoke();
        }

        public static void Append(List<Composite> expression, Operator operation)
        {
            List<Composite> actualExpression = GetActualExpression(expression);

            if (actualExpression.Count != 0)
            {
                switch (actualExpression.Last())
                {
                    case Operator: { actualExpression[actualExpression.Count - 1] = operation; break; }
                    case Variable or Term or Function or Staples: { actualExpression.Add(operation); break; }
                }
                Update.Invoke();
            }
        }

        public static void Append(List<Composite> expression, IExpressionStoreable storable)
        {
            List<Composite> actualExpression = GetActualExpression(expression);

            if (actualExpression.Count == 0 || actualExpression.Last() is Operator) { actualExpression.Add((Composite)storable); Update.Invoke(); }
        }

        public static void Append(List<Composite> expression, Variable variable)
        {
            AppendConstantaLike(expression, variable);
        }

        public static void Append(List<Composite> expression, CustomFunction function)
        {
            List<Composite> actualExpression = GetActualExpression(expression);

            if (actualExpression.Count == 0 || actualExpression.Last() is Operator)
            {
                actualExpression.AddRange(function.GetExpression);
            }
            Update.Invoke();
            ValidateVariableInExpression(expression);
        }

        public static void ChangeSign(List<Composite> expression)
        {
            List<Composite> actualExpression = GetActualExpression(expression);

            if (actualExpression.Count != 0 && actualExpression.Last() is Term term) { term.ChangeSign(); Update.Invoke(); }
        }

        private static List<Composite> GetActualExpression(List<Composite> expression)
        {
            return expression.Count != 0 && expression.Last() is IExpressionStoreable expressionStoreable ?
                expressionStoreable.GetActualExpression(expression) :
                expression;
        }

        private static List<Composite> GetNotEmptyExpression(List<Composite> expression)
        {
            return expression.Last() is IExpressionStoreable expressionStoreable
                ? expressionStoreable.GetActualNotEmptyExpression(expression)
                : expression;
        }

        public static void CloseExpressionWrite(List<Composite> expression)
        {
            if (expression.Count == 0)
            {
                return;
            }

            if (expression.Last() is IExpressionStoreable compositeStoreable)
            {
                IExpressionStoreable actualComposite = compositeStoreable.GetActualComposite();
                List<Composite>? currentExpression = actualComposite.GetCurrentExpression();
                if (currentExpression is not null && currentExpression.Count != 0 && currentExpression.Last() is not Operator)
                {
                    actualComposite.CloseWrite();
                }
            }
        }

        public static void Equally(ref List<Composite> expression)
        {
            if ((expression.Count > 1 && expression[expression.Count - 2] is Operator) | (expression.Count == 1 && expression.Last() is IExpressionStoreable))
            {
                expression =
                [
                    Calculate.SolutionEquation(expression)
                ];
            }
            Update.Invoke();
        }

        public static void DeleteLast(List<Composite> expression)
        {
            if (expression.Count == 0)
            {
                return;
            }

            List<Composite> actualExpression = GetNotEmptyExpression(expression);

            switch (actualExpression.Last())
            {
                case Term number:
                    {
                        if (actualExpression[actualExpression.Count - 1].Record.Length == 1)
                        {
                            actualExpression.RemoveAt(actualExpression.Count - 1);
                        }
                        else
                        {
                            actualExpression[actualExpression.Count - 1].Set(number.Record.Remove(number.Record.Length - 1));
                        }

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

        public static void ClearAll(List<Composite> expression)
        {
            if (expression.Count != 0) { expression.Clear(); Update.Invoke(); }
        }
        public static void ClearOne(List<Composite> expression)
        {
            if (expression.Count == 0)
            {
                return;
            }

            List<Composite> actualExpression = GetNotEmptyExpression(expression);

            actualExpression.RemoveAt(actualExpression.Count - 1);

            Update.Invoke();
        }

        public static void UpdateVariableInExpression(List<Composite> expression)
        {
            if (expression.Count == 0)
            {
                return;
            }

            List<string> variableNames = Config.GetVariableNames();
            //List<string> missing

            foreach (Composite composite in expression)
            {
                if (composite is Variable variable && !variableNames.Contains(variable.Record))
                {
                    expression.Clear();
                    Update.Invoke();
                    break;
                }

            }
        }

        public static void ValidateVariableInExpression(List<Composite> expression)
        {
            if (expression.Count == 0)
            {
                return;
            }

            List<string> actualVariableNames = Config.GetVariableNames();

            List<string> expressionVariableNames = new List<string>();

            List<string> missingVariableNames = new List<string>();

            foreach (Composite composite in expression)
            {
                if (composite is Variable variable) expressionVariableNames.Add(variable.Record);
                if (composite is IExpressionStoreable storable) expressionVariableNames.AddRange(storable.ContainsVariable());
            }

            foreach(string variableName in expressionVariableNames.Distinct())
            {
                if (!actualVariableNames.Contains(variableName)) missingVariableNames.Add(variableName);
            }

            if (missingVariableNames.Count != 0)
            {
                Messages.RaiseInformationMessage($"""
                    Отсутствуют необходимые переменные:
                    {string.Join(",\n", missingVariableNames)}.
                    Пожалуйста, добавьте переменные и повторите попытку.
                    """);
                expression.Clear();
                Update.Invoke();
            }
        }


        public static void UpdateDisplay()
        {
            Update.Invoke();
        }
    }
}