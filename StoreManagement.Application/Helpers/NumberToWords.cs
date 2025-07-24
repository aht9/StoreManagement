namespace StoreManagement.Application.Helpers;

public static class NumberToWords
    {
        private static readonly string[] Yekan = { "صفر", "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه" };
        private static readonly string[] Dahgan = { "", "", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود" };
        private static readonly string[] Dahyek = { "ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده" };
        private static readonly string[] Sadgan = { "", "یکصد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد" };
        private static readonly string[] Basse = { "", "هزار", "میلیون", "میلیارد" };

        private static string GetNum3(int num3)
        {
            var s = "";
            int d3, d12;
            d12 = num3 % 100;
            d3 = num3 / 100;
            if (d3 != 0)
                s = Sadgan[d3] + " و ";
            if ((d12 >= 10) && (d12 <= 19))
            {
                s = s + Dahyek[d12 - 10];
            }
            else
            {
                int d2 = d12 / 10;
                if (d2 != 0)
                    s = s + Dahgan[d2] + " و ";
                int d1 = d12 % 10;
                if (d1 != 0)
                    s = s + Yekan[d1] + " و ";
                s = s.Substring(0, s.Length - 3);
            }
            return s;
        }

        public static string Convert(decimal number)
        {
            var st = number.ToString("0");
            if (st == "0") return Yekan[0];
            var st_len = st.Length;
            var num = 0;
            var s = "";
            while (st_len > 0)
            {
                var len = (st_len % 3 == 0) ? 3 : st_len % 3;
                var temp = st.Substring(0, len);
                st = st.Substring(len);
                st_len = st.Length;
                if (temp == "000") continue;
                s = s + GetNum3(int.Parse(temp)) + " " + Basse[st_len / 3] + " و ";
                num++;
            }
            return s.Substring(0, s.Length - 3);
        }
    }