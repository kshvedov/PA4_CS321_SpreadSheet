/*
 * Name: Konstantin Shvedov
 * Date: 19/10/2018
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetEngine;

namespace ExpressionTreeMenu
{
    public class ExpressionTreeMenu
    {
        static void Main(string[] args)
        {
            bool endVar = false;
            string menuOptionS = "";
            string expression = "A1-12-C1";
            string varName = "";
            string tempVal = "";
            double varVal = 0.0;
            int menuOptionV = 0;
            ExpTree newExpTree = new ExpTree(expression);
            do //a do while loop tha continues going through the menu and different functions till quit
            {
                Console.WriteLine(">>>>>>>>>> Menu <<<<<<<<<<");
                Console.WriteLine("(Shunting Yard expression = \"" + newExpTree.shuntingYardAlgo(expression) + "\")");
                Console.WriteLine("(Current expression = \"" + expression + "\")");
                Console.WriteLine("\t 1 = Enter a new expression");
                Console.WriteLine("\t 2 = Set a variable value");
                Console.WriteLine("\t 3 = Evaluate tree");
                Console.WriteLine("\t 4 = Quit");
                menuOptionS = Console.ReadLine();
                if (!int.TryParse(menuOptionS, out menuOptionV))
                {
                    Console.WriteLine("Error: Only ints for menu selection\n");
                }
                else
                {
                    menuOptionV = Convert.ToInt32(menuOptionS);
                    switch (menuOptionV)
                    {
                        case 1:
                            Console.Write("Enter new expression: ");
                            expression = Console.ReadLine();
                            Console.WriteLine();
                            newExpTree = new ExpTree(expression);
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("New Expression Has Been Chosen");
                            Console.ResetColor();
                            break;
                        case 2:
                            Console.Write("Enter variable name: ");
                            varName = Console.ReadLine();
                            Console.WriteLine();
                            Console.Write("Enter variable value: ");
                            tempVal = Console.ReadLine();
                            varVal = Convert.ToDouble(tempVal);
                            newExpTree.SetVar(varName, varVal);
                            Console.WriteLine();
                            break;
                        case 3:
                            Console.WriteLine(newExpTree.Eval());
                            break;
                        case 4:
                            endVar = true;
                            break;
                    }
                }
            } while (!endVar);
            Console.WriteLine("Done");
        }
    }
}
