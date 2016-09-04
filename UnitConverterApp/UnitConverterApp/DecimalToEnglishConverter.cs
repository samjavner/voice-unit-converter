using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitConverterApp
{
    public class DecimalToEnglishConverter
    {
        public string Convert(decimal value)
        {
            var isNegative = value < 0;
            var reversed = new Stack<char>(Math.Abs(value).ToString("F99").ToCharArray());
            var digits = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
            var groupings = new string[] { "", "thousand", "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion", "nonillion", "decillion" };
            var words = new Stack<string>();
            char digit;
            int grouping = 0;

            if (reversed.Contains('.'))
            {
                digit = reversed.Pop();
                while (digit != '.')
                {
                    var digitValue = int.Parse(digit.ToString());
                    if (digitValue > 0 || words.Count > 0)
                    {
                        words.Push(digits[digitValue]);
                    }
                    digit = reversed.Pop();
                }
                if (words.Count > 0)
                {
                    words.Push("point");
                }
            }

            while (reversed.Count > 2)
            {
                var ones = int.Parse(reversed.Pop().ToString());
                var tens = int.Parse(reversed.Pop().ToString());
                var hundreds = int.Parse(reversed.Pop().ToString());
                var word = ConvertNumberToText(hundreds, tens, ones);
                if (word.Length > 0)
                {
                    if (grouping > 0)
                    {
                        words.Push(groupings[grouping]);
                    }
                    words.Push(word);
                }
                grouping++;
            }

            if (reversed.Count == 2)
            {
                var ones = int.Parse(reversed.Pop().ToString());
                var tens = int.Parse(reversed.Pop().ToString());
                if (grouping > 0)
                {
                    words.Push(groupings[grouping]);
                }
                words.Push(ConvertNumberToText(0, tens, ones));
            }
            else if (reversed.Count == 1)
            {
                var ones = int.Parse(reversed.Pop().ToString());

                if (grouping > 0)
                {
                    words.Push(groupings[grouping]);
                }

                if (grouping == 0 && ones == 0)
                {
                    words.Push(digits[ones]);
                }
                else
                {
                    words.Push(ConvertNumberToText(0, 0, ones));
                }

            }

            if (isNegative)
            {
                words.Push("negative");
            }

            return string.Join(" ", words);
        }

        private string ConvertNumberToText(int hundreds, int tens, int ones)
        {
            var digits = new string[] { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
            var teens = new string[] { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var x10s = new string[] { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            var words = new List<string>();

            if (hundreds > 0)
            {
                words.Add(digits[hundreds]);
                words.Add("hundred");
            }

            if (tens == 1)
            {
                words.Add(teens[ones]);
            }
            else
            {
                if (tens > 1)
                {
                    words.Add(x10s[tens]);
                }
                if (ones > 0)
                {
                    words.Add(digits[ones]);
                }
            }

            return string.Join(" ", words);
        }
    }
}
