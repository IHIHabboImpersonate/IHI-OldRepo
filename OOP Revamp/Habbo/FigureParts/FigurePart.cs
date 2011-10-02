using System.Text;

namespace IHI.Server.Habbos.Figure
{
    public abstract class FigurePart
    {
        private ushort _primaryColour;
        private ushort _secondaryColour;

        public ushort GetPrimaryColour()
        {
            return _primaryColour;
        }

        public ushort GetSecondaryColour()
        {
            return _secondaryColour;
        }

        public FigurePart SetPrimaryColour(ushort colour)
        {
            _primaryColour = colour;
            return this;
        }

        public FigurePart SetSecondaryColour(ushort colour)
        {
            _secondaryColour = colour;
            return this;
        }


        public abstract ushort GetModelID();

        public byte GetAmountOfColours()
        {
            if (_primaryColour == 0)
                return 0;
            if (_secondaryColour == 0)
                return 1;
            return 2;
        }

        public string ToString(bool prefixRequired)
        {
            var sb = new StringBuilder();

            sb.Append(prefixRequired ? ".hd-" : "hd-");


            sb.Append(GetModelID());

            if (_primaryColour != 0)
            {
                sb.Append('-');
                sb.Append(_primaryColour);

                if (_secondaryColour != 0)
                {
                    sb.Append('-');
                    sb.Append(_secondaryColour);
                }
            }

            return sb.ToString();
        }
    }
}