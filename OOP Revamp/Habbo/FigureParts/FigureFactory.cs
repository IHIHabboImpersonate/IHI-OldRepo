using System;
using System.Collections.Generic;

namespace IHI.Server.Habbos.Figure
{
    public class HabboFigureFactory
    {
        private readonly Dictionary<ushort, Type> _figureModelIDs;

        internal HabboFigureFactory()
        {
            _figureModelIDs = new Dictionary<ushort, Type>();
        }

        public HabboFigure Parse(string figureString, bool gender)
        {
            // TODO: Possible optimisation: Store unparsed string. Don't parse till needed. (Lazy parsing)

            // Create a new instance of HabboFigure to work on.
            var figureInProgress = new HabboFigure(gender);

            // Split the input string into parts of the figure.
            var partStringArray = figureString.Split(new[] {'.'});

            // For each part of the figure...
            foreach (var partString in partStringArray)
            {
                // Split it into part type, model and colours.
                var detailsArray = partString.Split(new[] {'-'});


                ushort modelID;

                // Parse the ModelID.
                if (!ushort.TryParse(detailsArray[1], out modelID))
                    throw new FormatException("Figure ModelID is not a valid ushort in '" + partString + "'.");

                // Make sure the ModelID is valid.
                if (!_figureModelIDs.ContainsKey(modelID))
                    throw new KeyNotFoundException("Figure ModelID " + modelID + " is not a valid figure part model.");

                // Create a new instance of the figure part.
                var part = _figureModelIDs[modelID]
                               .GetConstructor(new Type[0])
                               .Invoke(new object[0]) as FigurePart;

                // Was a primary colour provided?
                if (detailsArray.Length > 2)
                {
                    ushort colourID;

                    // Parse ColorID and validate it.
                    if (!ushort.TryParse(detailsArray[2], out colourID))
                        throw new FormatException("Figure ColourID is not a valid ushort in '" + partString + "'.");

                    //Set PrimaryColour for this part.
                    part.SetPrimaryColour(colourID);

                    // Was a secondary colour provided?
                    if (detailsArray.Length > 3)
                    {
                        // Parse ColourID and validate it.
                        if (!ushort.TryParse(detailsArray[3], out colourID))
                            throw new FormatException("Figure ColourID is not a valid ushort in '" + partString + "'.");

                        // Set the SecondaryColour for this part.
                        part.SetSecondaryColour(colourID);
                    }
                }


                // This part is a...
                switch (detailsArray[0])
                {
                        // Shirt
                    case "hd":
                        {
                            // Verify this model is a shirt.
                            if (!(part is Body))
                                throw new InvalidCastException("Figure ModelID " + modelID +
                                                               " is a valid figure model but not a valid body.");

                            // Apply the part to the HabboFigure
                            figureInProgress.SetBody(part as Body);
                            break;
                        }
                    default:
                        continue; // Not a valid part? Ignore it.
                }
            }

            return figureInProgress;
        }

        public HabboFigureFactory RegisterModelID(Type part)
        {
            if (part.IsSubclassOf(typeof (FigurePart)))
            {
                var modelID = (part.GetConstructors()[0].Invoke(new object[0]) as FigurePart).GetModelID();
                _figureModelIDs.Add(modelID, part);
            }
            return this;
        }

        public HabboFigureFactory UnregisterModelID(Type part)
        {
            if (part.IsSubclassOf(typeof (FigurePart)))
            {
                var modelID = (part.GetConstructors()[0].Invoke(new object[0]) as FigurePart).GetModelID();
                if (_figureModelIDs.ContainsKey(modelID))
                    _figureModelIDs.Remove(modelID);
            }
            return this;
        }
    }
}