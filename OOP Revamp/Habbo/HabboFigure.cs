using System;
using System.Collections.Generic;
using System.Data;
using IHI.Server.Networking.Messages;
using IHI.Server.Users.Permissions;
using IHI.Server.Networking;
using System.Linq;
using IHI.Database;
using NHibernate;

using IHI.Server.Rooms;

namespace IHI.Server.Habbos
{
    public class HabboFigure : IFigure
    {
        /// <summary>
        /// The gender of the user.
        /// Male = True
        /// Female = False
        /// </summary>
        private bool fGender;

        private FigureBody fBody;
        private FigureHair fHair;
        private FigureShirt fShirt;
        private FigureTrousers fTrousers;
        private FigureShoes fShoes;

        private FigureHat fHat;
        private FigureGlasses fGlasses;
        private FigureHairAccesories fHairAccesories;
        private FigureFaceAccesories fFaceAccesories;
        private FigureShirtAccesories fShirtAccesories;
        private FigureBelt fBelt;
        private FigureJacket fJacket;

        public HabboFigure(bool Gender)
        {
            this.fGender = Gender;
        }

        #region Gender
        public bool GetGender()
        {
            return this.fGender;
        }
        public char GetGenderChar()
        {
            return (this.fGender ? 'M' : 'F');
        }
        public HabboFigure SetGender(bool Gender)
        {
            this.fGender = Gender;
            return this;
        }
        #endregion

        public FigureBody GetBody()
        {
            return this.fBody;
        }
        public HabboFigure SetBody(FigureBody Value)
        {
            this.fBody = Value;
            return this;
        }

        public FigureHair GetHair()
        {
            return this.fHair;
        }
        public HabboFigure SetHair(FigureHair Value)
        {
            this.fHair = Value;
            return this;
        }

        public FigureShirt GetShirt()
        {
            return this.fShirt;
        }
        public HabboFigure SetShirt(FigureShirt Value)
        {
            this.fShirt = Value;
            return this;
        }

        public FigureTrousers GetTrousers()
        {
            return this.fTrousers;
        }
        public HabboFigure SetTrousers(FigureTrousers Value)
        {
            this.fTrousers = Value;
            return this;
        }

        public FigureShoes GetShoes()
        {
            return this.fShoes;
        }
        public HabboFigure SetShoes(FigureShoes Value)
        {
            this.fShoes = Value;
            return this;
        }


        public FigureHat GetHat()
        {
            return this.fHat;
        }
        public HabboFigure SetHat(FigureHat Value)
        {
            this.fHat = Value;
            return this;
        }

        public FigureGlasses GetGlasses()
        {
            return this.fGlasses;
        }
        public HabboFigure SetGlasses(FigureGlasses Value)
        {
            this.fGlasses = Value;
            return this;
        }

        public FigureHairAccesories GetHairAccesories()
        {
            return this.fHairAccesories;
        }
        public HabboFigure SetHairAccesories(FigureHairAccesories Value)
        {
            this.fHairAccesories = Value;
            return this;
        }

        public FigureFaceAccesories GetFaceAccesories()
        {
            return this.fFaceAccesories;
        }
        public HabboFigure SetFaceAccesories(FigureFaceAccesories Value)
        {
            this.fFaceAccesories = Value;
            return this;
        }

        public FigureShirtAccesories GetShirtAccesories()
        {
            return this.fShirtAccesories;
        }
        public HabboFigure SetShirtAccesories(FigureShirtAccesories Value)
        {
            this.fShirtAccesories = Value;
            return this;
        }

        public FigureBelt GetBelt()
        {
            return this.fBelt;
        }
        public HabboFigure SetBelt(FigureBelt Value)
        {
            this.fBelt = Value;
            return this;
        }

        public FigureJacket GetJacket()
        {
            return this.fJacket;
        }
        public HabboFigure SetJacket(FigureJacket Value)
        {
            this.fJacket = Value;
            return this;
        }


        private uint fSwimFigure;
        /// <summary>
        /// Returns a byte array containing 3 values.
        /// The values are the RGB colour values of the swim figure.
        /// </summary>
        public byte[] GetSwimFigure()
        {
            return new byte[] { (byte)(this.fSwimFigure >> 16), (byte)(this.fSwimFigure << 8 >> 16), (byte)((this.fSwimFigure << 16) >> 16) };
        }

        /// <summary>
        /// Returns a byte array containing 3 values.
        /// The values are the RGB colour values of the swim figure.
        /// </summary>
        public string GetFormattedSwimFigure()
        {
            return "ch=s0" + (this.GetGender() ? '1' : '2') + "/" + (this.fSwimFigure >> 16) + "," + (this.fSwimFigure << 8 >> 16) + "," + ((this.fSwimFigure << 16) >> 16);
        }
        /// <summary>
        /// Sets the colour of the swim figure.
        /// </summary>
        /// <param name="Red">The amount of red in the colour.</param>
        /// <param name="Green">The amount of green in the colour.</param>
        /// <param name="Blue">The amount of blue in the colour.</param>
        /// <returns></returns>
        public HabboFigure SetSwimFigure(byte Red, byte Green, byte Blue)
        {
            this.fSwimFigure = (uint)((Red << 16) | Green << 8) | Blue;
            return this;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}