using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThesisProjectV1
{
    internal class CmdHandler
    {
        #region Fields
        private List<string> options;

        private int selectedIndex = 0;
        #endregion

        #region Constructors
        public CmdHandler()
        {
            options = new List<string>()
            {
                "Controller","Routine","Program","AddOnInstructionDefinition","DataType","Tag","Module", "AlarmCondition", "Task", "Trend"
            };
        }
        public CmdHandler(List<string> ops) { options = ops; }

        public void DisplayMenu()
        {
            Console.Clear();
            for (int i = 0; i < options.Count; i++)
            {
                if(i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(">"+options[i]);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(" "+ options[i]);
                }
            }
            Console.ResetColor();
        }

        public string Navigate()
        {
            ConsoleKeyInfo key;
            String selected ="";
            do
            {
                DisplayMenu();
                key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if(selectedIndex>0)
                            selectedIndex--;
                        break;
                    case ConsoleKey.DownArrow:
                        if(selectedIndex<options.Count-1)
                            selectedIndex++;
                        break;
                    case ConsoleKey.Enter:
                        selected = options[selectedIndex];
                        break;
                }

            }while(key.Key != ConsoleKey.Escape && selected.Equals(""));
            return selected;
        }

        #endregion
    }
}
