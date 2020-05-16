using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UnoPlatformApp1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // 計算
        public Calculator Calc { get; private set; }

        public MainPage()
        {
            Calc = new Calculator();

            this.InitializeComponent();
        }

        private void UpdateAnswerText()
        {
            Text_Answer.Text = Calc.AnswerNumber.ToHexString() + Calc.InputSymbol.ToSymbolString();
            Text_Input.Text = Calc.IsInputingNumber ? Calc.InputNumber.ToHexString() : "";
        }

        private void PushNumber(object sender, RoutedEventArgs e)
        {
            var number = int.Parse((sender as Button).Content as string, NumberStyles.HexNumber);

            Calc.PushNumber(number);

            UpdateAnswerText();
        }

        private void AllClear(object sender, RoutedEventArgs e)
        {
            Calc.AllClear();
            UpdateAnswerText();
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            Calc.Clear();
            UpdateAnswerText();
        }

        private void ChangeSign(object sender, RoutedEventArgs e)
        {
            Calc.InputNumber *= -1;
            UpdateAnswerText();
        }

        private void ChangeFract(object sender, RoutedEventArgs e)
        {
            Calc.IsInputingFract = true;
        }

        private void PushCalc(object sender, RoutedEventArgs e)
        {
            Calc.Calculate();
            Calc.InputSymbol = Extensions.ParseToCalculation2Symbol((sender as Button).Content as string);
            UpdateAnswerText();
        }

        private void AdaptCalc(object sender, RoutedEventArgs e)
        {
            var symbol = Extensions.ParseToCalculation1Symbol((sender as Button).Content as string);
            Calc.AdaptCalc(symbol);
            UpdateAnswerText();
        }
    }

    // 計算
    public class Calculator
    {
        // 答えの数
        public double AnswerNumber { get; private set; } = 0.0;

        // 入力している数
        private double inputNumber = 0.0;
        public double InputNumber
        {
            get => inputNumber;
            set { inputNumber = value; IsInputingNumber = true; }
        }

        // 小数点以下の桁
        public int InputFractDigit { get; private set; } = 0;

        // 小数点以下入力中か
        public bool IsInputingFract
        {
            get => InputFractDigit > 0;
            set
            {
                if (value && InputFractDigit == 0)
                {
                    InputFractDigit = 1;
                }
                else if (!value && InputFractDigit > 0)
                {
                    InputFractDigit = 0;
                }
            }
        }

        // 入力している演算
        public Calculation2Symbol InputSymbol { get; set; } = Calculation2Symbol.None;

        public bool IsInputingNumber { get; private set; } = false;

        public void AllClear()
        {
            AnswerNumber = 0.0;
            InputNumber = 0.0;
            IsInputingFract = false;
            InputSymbol = Calculation2Symbol.None;
        }

        public void Clear()
        {
            InputNumber = 0.0;
            IsInputingFract = false;
            IsInputingNumber = false;
            InputSymbol = Calculation2Symbol.None;
        }

        public void PushNumber(int number)
        {
            if (IsInputingFract)
            {
                InputNumber += number * Math.Pow(16, -InputFractDigit) * (InputNumber >= 0 ? 1 : -1);
                InputFractDigit++;
            }
            else
            {
                InputNumber *= 16.0;
                InputNumber += number * (InputNumber >= 0 ? 1 : -1);
            }
        }

        public void Calculate()
        {
            if (IsInputingNumber)
            {
                switch (InputSymbol)
                {
                    case Calculation2Symbol.None:
                        AnswerNumber = InputNumber;
                        break;

                    case Calculation2Symbol.Add:
                        AnswerNumber += InputNumber;
                        break;

                    case Calculation2Symbol.Minus:
                        AnswerNumber -= InputNumber;
                        break;

                    case Calculation2Symbol.Multi:
                        AnswerNumber *= InputNumber;
                        break;

                    case Calculation2Symbol.Div:
                        AnswerNumber /= InputNumber;
                        break;
                }
            }

            InputNumber = 0.0;
            IsInputingNumber = false;
            IsInputingFract = false;
        }

        public void AdaptCalc(Calculation1Symbol symbol)
        {
            if (IsInputingNumber)
            {
                switch (symbol)
                {
                    case Calculation1Symbol.None:
                        break;

                    case Calculation1Symbol.Inverse:
                        InputNumber = 1.0 / InputNumber;
                        break;

                    case Calculation1Symbol.Square:
                        InputNumber *= InputNumber;
                        break;

                    case Calculation1Symbol.SquareRoot:
                        InputNumber = Math.Sqrt(InputNumber);
                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch (symbol)
                {
                    case Calculation1Symbol.None:
                        break;

                    case Calculation1Symbol.Inverse:
                        AnswerNumber = 1.0 / AnswerNumber;
                        break;

                    case Calculation1Symbol.Square:
                        AnswerNumber *= AnswerNumber;
                        break;

                    case Calculation1Symbol.SquareRoot:
                        AnswerNumber = Math.Sqrt(AnswerNumber);
                        break;

                    default:
                        break;
                }
            }
        }
    }

    // 単項演算の文字
    public enum Calculation1Symbol
    {
        None,
        Inverse,
        Square,
        SquareRoot,
    }

    // 1項演算の文字
    public enum Calculation2Symbol
    {
        None,
        Add,
        Minus,
        Multi,
        Div,
    }

    public static class Extensions
    {
        // numberを16進数の文字列に変換する
        public static string ToHexString(this double @this, int maxFractDigit = -1)
        {
            var s = new StringBuilder();

            if (double.IsNaN(@this))
            {
                return "Nan";
            }

            if (double.IsInfinity(@this))
            {
                return "Inf";
            }

            // 整数部分と小数部分で分ける
            long integral = (long)Math.Truncate(Math.Abs(@this));
            double fractional = Math.Abs(@this) % 1.0;

            if (@this < 0.0)
            {
                s.Append("-");
            }

            // 整数部分を16進数表記に変更し、追加する
            s.Append(integral.ToString("X"));

            // 小数部分を16進数表記に変更する
            if (fractional != 0.0)
            {
                s.Append(".");

                for (int i = 0; (maxFractDigit == -1 || i < maxFractDigit) && fractional != 0.0; ++i)
                {
                    fractional *= 16.0;

                    // 溢れた部分（整数部分）を取り除く
                    var over = (long)Math.Truncate(fractional);
                    fractional %= 1.0;

                    // 溢れた部分を16進数表記にして追加する
                    s.Append(over.ToString("X"));
                }
            }

            return s.ToString();
        }

        public static string ToSymbolString(this Calculation2Symbol @this)
        {
            switch (@this)
            {
                case Calculation2Symbol.None:
                    return "";

                case Calculation2Symbol.Add:
                    return "+";

                case Calculation2Symbol.Minus:
                    return "-";

                case Calculation2Symbol.Multi:
                    return "×";

                case Calculation2Symbol.Div:
                    return "÷";

                default:
                    return "_";
            }
        }

        public static Calculation1Symbol ParseToCalculation1Symbol(string text)
        {
            switch (text)
            {
                case "":
                    return Calculation1Symbol.None;

                case "1/x":
                    return Calculation1Symbol.Inverse;

                case "x^2":
                case "x²":
                    return Calculation1Symbol.Square;

                case "√x":
                    return Calculation1Symbol.SquareRoot;

                default:
                    return Calculation1Symbol.None;
            }
        }

        public static Calculation2Symbol ParseToCalculation2Symbol(string text)
        {
            switch (text)
            {
                case "":
                case "=":
                    return Calculation2Symbol.None;

                case "+":
                    return Calculation2Symbol.Add;

                case "-":
                    return Calculation2Symbol.Minus;

                case "*":
                case "×":
                    return Calculation2Symbol.Multi;

                case "/":
                case "÷":
                    return Calculation2Symbol.Div;

                default:
                    return Calculation2Symbol.None;
            }
        }
    }
}
