using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BinaryCalc
{
    /// <summary>
    /// Binary Calculator
    /// 
    /// written by Bartosz Wasilewski
    /// bart.wasilewski@gmail.com
    /// 
    ///
    /// Interaction logic for MainWindow.xaml
    /// Also contains some minor logic related to IO-label behavior.
    /// [button name]_Click are event handlers for button clicks
    /// [button name]_KB are event handlers for keyboard presses
    ///     Note:   This featureset is incomplete. The implementation is suboptimal from an API-standpoint.
    ///             At present the keyboard button presses trigger the buttons' Click events. They should be
    ///             implemented by use of Commands as hotkeys.
    /// the remaining methods are for updating the IO-label and to add the keyboard listeners.
    /// </summary>
    public partial class MainWindow : Window
    {
        Operations op;
        public MainWindow()
        {
            InitializeComponent();
            op = new Operations();
            addKeyBinds();
        }

        private void addKeyBinds()
        {
            mainCanvas.KeyDown += new KeyEventHandler(clearEntry_KB);
            mainCanvas.KeyDown += new KeyEventHandler(clearAll_KB);
            mainCanvas.KeyDown += new KeyEventHandler(inputOne_KB);
            mainCanvas.KeyDown += new KeyEventHandler(inputZero_KB);
            mainCanvas.KeyDown += new KeyEventHandler(opAdd_KB);
            mainCanvas.KeyDown += new KeyEventHandler(opSub_KB);
            mainCanvas.KeyDown += new KeyEventHandler(opEquals_KB);
        }

        private void clearEntry_Click(object sender, RoutedEventArgs e)
        {
            updateIO("");
        }

        private void clearEntry_KB(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                clearEntry.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void clearAll_Click(object sender, RoutedEventArgs e)
        {
            updateIO("");
            op.clearMem();
        }

        private void clearAll_KB(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                clearAll.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void inputOne_Click(object sender, RoutedEventArgs e)
        {
            if (op.isFreshIO())
            {
                updateIO("");
                op.refreshedIO();
            }
            if (op.isOpDone())
            {
                updateIO("");
                op.clearMem();
                op.opReady();
            }
            updateIO(String.Concat(op.getIoContent(), "1"));
        }

        private void inputOne_KB(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.NumPad1 || e.Key == Key.D1)
            {
                inputOne.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void inputZero_Click(object sender, RoutedEventArgs e)
        {
            if (op.isFreshIO())
            {
                updateIO("");
                op.refreshedIO();
            }
            if (op.isOpDone())
            {
                updateIO("");
                op.clearMem();
                op.opReady();
            }
            updateIO(String.Concat(op.getIoContent(), "0"));
        }

        private void inputZero_KB(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.NumPad0 || e.Key == Key.D0)
            {
                inputZero.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void opAdd_Click(object sender, RoutedEventArgs e)
        {
            if (op.getIoContent().CompareTo("") == 0 || op.getIoContent().CompareTo("-") == 0)
            {
                MessageBox.Show("Please enter a number");
            }
            else
            {
                updateIO(op.calculate("+"));
            }
        }

        private void opAdd_KB(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                opAdd.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void opSub_Click(object sender, RoutedEventArgs e)
        {
            if(op.getIoContent().CompareTo("") == 0)
            {
                updateIO(String.Concat(op.getIoContent(), "-"));
            }
            else if(op.getIoContent().CompareTo("-") == 0)
            {
                MessageBox.Show("Please enter a number");
            }
            else
            {
                updateIO(op.calculate("-"));
            }
        }

        private void opSub_KB(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
            {
                opSub.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void opEquals_Click(object sender, RoutedEventArgs e)
        {
            updateIO(op.calculate("="));
        }

        private void opEquals_KB(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                opEquals.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void updateIO (string output)
        {
            op.setIoContent(output);
            ioContentTB.Text = output;
        }
    }

    public class Operations
    {
        /// <summary>
        /// This class contains logic for calculations and conversion between binary display and internal variables.
        ///     Note:   Error handling for overflow exceptions on input and calculation results may have bugs. If encountered,
        ///             an AC-operation will reset the calculator to a usable state.
        /// binaryInput() and binaryOutput() are conversion methods for binary->decimal and decimal->binary respectively.
        ///     Binary numbers are handled as strings while decimal numbers are handled as nullable longs internally.
        /// calculate() does the bulk of logic for determining operation order, defined as per specifications.
        /// addition() and subtraction() are simple add and subtract methods.
        /// The remaining methods are getters and setters for the IO-Label and its behavior.
        /// </summary>
        string ioContent = "";
        long? previousNumber1 = null;
        long? previousNumber2 = null;
        long? previousAnswer = null;
        string currentOperation = "";
        bool freshIO = false;
        bool opDone = false;

        public long? binaryInput(string input)
        {
            long convResult = 0;
            bool isNegative = false;
            double charValue;

            if (input[0] == '-')
            {
                isNegative = true;
                input = input.TrimStart(new char[] {'-'});
            }

            input = new string(input.ToCharArray().Reverse().ToArray());

            for (int i = 0; i < input.Length; i++)
            {
                charValue = Char.GetNumericValue(input[i]);
                convResult = checked(convResult + (long)(charValue * (Math.Pow(charValue * 2, (double)i))));
            }

            if (isNegative)
            {
                convResult = 0 - convResult;
            }

            return convResult;
        }

        public string calculate(string newOperation)
        {
            if (currentOperation.CompareTo("") == 0 && previousNumber1 == null)
            {
                if (newOperation.CompareTo("+") == 0 || newOperation.CompareTo("-") == 0)
                {
                    currentOperation = newOperation;
                    try
                    {
                        previousNumber1 = binaryInput(ioContent);
                    }
                    catch (OverflowException)
                    {
                        MessageBox.Show("Error: Number value of input exceeds computational range.\nPlease clear memory (AC) to proceed.");
                        clearMem();
                        return "";
                    }
                    freshIO = true;
                }
                else if (currentOperation.CompareTo("=") == 0)
                {
                    MessageBox.Show("To get an answer please follow these steps:\n1. Enter a number\n2. Select an operation\n3. Enter a number\n4. Select \"=\".");
                }
                else
                {
                    MessageBox.Show("Error:\nThe selected operation (" + newOperation + ") is unsupported and/or not implemented.\nPlease clear memory (AC) and try again.");
                    clearMem();
                    return "";
                }
            }
            else if (currentOperation.CompareTo("") != 0 && previousNumber1 == null)
            {
                MessageBox.Show("Error:\nThere is a saved operation (\"" + currentOperation + "\"),\nbut no number to use it on.\nPlease clear memory (AC) and try again.");
                clearMem();
                return "";
            }
            else
            {
                if (!opDone)
                {
                    try
                    {
                        previousNumber2 = binaryInput(ioContent);
                    }
                    catch (OverflowException)
                    {
                        MessageBox.Show("Error: Number value of input exceeds computational range.\nPlease clear memory (AC) and try again.");
                        clearMem();
                        return "";
                    }
                }
                if (newOperation.CompareTo("=") == 0)
                {
                    if (currentOperation.CompareTo("+") == 0)
                    {
                        previousAnswer = addition(previousNumber1, previousNumber2);
                        opDone = true;
                        previousNumber1 = previousAnswer;
                        return binaryOutput(previousAnswer);
                    }
                    else if (currentOperation.CompareTo("-") == 0)
                    {
                        previousAnswer = subtraction(previousNumber1, previousNumber2);
                        opDone = true;
                        previousNumber1 = previousAnswer;
                        return binaryOutput(previousAnswer);
                    }
                }
                if (newOperation.CompareTo("+") == 0)
                {
                    if (opDone)
                    {
                        currentOperation = newOperation;
                        opDone = false;
                        freshIO = true;
                    }
                    else if (currentOperation.CompareTo("+") == 0)
                    {
                        previousAnswer = addition(previousNumber1, previousNumber2);
                        freshIO = true;
                        previousNumber1 = previousAnswer;
                        currentOperation = "+";
                        return binaryOutput(previousAnswer);
                    }
                    else if (currentOperation.CompareTo("-") == 0)
                    {
                        previousAnswer = subtraction(previousNumber1, previousNumber2);
                        freshIO = true;
                        previousNumber1 = previousAnswer;
                        currentOperation = "+";
                        return binaryOutput(previousAnswer);
                    }
                }
                if (newOperation.CompareTo("-") == 0)
                {
                    if (opDone)
                    {
                        currentOperation = newOperation;
                        opDone = false;
                        freshIO = true;
                    }
                    else if (currentOperation.CompareTo("+") == 0)
                    {
                        previousAnswer = addition(previousNumber1, previousNumber2);
                        freshIO = true;
                        previousNumber1 = previousAnswer;
                        currentOperation = "-";
                        return binaryOutput(previousAnswer);
                    }
                    else if (currentOperation.CompareTo("-") == 0)
                    {
                        previousAnswer = subtraction(previousNumber1, previousNumber2);
                        freshIO = true;
                        previousNumber1 = previousAnswer;
                        currentOperation = "-";
                        return binaryOutput(previousAnswer);
                    }
                }
            }
            return ioContent;
        }

        public long? addition(long? savedNumber, long? newNumber)
        {
            try
            {
                savedNumber = checked(savedNumber + newNumber);
            }
            catch (OverflowException)
            {
                MessageBox.Show("Error: Number value of answer exceeds computational range.\nPlease clear memory (AC) and try again.");
                clearMem();
                return null;
            }
            return savedNumber;
        }

        public long? subtraction(long? savedNumber, long? newNumber)
        {
            try
            {
                savedNumber = checked(savedNumber - newNumber);
            }
            catch (OverflowException)
            {
                MessageBox.Show("Error: Number value of answer exceeds computational range.\nPlease clear memory (AC) and try again.");
                clearMem();
                return null;
            }
            return savedNumber;
        }

        public string binaryOutput(long? output)
        {
            string convResult = "";
            byte bit;
            bool isNegative = false;
            if (output < 0)
            {
                isNegative = true;
                output = (2 * -output) + output;
            }
            else if (output == 0)
            {
                return "0";
            }
            while (output > 0)
            {
                bit = (byte)(output % 2);
                output = output / 2;
                convResult = convResult.Insert(0, bit.ToString());
            }
            if (isNegative)
            {
                convResult = convResult.Insert(0, "-");
            }
            return convResult;
        }

        public void clearMem()
        {
            previousNumber1 = null;
            previousNumber2 = null;
            previousAnswer = null;
            currentOperation = "";
            freshIO = false;
        }

        public string getIoContent()
        {
            return ioContent;
        }

        public void setIoContent(string input)
        {
            ioContent = input;
        }

        public bool isFreshIO()
        {
            return freshIO;
        }

        public void refreshedIO()
        {
            freshIO = false;
        }

        public bool isOpDone()
        {
            return opDone;
        }

        public void opReady()
        {
            opDone = false;
        }
    }
}
