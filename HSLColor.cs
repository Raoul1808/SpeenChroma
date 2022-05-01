namespace SpeenChroma
{
    public struct HSLColor
    {
        public float H;
        public float S;
        public float L;

        public HSLColor(float h, float s, float l)
        {
            H = h;
            S = s;
            L = l;
        }

        public static HSLColor White => new HSLColor(1f, 1f, 1f);
    }
}
