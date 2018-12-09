/*
 * Name: Konstantin Shvedov
 * Date: 19/10/2018
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetEngine
{
    public class ExpTree
    {
        //abstarct node class off of which all other nodes are built
        private abstract class Node
        {
            public abstract double Eval();
        }

        //ExpTree variables
        private Node _mRoot;
        private static Dictionary<string, Cell> _mDict;

        //this node is one of the more complex ones which helps
        //to idetify the opperation that needs to be performed
        private class OperationNode : Node
        {
            private char _mOp;
            private Node _mLeft, _mRight;

            public OperationNode(char newmOp, Node newLeft, Node newRight)
            {
                _mOp = newmOp;
                _mLeft = newLeft;
                _mRight = newRight;
            }

            public override double Eval()
            {
                double expLeft = _mLeft.Eval();
                double expRight = _mRight.Eval();
                if (_mOp == '+')
                {
                    return expLeft + expRight;
                }
                else if (_mOp == '-')
                {
                    return expLeft - expRight;
                }
                else if (_mOp == '*')
                {
                    return expLeft * expRight;
                }
                else if (_mOp == '/')
                {
                    return (expLeft) / (expRight);
                }
                return 0;
            }
        }

        //Variable node contains a variable, if the variable
        //definition exists in the dictionary then value of it is returned
        //if not the key is created and 0.0 is returned
        private class VariableNode : Node
        {
            private string _mVar;

            public VariableNode(string newVar)
            {
                _mVar = newVar;
            }

            public override double Eval()
            {
                return Convert.ToDouble(_mDict[_mVar].Value);
            }
        }

        //a simple value node that contains a double value
        private class ValueNode : Node
        {
            private double _mVal;

            public ValueNode (double newVal)
            {
                _mVal = newVal;
            }

            public override double Eval()
            {
                return _mVal;
            }
        }

        //this is a makeNode node function that undertsnad if the node to be created
        //is a Value node or a variable node
        private Node MakeNodes(string expression)
        {
            double _mval;
            if (double.TryParse(expression, out _mval))
            {
                return new ValueNode(_mval);
            }
            return new VariableNode(expression);
        }

        //ExpTree constructor
        public ExpTree(string expression)
        {
            _mRoot = makeTree(expression);
            if (_mDict == null)
            {
                _mDict = new Dictionary<string, Cell>();
            }
        }


        //This is the shunting yard algorithem that parses the expression and creats rpn expression
        public string shuntingYardAlgo(string expression)
        {
            Stack<string> opStack = new Stack<string>();
            Queue<string> posQueue = new Queue<string>();
            int expLength = expression.Length;
            int iter = 0, prevOp = 0;
            string rpnExpression = "";

            opStack.Push(null); //helps identify bottom of stack

            for (; iter < expLength; iter++)//itterates through whole string
            {
                if (opChekHelper(expression[iter]))//checks to see if the current char is a operation symbol
                {
                    posQueue.Enqueue(expression.Substring(prevOp, (iter - prevOp)));
                    prevOp = iter + 1;
                }
                else if (iter == (expLength - 1))//checks if at last position of expression (push required of last element)
                {
                    posQueue.Enqueue(expression.Substring(prevOp, (iter - prevOp + 1)));
                }
                switch(expression[iter])//depending on what opeerator is pushed makes according adjustments to move from stack to queue
                {
                    case '+':
                        while ((opStack.Peek() == "*") || (opStack.Peek() == "/") || (opStack.Peek() == "-") ||(opStack.Peek() == "_"))
                        {
                            posQueue.Enqueue(opStack.Pop());
                        }
                        opStack.Push("+");
                        break;

                    case '-':
                        if (iter > 0 && !opChekHelper(expression[iter - 1]))
                        {
                            while ((opStack.Peek() == "*") || (opStack.Peek() == "/") || (opStack.Peek() == "+") || (opStack.Peek() == "_"))
                            {
                                posQueue.Enqueue(opStack.Pop());
                            }
                            opStack.Push("-");
                        }
                        else opStack.Push("_");
                        break;

                    case '*':
                        while (opStack.Peek() == "/" || (opStack.Peek() == "_"))
                        {
                            posQueue.Enqueue(opStack.Pop());
                        }
                        opStack.Push("*");
                        break;

                    case '/':
                        while (opStack.Peek() == "*" || (opStack.Peek() == "_"))
                        {
                            posQueue.Enqueue(opStack.Pop());
                        }
                        opStack.Push("/");
                        break;

                    case '(':
                        opStack.Push("(");
                        break;

                    case ')':
                        while (opStack.Peek() != "(" || (opStack.Peek() == "_"))
                        {
                            posQueue.Enqueue(opStack.Pop());
                        }
                        opStack.Pop();
                        break;
                }
            }
            while(opStack.Peek() != null)//pops the remaining operators from stack
            {
                posQueue.Enqueue(opStack.Pop());
            }
            posQueue.Enqueue(null);//helps identify last element of queue
            while (posQueue.Peek() != null)//turns queueinto a string
            {
                rpnExpression = rpnExpression + " " + posQueue.Dequeue();
            }
            rpnExpression = rpnExpression + " " + posQueue.Dequeue() + " ";
            return rpnExpression;
        }

        //function that cheks if item is an operator
        private bool opChekHelper(char cChar)
        {
            switch (cChar)
            {
                case '+':
                case '-':
                case '*':
                case '/':
                case '(':
                case ')':
                case '_':
                    return true;
            }
            return false;
        }

        //the remade make tree algorithem so a rpn string can be evaluated,
        //uses a stack of nodes and trees to hold items
        private Node makeTree(string expression)
        {
            expression = shuntingYardAlgo(expression);
            int expLength = expression.Length;
            Stack<Node> expStack = new Stack<Node>();
            expStack.Push(null);
            OperationNode tempTree = null;
            Node tempNodeR = null, tempNodeL = null;
            string tSubString = "";

            int iter = 1, prevSpace = 0;
            for (; iter < expLength; iter++)
            {
                if (expression[iter] == ' ')
                {
                    tSubString = expression.Substring(prevSpace + 1, (iter - prevSpace - 1));
                    prevSpace = iter;
                    if (tSubString.Length != 0)
                    {
                        switch (tSubString)
                        {
                            case "+":
                            case "-":
                            case "*":
                            case "/":
                                tempNodeR = expStack.Pop();
                                tempNodeL = expStack.Pop();
                                tempTree = new OperationNode(Convert.ToChar(tSubString), tempNodeL, tempNodeR);
                                expStack.Push(tempTree);
                                break;
                            case "_"://unary negation is handled differently, a node with a negative is made
                                tempNodeR = expStack.Pop();
                                tempTree = new OperationNode(Convert.ToChar("-"), MakeNodes("0"), tempNodeR);//the left node is 0 and the right node is the value
                                expStack.Push(tempTree);
                                break;
                            default:
                                expStack.Push(MakeNodes(tSubString));
                                break;
                        }
                    }
                }
            }
            return expStack.Pop();
        }

        //checks if avriable already exists, if not estantiates a new one
        public static void SetVar(string varName, Cell varValue)
        {
            if (!_mDict.ContainsKey(varName))
            {
                _mDict.Add(varName, varValue);
            }
            else _mDict[varName] = varValue;
        }

        public double Eval()
        {
            if (_mRoot != null)
            {
                return _mRoot.Eval();
            }
            else return double.NaN;
        }
    }
}
