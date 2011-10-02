using System.Collections.Generic;
using System.Text;

namespace IHI.Server.Install
{
    public abstract class Step
    {
        protected string Description;
        protected ICollection<string> Examples;
        protected string Title;

        public Step SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public Step SetDescription(string description)
        {
            Description = description;
            return this;
        }

        public Step AddExample(string example)
        {
            Examples.Add(example);
            return this;
        }

        public Step RemoveExample(string example)
        {
            Examples.Remove(example);
            return this;
        }

        protected string ToString(string defaultValue)
        {
            var outputBuilder = new StringBuilder();

            outputBuilder.
                Append(">>>   ").
                Append(Title).
                AppendLine("   <<<").
                AppendLine().
                AppendLine(Description).
                Append("Examples: ");

            var firstExample = true;
            foreach (var example in Examples)
            {
                if (!firstExample)
                    outputBuilder.Append("          ");
                else
                    firstExample = false;
                outputBuilder.AppendLine(example);
            }

            outputBuilder.
                AppendLine().
                Append("Default Value: ").
                AppendLine(defaultValue).
                AppendLine().
                Append("=> ");

            return outputBuilder.ToString();
        }

        public abstract object Run();
    }
}