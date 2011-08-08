using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server.Habbos
{
    public class HabboFigureFactory
    {
        Dictionary<ushort, Type> fFigureModelIDs;

        internal HabboFigureFactory()
        {
            this.fFigureModelIDs = new Dictionary<ushort, Type>();
        }

        public HabboFigure Parse(string FigureString, bool Gender)
        {
            // TODO: Possible optimisation: Store unparsed string. Don't parse till needed.

            // Create a new instance of HabboFigure to work on.
            HabboFigure FigureInProgress = new HabboFigure(Gender);

            // Split the input string into parts of the figure.
            string[] PartStringArray = FigureString.Split(new char[] { '.' });

            // For each part of the figure...
            foreach (string PartString in PartStringArray)
            {
                // Split it into part type, model and colours.
                string[] DetailsArray = PartString.Split(new char[] { '-' });

                
                ushort ModelID = 0;

                // Parse the ModelID.
                if (!ushort.TryParse(DetailsArray[1], out ModelID))
                    throw new FormatException("Figure ModelID is not a valid ushort in '" + PartString + "'.");

                // Make sure the ModelID is valid.
                if (!this.fFigureModelIDs.ContainsKey(ModelID))
                    throw new KeyNotFoundException("Figure ModelID " + ModelID + " is not a valid figure part model.");

                // Create a new instance of the figure part.
                FigurePart Part =   this.fFigureModelIDs[ModelID]
                                        .GetConstructor(new Type[0])
                                            .Invoke(new object[0]) as FigurePart;

                // Was a primary colour provided?
                if (DetailsArray.Length > 2)
                {
                    ushort ColourID = 0;

                    // Parse ColorID and validate it.
                    if (!ushort.TryParse(DetailsArray[2], out ColourID))
                        throw new FormatException("Figure ColourID is not a valid ushort in '" + PartString + "'.");
                    
                    //Set PrimaryColour for this part.
                    Part.SetPrimaryColour(ColourID);

                    // Was a secondary colour provided?
                    if (DetailsArray.Length > 3)
                    {
                        // Parse ColourID and validate it.
                        if (!ushort.TryParse(DetailsArray[3], out ColourID))
                            throw new FormatException("Figure ColourID is not a valid ushort in '" + PartString + "'.");

                        // Set the SecondaryColour for this part.
                        Part.SetSecondaryColour(ColourID);
                    }
                }


                // This part is a...
                switch (DetailsArray[0])
                {
                    // Shirt
                    case "ch":
                        {
                            // Verify this model is a shirt.
                            if (!(Part is FigureShirt))
                                throw new InvalidCastException("Figure ModelID " + ModelID + " is a valid figure model but not a valid shirt.");
                            
                            // Apply the part to the HabboFigure
                            FigureInProgress.SetShirt(Part as FigureShirt);
                            break;
                        }
                    default:
                            continue; // Not a valid part? Ignore it.
                }
            }

            return FigureInProgress;
        }

        public HabboFigureFactory RegisterModelID(Type Part)
        {
            if (Part.IsSubclassOf(typeof(FigurePart)))
            {
                ushort ModelID = (Part.GetConstructors()[0].Invoke(new object[0]) as FigurePart).GetModelID();
                this.fFigureModelIDs.Add(ModelID, Part);
            }
            return this;
        }
        public HabboFigureFactory UnregisterModelID(Type Part)
        {
            if (Part.IsSubclassOf(typeof(FigurePart)))
            {
                ushort ModelID = (Part.GetConstructors()[0].Invoke(new object[0]) as FigurePart).GetModelID();
                if (this.fFigureModelIDs.ContainsKey(ModelID))
                    this.fFigureModelIDs.Remove(ModelID);
            }
            return this;
        }
    }
}
