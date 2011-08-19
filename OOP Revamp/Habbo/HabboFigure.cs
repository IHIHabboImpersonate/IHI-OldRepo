using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using IHI.Server.Networking.Messages;
using IHI.Server.Users.Permissions;
using IHI.Server.Networking;
using System.Linq;
using IHI.Database;
using NHibernate;

using IHI.Server.Rooms;
using IHI.Server.Habbos.Figure;

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

        private Body fBody;
        private Hair fHair;
        private Shirt fShirt;
        private Legs fLegs;
        private Shoes fShoes;

        private Hat fHat;
        private EyeAccessory fEyeAccessory;
        private HeadAccessory fHeadAccessory;
        private FaceAccessory fFaceAccessory;
        private ShirtAccessory fShirtAccessory;
        private WaistAccessory fWaistAccessory;
        private Jacket fJacket;

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

        public Body GetBody()
        {
            return this.fBody;
        }
        public HabboFigure SetBody(Body Value)
        {
            this.fBody = Value;
            return this;
        }

        public Hair GetHair()
        {
            return this.fHair;
        }
        public HabboFigure SetHair(Hair Value)
        {
            this.fHair = Value;
            return this;
        }

        public Shirt GetShirt()
        {
            return this.fShirt;
        }
        public HabboFigure SetShirt(Shirt Value)
        {
            this.fShirt = Value;
            return this;
        }

        public Legs GeLegs()
        {
            return this.fLegs;
        }
        public HabboFigure SetLegs(Legs Value)
        {
            this.fLegs = Value;
            return this;
        }

        public Shoes GetShoes()
        {
            return this.fShoes;
        }
        public HabboFigure SetShoes(Shoes Value)
        {
            this.fShoes = Value;
            return this;
        }


        public Hat GetHat()
        {
            return this.fHat;
        }
        public HabboFigure SetHat(Hat Value)
        {
            this.fHat = Value;
            return this;
        }

        public EyeAccessory GetEyeAccessory()
        {
            return this.fEyeAccessory;
        }
        public HabboFigure SetEyeAccessory(EyeAccessory Value)
        {
            this.fEyeAccessory = Value;
            return this;
        }

        public HeadAccessory GetHeadAccessory()
        {
            return this.fHeadAccessory;
        }
        public HabboFigure SetHeadAccessory(HeadAccessory Value)
        {
            this.fHeadAccessory = Value;
            return this;
        }

        public FaceAccessory GetFaceAccesory()
        {
            return this.fFaceAccessory;
        }
        public HabboFigure SetFaceAccessory(FaceAccessory Value)
        {
            this.fFaceAccessory = Value;
            return this;
        }

        public ShirtAccessory GetShirtAccessory()
        {
            return this.fShirtAccessory;
        }
        public HabboFigure SetShirtAccessories(ShirtAccessory Value)
        {
            this.fShirtAccessory = Value;
            return this;
        }

        public WaistAccessory GetWaistAccessory()
        {
            return this.fWaistAccessory;
        }
        public HabboFigure SetWaistAccessory(WaistAccessory Value)
        {
            this.fWaistAccessory = Value;
            return this;
        }

        public Jacket GetJacket()
        {
            return this.fJacket;
        }
        public HabboFigure SetJacket(Jacket Value)
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
            StringBuilder SB = new StringBuilder();

            bool PrefixRequired = false;

            if (this.fBody != null)
            {
                SB.Append(this.fBody.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fEyeAccessory != null)
            {
                SB.Append(this.fEyeAccessory.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fFaceAccessory != null)
            {
                SB.Append(this.fFaceAccessory.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fHair != null)
            {
                SB.Append(this.fHair.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fHat != null)
            {
                SB.Append(this.fHat.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fHeadAccessory != null)
            {
                SB.Append(this.fHeadAccessory.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fJacket != null)
            {
                SB.Append(this.fJacket.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fLegs != null)
            {
                SB.Append(this.fLegs.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fShirt != null)
            {
                SB.Append(this.fShirt.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fShirtAccessory != null)
            {
                SB.Append(this.fShirtAccessory.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fShoes != null)
            {
                SB.Append(this.fShoes.ToString(PrefixRequired));
                PrefixRequired = true;
            }
            if (this.fWaistAccessory != null)
            {
                SB.Append(this.fWaistAccessory.ToString(PrefixRequired));
                PrefixRequired = true;
            }

            return SB.ToString();
        }
    }
}